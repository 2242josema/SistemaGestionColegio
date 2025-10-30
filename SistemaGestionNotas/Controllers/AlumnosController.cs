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
    public class AlumnosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AlumnosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Alumnos
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Alumnos
                .Include(a => a.Carrera)
                .Include(a => a.Usuario)
                .Where(a => !a.AlumnoEliminado);

            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Alumnos/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null) return NotFound();

            var alumno = await _context.Alumnos
                .Include(a => a.Carrera)
                .Include(a => a.Usuario)
                .FirstOrDefaultAsync(m => m.AlumnoId == id);
            if (alumno == null) return NotFound();

            return View(alumno);
        }

        // GET: Alumnos/Create 
        public IActionResult Create(string usuarioId)
        {
           
            var usuariosAsignados = _context.Alumnos
                .Where(a => !a.AlumnoEliminado)
                .Select(a => a.UsuarioId)
                .Union(
                    _context.Profesores
                        .Where(p => !p.ProfesorEliminado)
                        .Select(p => p.UsuarioId)
                );

            
            var usuariosDisponibles = _context.Users
                .Where(u => !usuariosAsignados.Contains(u.Id) || u.Id == usuarioId)
                .ToList();

            ViewData["CarreraId"] = new SelectList(_context.Carreras.Where(c => !c.CarreraEliminada && c.EstadoCarrera == "Activa"), "CarreraId", "Nombre");

           
            ViewData["UsuarioId"] = new SelectList(usuariosDisponibles, "Id", "UserName", usuarioId);

           
            var alumno = new Alumno();
            if (!string.IsNullOrEmpty(usuarioId))
            {
                alumno.UsuarioId = usuarioId;
            }

            return View(alumno);
        }

        // POST: Alumnos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AlumnoId,UsuarioId,Codigo,Nombre,CarreraId,Identificacion,NumeroTelefono,Direccion,FechaNacimiento,FechaIngreso,EstadoAlumno,Solvente,FechaInicioSolvencia,FechaFinSolvencia")] Alumno alumno)
        {
            
            if (await _context.Alumnos.AnyAsync(a => a.Codigo == alumno.Codigo && !a.AlumnoEliminado))
            {
                ModelState.AddModelError("Codigo", "Este código de alumno ya existe.");
            }
            if (await _context.Alumnos.AnyAsync(a => a.Identificacion == alumno.Identificacion && !a.AlumnoEliminado))
            {
                ModelState.AddModelError("Identificacion", "Este número de identificación ya está en uso.");
            }
            if (await _context.Profesores.AnyAsync(p => p.UsuarioId == alumno.UsuarioId && !p.ProfesorEliminado))
            {
                ModelState.AddModelError("UsuarioId", "Este usuario ya está asignado a un profesor.");
            }

            var usuario = await _context.Users.FindAsync(alumno.UsuarioId);

           
            if (ModelState.IsValid && usuario != null)
            {
                _context.Add(alumno);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            
            var usuariosAsignados = _context.Alumnos
                .Where(a => !a.AlumnoEliminado && a.UsuarioId != alumno.UsuarioId)
                .Select(a => a.UsuarioId)
                .Union(
                    _context.Profesores
                        .Where(p => !p.ProfesorEliminado)
                        .Select(p => p.UsuarioId)
                );

            
            var usuariosDisponibles = _context.Users
                .Where(u => !usuariosAsignados.Contains(u.Id) || u.Id == alumno.UsuarioId)
                .ToList();

            ViewData["CarreraId"] = new SelectList(_context.Carreras.Where(c => !c.CarreraEliminada && c.EstadoCarrera == "Activa"), "CarreraId", "Nombre", alumno.CarreraId);
            ViewData["UsuarioId"] = new SelectList(usuariosDisponibles, "Id", "UserName", alumno.UsuarioId);
            return View(alumno);
        }

        // GET: Alumnos/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null) return NotFound();

            var alumno = await _context.Alumnos.FindAsync(id);
            if (alumno == null) return NotFound();

            
            var usuariosAsignados = _context.Alumnos
                .Where(a => !a.AlumnoEliminado && a.UsuarioId != alumno.UsuarioId) 
                .Select(a => a.UsuarioId)
                .Union(
                    _context.Profesores
                        .Where(p => !p.ProfesorEliminado)
                        .Select(p => p.UsuarioId)
                );
            var usuariosDisponibles = _context.Users
                .Where(u => !usuariosAsignados.Contains(u.Id) || u.Id == alumno.UsuarioId) 
                .ToList();

            ViewData["CarreraId"] = new SelectList(_context.Carreras.Where(c => !c.CarreraEliminada && c.EstadoCarrera == "Activa"), "CarreraId", "Nombre", alumno.CarreraId);
            ViewData["UsuarioId"] = new SelectList(usuariosDisponibles, "Id", "UserName", alumno.UsuarioId);
            return View(alumno);
        }

        // POST: Alumnos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("AlumnoId,UsuarioId,Codigo,Nombre,CarreraId,Identificacion,NumeroTelefono,Direccion,FechaNacimiento,FechaIngreso,EstadoAlumno,Solvente,FechaInicioSolvencia,FechaFinSolvencia,AlumnoEliminado")] Alumno alumno)
        {
            if (id != alumno.AlumnoId) return NotFound();

            
            if (await _context.Alumnos.AnyAsync(a => a.Codigo == alumno.Codigo && a.AlumnoId != id && !a.AlumnoEliminado))
            {
                ModelState.AddModelError("Codigo", "Este código de alumno ya está en uso por otro alumno.");
            }
            if (await _context.Alumnos.AnyAsync(a => a.Identificacion == alumno.Identificacion && a.AlumnoId != id && !a.AlumnoEliminado))
            {
                ModelState.AddModelError("Identificacion", "Este número de identificación ya está en uso por otro alumno.");
            }
            if (await _context.Profesores.AnyAsync(p => p.UsuarioId == alumno.UsuarioId && !p.ProfesorEliminado))
            {
                ModelState.AddModelError("UsuarioId", "Este usuario ya está asignado a un profesor.");
            }

            var usuario = await _context.Users.FindAsync(alumno.UsuarioId);

            if (ModelState.IsValid && usuario != null)
            {
                try
                {
                    _context.Update(alumno);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AlumnoExists(alumno.AlumnoId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            
            var usuariosAsignados = _context.Alumnos
                .Where(a => !a.AlumnoEliminado && a.UsuarioId != alumno.UsuarioId)
                .Select(a => a.UsuarioId)
                .Union(
                    _context.Profesores
                        .Where(p => !p.ProfesorEliminado)
                        .Select(p => p.UsuarioId)
                );
            var usuariosDisponibles = _context.Users
                .Where(u => !usuariosAsignados.Contains(u.Id) || u.Id == alumno.UsuarioId)
                .ToList();

            ViewData["CarreraId"] = new SelectList(_context.Carreras.Where(c => !c.CarreraEliminada && c.EstadoCarrera == "Activa"), "CarreraId", "Nombre", alumno.CarreraId);
            ViewData["UsuarioId"] = new SelectList(usuariosDisponibles, "Id", "UserName", alumno.UsuarioId);
            return View(alumno);
        }

        // GET: Alumnos/Delete/5 
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null) return NotFound();

            var alumno = await _context.Alumnos
                .Include(a => a.Carrera)
                .Include(a => a.Usuario)
                .FirstOrDefaultAsync(m => m.AlumnoId == id);

            if (alumno == null) return NotFound();

            return View(alumno);
        }

        // POST: Alumnos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var alumno = await _context.Alumnos.FindAsync(id);
            if (alumno != null)
            {
                alumno.AlumnoEliminado = true;
                _context.Update(alumno);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        [AcceptVerbs("GET", "POST")]
        public async Task<IActionResult> ValidateCodigo(string codigo, Guid? alumnoId)
        {
            bool exists = await _context.Alumnos.AnyAsync(a => a.Codigo == codigo && a.AlumnoId != alumnoId && !a.AlumnoEliminado);
            return exists ? Json("Este código de alumno ya existe.") : Json(true);
        }

        [AcceptVerbs("GET", "POST")]
        public async Task<IActionResult> ValidateIdentificacion(string identificacion, Guid? alumnoId)
        {
            bool exists = await _context.Alumnos.AnyAsync(a => a.Identificacion == identificacion && a.AlumnoId != alumnoId && !a.AlumnoEliminado);
            return exists ? Json("Este número de identificación ya está en uso.") : Json(true);
        }

        private bool AlumnoExists(Guid id)
        {
            return _context.Alumnos.Any(e => e.AlumnoId == id);
        }
    }
}