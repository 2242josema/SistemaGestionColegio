using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SistemaGestionNotas.Data;
using SistemaGestionNotas.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaGestionNotas.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class RolesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<RolAplicacion> _roleManager;

        public RolesController(ApplicationDbContext context, RoleManager<RolAplicacion> roleManager)
        {
            _context = context;
            _roleManager = roleManager;
        }

        // GET: Roles
        public IActionResult Index()
        {
            var roles = _roleManager.Roles.ToList();
            return View(roles);
        }

        // GET: Roles/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null) return NotFound();

            var rol = await _roleManager.FindByIdAsync(id);
            if (rol == null) return NotFound();

            return View(rol);
        }

        // GET: Roles/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Roles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string Name, string Descripcion)
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                ModelState.AddModelError("Name", "El nombre del rol es obligatorio.");
            }

            if (ModelState.IsValid)
            {
                // Hora de Guatemala
                TimeZoneInfo zonaGuatemala = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
                var fechaGuatemala = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zonaGuatemala);

                var rol = new RolAplicacion
                {
                    Name = Name,
                    NormalizedName = Name.ToUpper(),
                    Descripcion = Descripcion,
                    FechaCreacion = fechaGuatemala
                };

                var result = await _roleManager.CreateAsync(rol);
                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
            }

            return View();
        }

        // GET: Roles/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null) return NotFound();

            var rol = await _roleManager.FindByIdAsync(id);
            if (rol == null) return NotFound();

            return View(rol);
        }

        // POST: Roles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, string Name, string Descripcion)
        {
            if (id == null) return NotFound();

            var rol = await _roleManager.FindByIdAsync(id);
            if (rol == null) return NotFound();

            if (ModelState.IsValid)
            {
                rol.Name = Name;
                rol.NormalizedName = Name.ToUpper();
                rol.Descripcion = Descripcion;

                var result = await _roleManager.UpdateAsync(rol);
                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
            }

            return View(rol);
        }

        // GET: Roles/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var rol = await _roleManager.FindByIdAsync(id);
            if (rol == null)
                return NotFound();

            return View(rol);
        }

        // POST: Roles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var rol = await _roleManager.FindByIdAsync(id);
            if (rol != null)
            {

                var result = await _roleManager.DeleteAsync(rol);

                if (!result.Succeeded)
                {

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(rol);
                }
            }

            return RedirectToAction(nameof(Index));
        }

    }
}
    