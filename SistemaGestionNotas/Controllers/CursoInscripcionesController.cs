using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaGestionNotas.Data;
using SistemaGestionNotas.Models.ViewModels;
using SistemaGestionNotas.Models;

namespace SistemaGestionNotas.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class CursoInscripcionesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CursoInscripcionesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var cursosConAlumnos = await _context.Cursos
                .Where(c => !c.CursoEliminado && c.EstadoCurso == "Activo")
                .OrderBy(c => c.Nombre)
                .Select(c => new CursoConAlumnosViewModel
                {
                    CursoId = c.CursoId,
                    Codigo = c.Codigo,
                    Nombre = c.Nombre,
                    Seccion = c.Seccion,
                    NombresAlumnos = c.CursoInscripciones
                                        .Where(ci => !ci.InscripcionEliminada && !ci.Alumno.AlumnoEliminado)
                                        .Select(ci => ci.Alumno.Nombre)
                                        .OrderBy(nombre => nombre)
                                        .ToList()
                })
                .ToListAsync();

            return View(cursosConAlumnos);
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var curso = await _context.Cursos.FindAsync(id);
            if (curso == null || curso.CursoEliminado)
            {
                return NotFound();
            }

            var todosLosAlumnos = await _context.Alumnos
                .Where(a => !a.AlumnoEliminado && a.EstadoAlumno == "Activo")
                .OrderBy(a => a.Nombre)
                .ToListAsync();

            var alumnosInscritosIds = new HashSet<Guid>(await _context.CursoInscripciones
                .Where(ci => ci.CursoId == id && !ci.InscripcionEliminada)
                .Select(ci => ci.AlumnoId)
                .ToListAsync());

            var viewModel = new AsignarAlumnosViewModel
            {
                CursoId = curso.CursoId,
                NombreCurso = curso.Nombre,
                Alumnos = todosLosAlumnos.Select(alumno => new AlumnoAsignacionViewModel
                {
                    AlumnoId = alumno.AlumnoId,
                    Nombre = alumno.Nombre,
                    EstaAsignado = alumnosInscritosIds.Contains(alumno.AlumnoId)
                }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AsignarAlumnosViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                var curso = await _context.Cursos.FindAsync(viewModel.CursoId);
                var todosLosAlumnos = await _context.Alumnos
                    .Where(a => !a.AlumnoEliminado && a.EstadoAlumno == "Activo")
                    .OrderBy(a => a.Nombre)
                    .ToListAsync();

                viewModel.NombreCurso = curso?.Nombre ?? "Curso Desconocido";
                viewModel.Alumnos = todosLosAlumnos.Select(alumno => new AlumnoAsignacionViewModel
                {
                    AlumnoId = alumno.AlumnoId,
                    Nombre = alumno.Nombre,
                    EstaAsignado = viewModel.Alumnos.Any(a => a.AlumnoId == alumno.AlumnoId && a.EstaAsignado)
                }).ToList();

                if (!ModelState.Values.Any(v => v.Errors.Any()))
                {
                    ModelState.AddModelError(string.Empty, "Hubo un error de validación en los datos. Revise los campos.");
                }

                return View(viewModel);
            }

            try
            {
                var inscripcionesActuales = await _context.CursoInscripciones
                    .Where(ci => ci.CursoId == viewModel.CursoId)
                    .ToListAsync();

                foreach (var alumnoViewModel in viewModel.Alumnos)
                {
                    var inscripcionExistente = inscripcionesActuales
                        .FirstOrDefault(i => i.AlumnoId == alumnoViewModel.AlumnoId);

                    if (alumnoViewModel.EstaAsignado)
                    {
                        if (inscripcionExistente == null)
                        {
                            _context.CursoInscripciones.Add(new CursoInscripcion
                            {
                                CursoId = viewModel.CursoId,
                                AlumnoId = alumnoViewModel.AlumnoId,
                                FechaInscripcion = DateTime.Now,
                                InscripcionEliminada = false
                            });
                        }
                        else if (inscripcionExistente.InscripcionEliminada)
                        {
                            inscripcionExistente.InscripcionEliminada = false;
                            _context.Update(inscripcionExistente);
                        }
                    }
                    else
                    {
                        if (inscripcionExistente != null && !inscripcionExistente.InscripcionEliminada)
                        {
                            inscripcionExistente.InscripcionEliminada = true;
                            _context.Update(inscripcionExistente);
                        }
                    }
                }

                await _context.SaveChangesAsync();
                TempData["success"] = "Inscripciones actualizadas correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["error"] = "Ocurrió un error al guardar los cambios: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var curso = await _context.Cursos
                .FirstOrDefaultAsync(c => c.CursoId == id && !c.CursoEliminado);

            if (curso == null) return NotFound();

            var inscripciones = await _context.CursoInscripciones
                .Where(ci => ci.CursoId == id && !ci.InscripcionEliminada)
                .Include(ci => ci.Alumno) 
                .Where(ci => !ci.Alumno.AlumnoEliminado)
                .OrderBy(ci => ci.Alumno.Nombre)
                .ToListAsync();

            var viewModel = new CursoInscripcionesDetailsViewModel
            {
                CursoId = curso.CursoId,
                NombreCurso = curso.Nombre,
                CodigoCurso = curso.Codigo,
                Seccion = curso.Seccion,
                AlumnosInscritos = inscripciones 
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> PrintDetails(Guid id)
        {
            var curso = await _context.Cursos
                .FirstOrDefaultAsync(c => c.CursoId == id && !c.CursoEliminado);

            if (curso == null) return NotFound();

            var inscripciones = await _context.CursoInscripciones
                .Where(ci => ci.CursoId == id && !ci.InscripcionEliminada)
                .Include(ci => ci.Alumno) 
                .Where(ci => !ci.Alumno.AlumnoEliminado)
                .OrderBy(ci => ci.Alumno.Nombre)
                .ToListAsync();

            var viewModel = new CursoInscripcionesDetailsViewModel
            {
                CursoId = curso.CursoId,
                NombreCurso = curso.Nombre,
                CodigoCurso = curso.Codigo,
                Seccion = curso.Seccion,
                AlumnosInscritos = inscripciones
            };

            return View("PrintDetails", viewModel);
        }
    }
}