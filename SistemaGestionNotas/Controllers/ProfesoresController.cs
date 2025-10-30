using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaGestionNotas.Data;
using SistemaGestionNotas.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaGestionNotas.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class ProfesoresController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProfesoresController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Profesores
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Profesores
                .Include(p => p.Usuario)
                .Where(p => !p.ProfesorEliminado);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Profesores/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null) return NotFound();
            var profesor = await _context.Profesores
                .Include(p => p.Usuario)
                .FirstOrDefaultAsync(m => m.ProfesorId == id && !m.ProfesorEliminado);
            if (profesor == null) return NotFound();
            return View(profesor);
        }

        // GET: Profesores/Create (MODIFICADO para recibir usuarioId)
        public IActionResult Create(string usuarioId)
        {
            CargarUsuariosDisponibles(usuarioId); // Usar el ID recibido para preseleccionar

            // Inicializar el modelo con el UsuarioId si se recibe
            var profesor = new Profesor();
            if (!string.IsNullOrEmpty(usuarioId))
            {
                profesor.UsuarioId = usuarioId;
            }
            return View(profesor);
        }

        // POST: Profesores/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProfesorId,UsuarioId,Codigo,Nombre,Identificacion,NumeroTelefono,FechaNacimiento,Direccion,Titulo,Especialidad,FechaIngreso,EstadoProfesor")] Profesor profesor)
        {
            // Validaciones de duplicados del lado del servidor
            if (await _context.Profesores.AnyAsync(p => p.Codigo == profesor.Codigo && !p.ProfesorEliminado))
            {
                ModelState.AddModelError("Codigo", "El código ya está registrado para otro profesor.");
            }
            if (await _context.Profesores.AnyAsync(p => p.Identificacion == profesor.Identificacion && !p.ProfesorEliminado))
            {
                ModelState.AddModelError("Identificacion", "La identificación ya está registrada para otro profesor.");
            }
            if (await _context.Profesores.AnyAsync(p => p.UsuarioId == profesor.UsuarioId && !p.ProfesorEliminado) ||
                await _context.Alumnos.AnyAsync(a => a.UsuarioId == profesor.UsuarioId && !a.AlumnoEliminado))
            {
                ModelState.AddModelError("UsuarioId", "El usuario ya está asignado a un profesor o alumno.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(profesor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Si hay error, recargar y volver a la vista
            CargarUsuariosDisponibles(profesor.UsuarioId);
            return View(profesor);
        }

        // GET: Profesores/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null) return NotFound();
            var profesor = await _context.Profesores.FindAsync(id);
            if (profesor == null) return NotFound();
            CargarUsuariosDisponibles(profesor.UsuarioId);
            return View(profesor);
        }

        // POST: Profesores/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("ProfesorId,UsuarioId,Codigo,Nombre,Identificacion,NumeroTelefono,FechaNacimiento,Direccion,Titulo,Especialidad,FechaIngreso,EstadoProfesor,ProfesorEliminado")] Profesor profesor)
        {
            if (id != profesor.ProfesorId) return NotFound();

            // Validaciones de duplicados del lado del servidor
            if (await _context.Profesores.AnyAsync(p => p.Codigo == profesor.Codigo && p.ProfesorId != id && !p.ProfesorEliminado))
            {
                ModelState.AddModelError("Codigo", "El código ya está registrado para otro profesor.");
            }
            if (await _context.Profesores.AnyAsync(p => p.Identificacion == profesor.Identificacion && p.ProfesorId != id && !p.ProfesorEliminado))
            {
                ModelState.AddModelError("Identificacion", "La identificación ya está registrada para otro profesor.");
            }
            if (await _context.Profesores.AnyAsync(p => p.UsuarioId == profesor.UsuarioId && p.ProfesorId != id && !p.ProfesorEliminado) ||
                await _context.Alumnos.AnyAsync(a => a.UsuarioId == profesor.UsuarioId && !a.AlumnoEliminado))
            {
                ModelState.AddModelError("UsuarioId", "El usuario ya está asignado a otro perfil.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(profesor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProfesorExists(profesor.ProfesorId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            // Si hay error, recargar y volver a la vista
            CargarUsuariosDisponibles(profesor.UsuarioId);
            return View(profesor);
        }

        // GET: Profesores/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null) return NotFound();
            var profesor = await _context.Profesores.Include(p => p.Usuario).FirstOrDefaultAsync(m => m.ProfesorId == id && !m.ProfesorEliminado);
            if (profesor == null) return NotFound();
            return View(profesor);
        }

        // POST: Profesores/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var profesor = await _context.Profesores.FindAsync(id);
            if (profesor != null)
            {
                profesor.ProfesorEliminado = true;
                _context.Update(profesor);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ProfesorExists(Guid id)
        {
            return _context.Profesores.Any(e => e.ProfesorId == id && !e.ProfesorEliminado);
        }

        private void CargarUsuariosDisponibles(string selectedUsuarioId = null)
        {
            // 1. Usuarios asignados a un perfil (excluyendo el actual si estamos en Edit)
            var usuariosAsignados = _context.Profesores
                .Where(p => !p.ProfesorEliminado && p.UsuarioId != selectedUsuarioId)
                .Select(p => p.UsuarioId)
                .Union(
                    _context.Alumnos
                        .Where(a => !a.AlumnoEliminado)
                        .Select(a => a.UsuarioId)
                );

            // 2. Usuarios disponibles (No asignados O es el ID que recibimos)
            var usuariosDisponibles = _context.Users
                .Where(u => !usuariosAsignados.Contains(u.Id) || u.Id == selectedUsuarioId)
                .ToList();

            ViewData["UsuarioId"] = new SelectList(usuariosDisponibles, "Id", "UserName", selectedUsuarioId);
        }

        // MÉTODOS PARA LA VALIDACIÓN EN TIEMPO REAL
        [AcceptVerbs("GET", "POST")]
        public async Task<IActionResult> ValidateCodigo(string codigo, Guid? profesorId)
        {
            bool exists = await _context.Profesores.AnyAsync(p => p.Codigo == codigo && p.ProfesorId != profesorId && !p.ProfesorEliminado);
            return exists ? Json($"El código '{codigo}' ya está en uso.") : Json(true);
        }

        [AcceptVerbs("GET", "POST")]
        public async Task<IActionResult> ValidateIdentificacion(string identificacion, Guid? profesorId)
        {
            bool exists = await _context.Profesores.AnyAsync(p => p.Identificacion == identificacion && p.ProfesorId != profesorId && !p.ProfesorEliminado);
            return exists ? Json($"La identificación '{identificacion}' ya está en uso.") : Json(true);
        }

        [AcceptVerbs("GET", "POST")]
        public async Task<IActionResult> ValidateUsuarioId(string usuarioId, Guid? profesorId)
        {
            bool isTakenByProfessor = await _context.Profesores.AnyAsync(p => p.UsuarioId == usuarioId && p.ProfesorId != profesorId && !p.ProfesorEliminado);
            if (isTakenByProfessor)
            {
                return Json("Este usuario ya está asignado a otro profesor.");
            }

            bool isTakenByAlumno = await _context.Alumnos.AnyAsync(a => a.UsuarioId == usuarioId && !a.AlumnoEliminado);
            if (isTakenByAlumno)
            {
                return Json("Este usuario ya está asignado a un alumno.");
            }

            return Json(true);
        }
    }
}