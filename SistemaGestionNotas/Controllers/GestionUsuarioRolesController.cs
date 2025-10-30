using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SistemaGestionNotas.Models;

namespace SistemaGestionNotas.Controllers
{

    [Authorize(Roles = "Administrador")]
    public class GestionUsuarioRolesController : Controller
    {
        private readonly UserManager<UsuarioAplicacion> _userManager;
        private readonly RoleManager<RolAplicacion> _roleManager;

        public GestionUsuarioRolesController(UserManager<UsuarioAplicacion> userManager, RoleManager<RolAplicacion> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: Lista de usuarios con roles
        public async Task<IActionResult> Index()
        {
            var users = _userManager.Users.ToList();
            var model = new List<GestionUsuarioRoles>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                model.Add(new GestionUsuarioRoles
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    Roles = roles.ToList(),
                    SelectedRoles = roles.ToList()
                });
            }

            return View(model);
        }

        // GET: Editar roles de un usuario
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var roles = _roleManager.Roles.Select(r => r.Name).ToList();
            var userRoles = await _userManager.GetRolesAsync(user);

            var model = new GestionUsuarioRoles
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Roles = roles,
                SelectedRoles = userRoles.ToList()
            };

            return View(model);
        }

        // POST: Guardar roles de un usuario
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(GestionUsuarioRoles model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
                return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);

            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (model.SelectedRoles != null && model.SelectedRoles.Count > 0)
            {
                await _userManager.AddToRolesAsync(user, model.SelectedRoles);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Confirmación de eliminación
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            return View(user);
        }

        // POST: Eliminar usuario
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Error al eliminar el usuario.");
                return View(user);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}