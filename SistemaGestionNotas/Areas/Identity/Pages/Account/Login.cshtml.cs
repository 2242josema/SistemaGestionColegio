#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims; // <-- 1. AÑADIR ESTE USING
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore; // <-- 2. AÑADIR ESTE USING
using Microsoft.Extensions.Logging;
using SistemaGestionNotas.Data; // <-- 3. AÑADIR ESTE USING
using SistemaGestionNotas.Models;

namespace SistemaGestionNotas.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<UsuarioAplicacion> _signInManager;
        private readonly UserManager<UsuarioAplicacion> _userManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly ApplicationDbContext _context; // <-- 4. AÑADIR EL CAMPO PARA EL DBCONTEXT

        // 5. MODIFICAR EL CONSTRUCTOR
        public LoginModel(
            SignInManager<UsuarioAplicacion> signInManager,
            UserManager<UsuarioAplicacion> userManager,
            ILogger<LoginModel> logger,
            ApplicationDbContext context) // <-- Añadir parámetro
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _context = context; // <-- Asignar el contexto
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "Correo o usuario")]
            public string UserNameOrEmail { get; set; }

            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Contraseña")]
            public string Password { get; set; }

            [Display(Name = "Recordar Cuenta")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (!ModelState.IsValid)
                return Page();

            UsuarioAplicacion usuario = null;
            if (Input.UserNameOrEmail.Contains("@"))
            {
                usuario = await _userManager.FindByEmailAsync(Input.UserNameOrEmail);
            }
            else
            {
                usuario = await _userManager.FindByNameAsync(Input.UserNameOrEmail);
            }

            if (usuario != null)
            {
                var result = await _signInManager.PasswordSignInAsync(
                    usuario.UserName,
                    Input.Password,
                    Input.RememberMe,
                    lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    // ===== INICIO DE LA LÓGICA PARA AÑADIR EL NOMBRE COMPLETO =====

                    string nombreCompleto = usuario.UserName; // Valor por defecto

                    // Buscamos el nombre real según el rol del usuario
                    if (await _userManager.IsInRoleAsync(usuario, "Profesor"))
                    {
                        var profesor = await _context.Profesores.FirstOrDefaultAsync(p => p.UsuarioId == usuario.Id);
                        if (profesor != null)
                        {
                            nombreCompleto = profesor.Nombre;
                        }
                    }
                    else if (await _userManager.IsInRoleAsync(usuario, "Alumno"))
                    {
                        var alumno = await _context.Alumnos.FirstOrDefaultAsync(a => a.UsuarioId == usuario.Id);
                        if (alumno != null)
                        {
                            nombreCompleto = alumno.Nombre;
                        }
                    }

                    // Creamos el nuevo Claim con el nombre completo
                    var claim = new Claim("FullName", nombreCompleto);
                    var existingClaim = (await _userManager.GetClaimsAsync(usuario)).FirstOrDefault(c => c.Type == "FullName");

                    // Si ya existe un claim con el nombre, lo actualizamos. Si no, lo creamos.
                    if (existingClaim != null)
                    {
                        await _userManager.ReplaceClaimAsync(usuario, existingClaim, claim);
                    }
                    else
                    {
                        await _userManager.AddClaimAsync(usuario, claim);
                    }

                    // Refrescamos la cookie de sesión para que incluya el nuevo claim
                    await _signInManager.RefreshSignInAsync(usuario);

                    // ===== FIN DE LA LÓGICA =====

                    _logger.LogInformation("Usuario ha iniciado sesión.");
                    return LocalRedirect(returnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("Usuario bloqueado.");
                    return RedirectToPage("./Lockout");
                }
            }

            ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrectos.");
            return Page();
        }
    }
}