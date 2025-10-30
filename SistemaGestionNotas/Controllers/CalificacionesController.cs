using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaGestionNotas.Data;
using SistemaGestionNotas.Models;
using SistemaGestionNotas.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SistemaGestionNotas.Controllers
{
    [Authorize]
    public class CalificacionesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<UsuarioAplicacion> _userManager;

        public CalificacionesController(ApplicationDbContext context, UserManager<UsuarioAplicacion> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // --- MÉTODO INDEX 
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Challenge();
            }

            var cursosParaMostrar = new List<ProfesorCursoViewModel>();

            // LÓGICA PARA EL ADMINISTRADOR
            if (User.IsInRole("Administrador"))
            {
                ViewData["Title"] = "Gestion de Calificaciones";
                cursosParaMostrar = await _context.Cursos
                    .Include(c => c.Carrera).Include(c => c.Profesor)
                    .Where(c => !c.CursoEliminado && c.EstadoCurso == "Activo")
                    .Select(c => new ProfesorCursoViewModel
                    {
                        CursoId = c.CursoId,
                        CodigoCurso = c.Codigo,
                        NombreCurso = c.Nombre,
                        Seccion = c.Seccion,
                        NombreCarrera = c.Carrera.Nombre,
                        Ciclo = c.Ciclo,
                        NombreProfesor = c.Profesor.Nombre
                    }).ToListAsync();
            }
            // LÓGICA PARA EL PROFESOR
            else if (User.IsInRole("Profesor"))
            {
                ViewData["Title"] = "Gestión de Calificaciones de Mis Cursos Asignados";
                var profesor = await _context.Profesores.FirstOrDefaultAsync(p => p.UsuarioId == userId);
                if (profesor != null)
                {
                    cursosParaMostrar = await _context.Cursos
                        .Include(c => c.Carrera).Include(c => c.Profesor)
                        .Where(c => c.ProfesorId == profesor.ProfesorId && !c.CursoEliminado && c.EstadoCurso == "Activo")
                        .Select(c => new ProfesorCursoViewModel
                        {
                            CursoId = c.CursoId,
                            CodigoCurso = c.Codigo,
                            NombreCurso = c.Nombre,
                            Seccion = c.Seccion,
                            NombreCarrera = c.Carrera.Nombre,
                            Ciclo = c.Ciclo,
                            NombreProfesor = c.Profesor.Nombre
                        }).ToListAsync();
                }
            }
            // LÓGICA PARA EL ALUMNO
            else if (User.IsInRole("Alumno"))
            {
                ViewData["Title"] = "Calificaciones de Los Cursos a los que estoy Inscrito";
                var alumno = await _context.Alumnos.FirstOrDefaultAsync(a => a.UsuarioId == userId);
                if (alumno != null)
                {
                    var idsDeCursosInscritos = await _context.CursoInscripciones
                        .Where(ci => ci.AlumnoId == alumno.AlumnoId && !ci.InscripcionEliminada)
                        .Select(ci => ci.CursoId).ToListAsync();

                    if (idsDeCursosInscritos.Any())
                    {
                        cursosParaMostrar = await _context.Cursos
                            .Include(c => c.Carrera).Include(c => c.Profesor)
                            .Where(c => idsDeCursosInscritos.Contains(c.CursoId) && !c.CursoEliminado && c.EstadoCurso == "Activo")
                            .Select(c => new ProfesorCursoViewModel
                            {
                                CursoId = c.CursoId,
                                CodigoCurso = c.Codigo,
                                NombreCurso = c.Nombre,
                                Seccion = c.Seccion,
                                NombreCarrera = c.Carrera.Nombre,
                                Ciclo = c.Ciclo,
                                NombreProfesor = c.Profesor.Nombre
                            }).ToListAsync();
                    }
                }
            }
            return View(cursosParaMostrar);
        }

        public async Task<IActionResult> DetallesCurso(Guid cursoId)
        {
            if (cursoId == Guid.Empty) return BadRequest();
            var curso = await _context.Cursos.FindAsync(cursoId);
            if (curso == null) return NotFound();

            var viewModel = new DetallesCursoViewModel
            {
                CursoId = curso.CursoId,
                NombreCurso = curso.Nombre,
                EsInsolvente = false,
                Alumnos = new List<DetalleAlumnoViewModel>()
            };

            List<Alumno> alumnosAMostrar = new List<Alumno>();

            // Si el usuario es un Alumno
            if (User.IsInRole("Alumno"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var alumnoActual = await _context.Alumnos.FirstOrDefaultAsync(a => a.UsuarioId == userId);

                if (alumnoActual != null)
                {
                    
                    if (alumnoActual.FechaFinSolvencia.HasValue && alumnoActual.FechaFinSolvencia.Value.Date < DateTime.Now.Date)
                    {
                        
                        viewModel.EsInsolvente = true;

                       
                        return View(viewModel);
                    }
                   
                    alumnosAMostrar = await _context.Alumnos
                        .Where(a => a.AlumnoId == alumnoActual.AlumnoId && !a.AlumnoEliminado)
                        .ToListAsync();
                }
            }
            // Si es Administrador o Profesor (o Alumno solvente)
            else
            {
                alumnosAMostrar = await _context.CursoInscripciones
                    .Include(i => i.Alumno)
                    .Where(i => i.CursoId == cursoId && !i.Alumno.AlumnoEliminado)
                    .Select(i => i.Alumno)
                    .OrderBy(a => a.Nombre)
                    .ToListAsync();
            }

            
            foreach (var alumno in alumnosAMostrar)
            {
                var calificacion = await _context.Calificaciones
                    .FirstOrDefaultAsync(c => c.AlumnoId == alumno.AlumnoId && c.CursoId == cursoId);

                viewModel.Alumnos.Add(new DetalleAlumnoViewModel
                {
                    NombreAlumno = alumno.Nombre,
                    NotaParcialUno = calificacion?.NotaParcialUno ?? 0,
                    NotaParcialDos = calificacion?.NotaParcialDos ?? 0,
                    NotaZona = calificacion?.NotaZona ?? 0,
                    NotaExamen = calificacion?.NotaExamen ?? 0,
                    NotaFinal = calificacion?.NotaFinal ?? 0
                });
            }

            return View(viewModel);
        }
       
        public async Task<IActionResult> CargarNotasPorCurso(Guid cursoId)
        {
            if (cursoId == Guid.Empty) return BadRequest();
            var curso = await _context.Cursos.FindAsync(cursoId);
            if (curso == null) return NotFound();

            var viewModel = new IngresoCalificacionesViewModel
            {
                CursoId = curso.CursoId,
                NombreCurso = curso.Nombre
            };

            var alumnosInscritos = await _context.CursoInscripciones
                .Include(i => i.Alumno).Where(i => i.CursoId == cursoId && !i.Alumno.AlumnoEliminado)
                .Select(i => i.Alumno).ToListAsync();

            foreach (var alumno in alumnosInscritos)
            {
                var calificacionExistente = await _context.Calificaciones
                    .FirstOrDefaultAsync(c => c.AlumnoId == alumno.AlumnoId && c.CursoId == cursoId);

                viewModel.CalificacionesAlumnos.Add(new CalificacionAlumnoViewModel
                {
                    CalificacionId = calificacionExistente?.CalificacionId,
                    AlumnoId = alumno.AlumnoId,
                    NombreAlumno = alumno.Nombre,
                    NotaParcialUno = calificacionExistente?.NotaParcialUno ?? 0,
                    NotaParcialDos = calificacionExistente?.NotaParcialDos ?? 0,
                    NotaZona = calificacionExistente?.NotaZona ?? 0,
                    NotaExamen = calificacionExistente?.NotaExamen ?? 0
                });
            }

            return View("IngresoNotasGrid", viewModel);
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarNotasPorCurso(IngresoCalificacionesViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("IngresoNotasGrid", model);
            }

            foreach (var item in model.CalificacionesAlumnos)
            {
                if (item.CalificacionId.HasValue && item.CalificacionId != Guid.Empty)
                {
                    var calificacion = await _context.Calificaciones.FindAsync(item.CalificacionId.Value);
                    if (calificacion != null)
                    {
                        calificacion.NotaParcialUno = item.NotaParcialUno; calificacion.NotaParcialDos = item.NotaParcialDos;
                        calificacion.NotaZona = item.NotaZona; calificacion.NotaExamen = item.NotaExamen;
                        calificacion.NotaFinal = CalcularNotaFinal(item.NotaParcialUno, item.NotaParcialDos, item.NotaZona, item.NotaExamen);
                        _context.Update(calificacion);
                    }
                }
                else
                {
                    var nuevaCalificacion = new Calificacion
                    {
                        CursoId = model.CursoId,
                        AlumnoId = item.AlumnoId,
                        NotaParcialUno = item.NotaParcialUno,
                        NotaParcialDos = item.NotaParcialDos,
                        NotaZona = item.NotaZona,
                        NotaExamen = item.NotaExamen,
                        NotaFinal = CalcularNotaFinal(item.NotaParcialUno, item.NotaParcialDos, item.NotaZona, item.NotaExamen)
                    };
                    _context.Add(nuevaCalificacion);
                }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Calificaciones guardadas exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        private int CalcularNotaFinal(int p1, int p2, int zona, int examen)
        {
            return p1 + p2 + zona + examen;
        }



        // GET: Calificaciones/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null) return NotFound();
            var calificacion = await _context.Calificaciones
                .Include(c => c.Alumno).Include(c => c.Curso)
                .FirstOrDefaultAsync(m => m.CalificacionId == id);
            if (calificacion == null || calificacion.CalificacionEliminada) return NotFound();
            return View(calificacion);
        }

        // GET: Calificaciones/Create
        public IActionResult Create()
        {
            ViewData["AlumnoId"] = new SelectList(_context.Alumnos.Where(a => !a.AlumnoEliminado), "AlumnoId", "Nombre");
            ViewData["CursoId"] = new SelectList(_context.Cursos.Where(c => !c.CursoEliminado), "CursoId", "Nombre");
            return View();
        }

        // POST: Calificaciones/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CalificacionId,CursoId,AlumnoId,NotaParcialUno,NotaParcialDos,NotaZona,NotaExamen,NotaFinal")] Calificacion calificacion)
        {
            if (ModelState.IsValid)
            {
                _context.Add(calificacion);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AlumnoId"] = new SelectList(_context.Alumnos.Where(a => !a.AlumnoEliminado), "AlumnoId", "Nombre", calificacion.AlumnoId);
            ViewData["CursoId"] = new SelectList(_context.Cursos.Where(c => !c.CursoEliminado), "CursoId", "Nombre", calificacion.CursoId);
            return View(calificacion);
        }

        // GET: Calificaciones/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null) return NotFound();
            var calificacion = await _context.Calificaciones.FindAsync(id);
            if (calificacion == null || calificacion.CalificacionEliminada) return NotFound();
            ViewData["AlumnoId"] = new SelectList(_context.Alumnos.Where(a => !a.AlumnoEliminado), "AlumnoId", "Nombre", calificacion.AlumnoId);
            ViewData["CursoId"] = new SelectList(_context.Cursos.Where(c => !c.CursoEliminado), "CursoId", "Nombre", calificacion.CursoId);
            return View(calificacion);
        }

        // POST: Calificaciones/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("CalificacionId,CursoId,AlumnoId,NotaParcialUno,NotaParcialDos,NotaZona,NotaExamen,NotaFinal,CalificacionEliminada")] Calificacion calificacion)
        {
            if (id != calificacion.CalificacionId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(calificacion);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CalificacionExists(calificacion.CalificacionId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["AlumnoId"] = new SelectList(_context.Alumnos.Where(a => !a.AlumnoEliminado), "AlumnoId", "Nombre", calificacion.AlumnoId);
            ViewData["CursoId"] = new SelectList(_context.Cursos.Where(c => !c.CursoEliminado), "CursoId", "Nombre", calificacion.CursoId);
            return View(calificacion);
        }

        // GET: Calificaciones/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null) return NotFound();
            var calificacion = await _context.Calificaciones
                .Include(c => c.Alumno).Include(c => c.Curso)
                .FirstOrDefaultAsync(m => m.CalificacionId == id);
            if (calificacion == null || calificacion.CalificacionEliminada) return NotFound();
            return View(calificacion);
        }

        // POST: Calificaciones/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var calificacion = await _context.Calificaciones.FindAsync(id);
            if (calificacion != null)
            {
                calificacion.CalificacionEliminada = true;
                _context.Update(calificacion);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CalificacionExists(Guid id)
        {
            return _context.Calificaciones.Any(e => e.CalificacionId == id && !e.CalificacionEliminada);
        }
    }
}