using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SistemaGestionNotas.Models;
using SistemaGestionNotas.Data;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System;

namespace SistemaGestionNotas.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class UsuariosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<UsuarioAplicacion> _userManager;
        private readonly RoleManager<RolAplicacion> _roleManager;

        public UsuariosController(
            ApplicationDbContext context,
            UserManager<UsuarioAplicacion> userManager,
            RoleManager<RolAplicacion> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: Usuarios
        public async Task<IActionResult> Index()
        {
            var usuarios = _userManager.Users.ToList();
            var rolesPorUsuario = new Dictionary<string, string>();
            foreach (var usuario in usuarios)
            {
                var roles = await _userManager.GetRolesAsync(usuario);
                rolesPorUsuario[usuario.Id] = roles.FirstOrDefault() ?? "N/A";
            }
            ViewBag.RolesPorUsuario = rolesPorUsuario;
            return View(usuarios);
        }

        // GET: Usuarios/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null) return NotFound();
            var usuario = await _userManager.FindByIdAsync(id);
            if (usuario == null) return NotFound();
            var rolesUsuario = await _userManager.GetRolesAsync(usuario);
            ViewBag.RolUsuario = rolesUsuario.FirstOrDefault() ?? "Sin rol asignado";
            return View(usuario);
        }

        // GET: Usuarios/Create
        public IActionResult Create()
        {
            ViewBag.Roles = new SelectList(_roleManager.Roles, "Name", "Name");
            return View();
        }

        // POST: Usuarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserName,Email")] UsuarioAplicacion usuario, string password, string rolSeleccionado)
        {
            ViewBag.Roles = new SelectList(_roleManager.Roles, "Name", "Name", rolSeleccionado);

            if (string.IsNullOrEmpty(rolSeleccionado))
            {
                ModelState.AddModelError("", "Debe seleccionar un rol.");
                return View(usuario);
            }

            if (string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("password", "La contraseña es requerida.");
                return View(usuario);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    TimeZoneInfo zonaGuatemala = TimeZoneInfo.FindSystemTimeZoneById("Central America Standard Time");
                    usuario.FechaCreacion = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zonaGuatemala);
                }
                catch (TimeZoneNotFoundException)
                {
                    usuario.FechaCreacion = DateTime.UtcNow; // Fallback
                }

                var result = await _userManager.CreateAsync(usuario, password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(usuario, rolSeleccionado);
                    TempData["UsuarioCreado_Password"] = password;
                    TempData["UsuarioCreado_UserName"] = usuario.UserName;
                    TempData["UsuarioCreado_Email"] = usuario.Email;
                    return RedirectToAction(nameof(CreateSuccess), new { usuarioId = usuario.Id, rol = rolSeleccionado });
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(usuario);
        }

        // GET: Usuarios/CreateSuccess
        public async Task<IActionResult> CreateSuccess(string usuarioId, string rol)
        {
            if (string.IsNullOrEmpty(usuarioId) || string.IsNullOrEmpty(rol))
            {
                return RedirectToAction(nameof(Index));
            }

            ViewBag.UsuarioId = usuarioId;
            ViewBag.RolCreado = rol;
            ViewBag.Password = TempData["UsuarioCreado_Password"] as string;
            ViewBag.UserName = TempData["UsuarioCreado_UserName"] as string;
            ViewBag.Email = TempData["UsuarioCreado_Email"] as string;

            if (ViewBag.UserName == null || ViewBag.Email == null)
            {
                var usuario = await _userManager.FindByIdAsync(usuarioId);
                if (usuario != null)
                {
                    ViewBag.UserName = usuario.UserName;
                    ViewBag.Email = usuario.Email;
                }
            }
            return View();
        }

        // GET: Usuarios/Edit/5 
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null) return NotFound();
            var usuario = await _userManager.FindByIdAsync(id);
            if (usuario == null) return NotFound();

            var rolesUsuario = await _userManager.GetRolesAsync(usuario);
            var rolActual = rolesUsuario.FirstOrDefault();

            ViewBag.Roles = new SelectList(_roleManager.Roles, "Name", "Name", rolActual);

            return View(usuario);
        }

        // POST: Usuarios/Edit/5 
        // ***** MODIFICADO *****
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,UserName,Email")] UsuarioAplicacion usuario, string? newPassword, string rolSeleccionado)
        {
            if (id != usuario.Id) return NotFound();

            var usuarioDb = await _userManager.FindByIdAsync(id);
            if (usuarioDb == null) return NotFound();

            if (string.IsNullOrEmpty(rolSeleccionado))
            {
                ModelState.AddModelError("", "Debe seleccionar un rol.");
                ViewBag.Roles = new SelectList(_roleManager.Roles, "Name", "Name");
                return View(usuarioDb);
            }

            if (ModelState.IsValid)
            {
                usuarioDb.UserName = usuario.UserName;
                usuarioDb.Email = usuario.Email;
                var updateResult = await _userManager.UpdateAsync(usuarioDb);

                if (!updateResult.Succeeded)
                {
                    foreach (var error in updateResult.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    ViewBag.Roles = new SelectList(_roleManager.Roles, "Name", "Name", rolSeleccionado);
                    return View(usuarioDb);
                }

                var rolesActuales = await _userManager.GetRolesAsync(usuarioDb);
                await _userManager.RemoveFromRolesAsync(usuarioDb, rolesActuales);
                await _userManager.AddToRoleAsync(usuarioDb, rolSeleccionado);

                if (!string.IsNullOrEmpty(newPassword))
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(usuarioDb);
                    var passwordResult = await _userManager.ResetPasswordAsync(usuarioDb, token, newPassword);

                    if (!passwordResult.Succeeded)
                    {
                        foreach (var error in passwordResult.Errors)
                        {
                            ModelState.AddModelError("newPassword", error.Description);
                        }
                        ViewBag.Roles = new SelectList(_roleManager.Roles, "Name", "Name", rolSeleccionado);
                        return View(usuarioDb);
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Roles = new SelectList(_roleManager.Roles, "Name", "Name", rolSeleccionado);
            return View(usuarioDb);
        }

        // GET: Usuarios/Delete/5 
        // ***** MODIFICADO *****
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null) return NotFound();
            var usuario = await _userManager.FindByIdAsync(id);
            if (usuario == null) return NotFound();

            var rolesUsuario = await _userManager.GetRolesAsync(usuario);
            ViewBag.RolUsuario = rolesUsuario.FirstOrDefault() ?? "Sin rol asignado";

            return View(usuario);
        }

        // POST: Usuarios/Delete/5 
        // ***** MODIFICADO *****
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var usuario = await _userManager.FindByIdAsync(id);
            if (usuario == null)
            {
                TempData["ErrorMessage"] = "No se encontró el usuario para eliminar.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var result = await _userManager.DeleteAsync(usuario);

                if (result.Succeeded)
                {
                    TempData["MensajeExitoUsuario"] = $"Usuario '{usuario.UserName}' eliminado correctamente.";
                }
                else
                {
                    string errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    TempData["MensajeErrorUsuario"] = $"Error de Identity al eliminar: {errors}";
                }
            }
            catch (DbUpdateException ex)
            {
                TempData["MensajeErrorUsuario"] = $"ERROR: No se puede eliminar a '{usuario.UserName}'. " +
                                           "El usuario está asociado a otros registros (como Alumnos o Profesores). " +
                                           "Debe eliminar esos registros primero.";
            }
            catch (Exception ex)
            {
                TempData["MensajeErrorUsuario"] = "Ocurrió un error inesperado al intentar eliminar.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}