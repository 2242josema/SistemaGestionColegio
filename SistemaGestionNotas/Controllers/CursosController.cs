using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaGestionNotas.Data;
using SistemaGestionNotas.Models;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SistemaGestionNotas.Controllers
{
    [Authorize(Roles = "Administrador,Profesor,Alumno")]
    public class CursosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CursosController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            IQueryable<Curso> cursos = _context.Cursos
                                              .Include(c => c.Carrera)
                                              .Include(c => c.Profesor)
                                              .Where(c => !c.CursoEliminado);

            if (User.IsInRole("Profesor"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var profesor = await _context.Profesores.FirstOrDefaultAsync(p => p.UsuarioId == userId);
                if (profesor != null)
                {
                    cursos = cursos.Where(c => c.ProfesorId == profesor.ProfesorId);
                }
            }
            else if (User.IsInRole("Alumno"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var alumno = await _context.Alumnos.FirstOrDefaultAsync(a => a.UsuarioId == userId);

                if (alumno != null)
                {
                    cursos = cursos.Where(c => _context.CursoInscripciones
                                                      .Any(ci => ci.AlumnoId == alumno.AlumnoId && ci.CursoId == c.CursoId && !ci.InscripcionEliminada));
                }
                else
                {
                    cursos = cursos.Where(c => false);
                }
            }

            return View(await cursos.ToListAsync());
        }

        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null) return NotFound();

            var curso = await _context.Cursos
                .Include(c => c.Carrera)
                .Include(c => c.Profesor)
                .FirstOrDefaultAsync(m => m.CursoId == id);

            if (curso == null) return NotFound();

            if (curso.CursoEliminado) return NotFound();

            if (User.IsInRole("Profesor"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var profesor = await _context.Profesores.FirstOrDefaultAsync(p => p.UsuarioId == userId);
                if (profesor != null && curso.ProfesorId != profesor.ProfesorId)
                {
                    return Forbid();
                }
            }

            return View(curso);
        }

        [Authorize(Roles = "Administrador")]
        public IActionResult Create()
        {
            ViewData["CarreraId"] = new SelectList(_context.Carreras.Where(c => !c.CarreraEliminada && c.EstadoCarrera == "Activa"), "CarreraId", "Nombre");
            ViewData["ProfesorId"] = new SelectList(_context.Profesores.Where(p => !p.ProfesorEliminado && p.EstadoProfesor == "Activo"), "ProfesorId", "Nombre");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Create([Bind("CursoId,Codigo,Nombre,Seccion,CarreraId,ProfesorId,Ciclo,EstadoCurso")] Curso curso)
        {
            if (await _context.Cursos.AnyAsync(c => c.Codigo == curso.Codigo && !c.CursoEliminado))
            {
                ModelState.AddModelError("Codigo", "Este código de curso ya existe. Por favor, ingrese uno diferente.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(curso);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["CarreraId"] = new SelectList(_context.Carreras.Where(c => !c.CarreraEliminada && c.EstadoCarrera == "Activa"), "CarreraId", "Nombre", curso.CarreraId);
            ViewData["ProfesorId"] = new SelectList(_context.Profesores.Where(p => !p.ProfesorEliminado && p.EstadoProfesor == "Activo"), "ProfesorId", "Nombre", curso.ProfesorId);
            return View(curso);
        }

        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null) return NotFound();

            var curso = await _context.Cursos.FindAsync(id);

            if (curso == null || curso.CursoEliminado) return NotFound();

            ViewData["CarreraId"] = new SelectList(_context.Carreras.Where(c => !c.CarreraEliminada && c.EstadoCarrera == "Activa"), "CarreraId", "Nombre", curso.CarreraId);
            ViewData["ProfesorId"] = new SelectList(_context.Profesores.Where(p => !p.ProfesorEliminado && p.EstadoProfesor == "Activo"), "ProfesorId", "Nombre", curso.ProfesorId);
            return View(curso);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Edit(Guid id, [Bind("CursoId,Codigo,Nombre,Seccion,CarreraId,ProfesorId,Ciclo,EstadoCurso,CursoEliminado")] Curso curso)
        {
            if (id != curso.CursoId) return NotFound();

            if (await _context.Cursos.AnyAsync(c => c.Codigo == curso.Codigo && c.CursoId != id && !c.CursoEliminado))
            {
                ModelState.AddModelError("Codigo", "Este código de curso ya está en uso por otro curso.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(curso);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CursoExists(curso.CursoId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["CarreraId"] = new SelectList(_context.Carreras.Where(c => !c.CarreraEliminada && c.EstadoCarrera == "Activa"), "CarreraId", "Nombre", curso.CarreraId);
            ViewData["ProfesorId"] = new SelectList(_context.Profesores.Where(p => !p.ProfesorEliminado && p.EstadoProfesor == "Activo"), "ProfesorId", "Nombre", curso.ProfesorId);
            return View(curso);
        }

        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null) return NotFound();

            var curso = await _context.Cursos
                .Include(c => c.Carrera)
                .Include(c => c.Profesor)
                .FirstOrDefaultAsync(m => m.CursoId == id);

            if (curso == null) return NotFound();

            if (curso.CursoEliminado) return NotFound();

            return View(curso);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var curso = await _context.Cursos.FindAsync(id);

            if (curso != null)
            {
                curso.CursoEliminado = true;
                _context.Update(curso);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        
        [AcceptVerbs("GET", "POST")]
        public async Task<IActionResult> ValidateCodigo(string codigo, Guid? cursoId)
        {
            bool exists = await _context.Cursos.AnyAsync(c => c.Codigo == codigo && c.CursoId != cursoId && !c.CursoEliminado);
            return exists ? Json($"El código '{codigo}' ya está en uso.") : Json(true);
        }

        private bool CursoExists(Guid id)
        {
            return _context.Cursos.Any(e => e.CursoId == id && !e.CursoEliminado);
        }
    }
}