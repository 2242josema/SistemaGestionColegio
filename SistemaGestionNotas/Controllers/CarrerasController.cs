using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaGestionNotas.Data;
using SistemaGestionNotas.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaGestionNotas.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class CarrerasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CarrerasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Carreras
        public async Task<IActionResult> Index()
        {
            return View(await _context.Carreras
                .Where(c => !c.CarreraEliminada)
                .ToListAsync());
        }

        // GET: Carreras/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var carrera = await _context.Carreras
                .FirstOrDefaultAsync(m => m.CarreraId == id);
            if (carrera == null)
            {
                return NotFound();
            }

            return View(carrera);
        }

        // GET: Carreras/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Carreras/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CarreraId,Codigo,Nombre,Descripcion,EstadoCarrera")] Carrera carrera)
        {
            if (await _context.Carreras.AnyAsync(c => c.Codigo == carrera.Codigo && !c.CarreraEliminada))
            {
                ModelState.AddModelError("Codigo", "Este código de carrera ya existe. Por favor, ingrese uno diferente.");
            }

            if (ModelState.IsValid)
            {
                carrera.CarreraId = Guid.NewGuid();
                _context.Add(carrera);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(carrera);
        }

        // GET: Carreras/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var carrera = await _context.Carreras.FindAsync(id);
            if (carrera == null)
            {
                return NotFound();
            }
            return View(carrera);
        }

        // POST: Carreras/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("CarreraId,Codigo,Nombre,Descripcion,EstadoCarrera")] Carrera carrera)
        {
            if (id != carrera.CarreraId)
            {
                return NotFound();
            }

            if (await _context.Carreras.AnyAsync(c => c.Codigo == carrera.Codigo && c.CarreraId != id && !c.CarreraEliminada))
            {
                ModelState.AddModelError("Codigo", "Este código de carrera ya está en uso por otra carrera.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(carrera);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CarreraExists(carrera.CarreraId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(carrera);
        }

        // GET: Carreras/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var carrera = await _context.Carreras
                .FirstOrDefaultAsync(m => m.CarreraId == id);
            if (carrera == null)
            {
                return NotFound();
            }

            return View(carrera);
        }

        // POST: Carreras/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var carrera = await _context.Carreras.FindAsync(id);

            if (carrera != null)
            {
                carrera.CarreraEliminada = true;
                _context.Update(carrera);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

       
        [AcceptVerbs("GET", "POST")]
        public async Task<IActionResult> ValidateCodigo(string codigo, Guid? carreraId)
        {
            bool exists = await _context.Carreras.AnyAsync(c => c.Codigo == codigo && c.CarreraId != carreraId && !c.CarreraEliminada);
            return exists ? Json("Este código de carrera ya existe.") : Json(true);
        }

        private bool CarreraExists(Guid id)
        {
            return _context.Carreras.Any(e => e.CarreraId == id);
        }
    }
}