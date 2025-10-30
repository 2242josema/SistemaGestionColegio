using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SistemaGestionNotas.Models
{
    public class UsuarioAplicacion : IdentityUser 
    {
        public DateTime FechaCreacion { get; set; }          

    }
}
