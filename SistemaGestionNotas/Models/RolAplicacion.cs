using Microsoft.AspNetCore.Identity;
using System;

namespace SistemaGestionNotas.Models
{ 
    public class RolAplicacion : IdentityRole
    {
        public string Descripcion { get; set; }
        public DateTime FechaCreacion { get; set; }       
    }
}