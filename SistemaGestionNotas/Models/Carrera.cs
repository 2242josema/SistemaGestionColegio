using Microsoft.AspNetCore.Mvc; 
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SistemaGestionNotas.Models
{
    public class Carrera
    {
        [Key]
        public Guid CarreraId { get; set; } = Guid.NewGuid();

        [Required(ErrorMessage = "El código es obligatorio")]
        [StringLength(20, ErrorMessage = "El código no puede tener más de 20 caracteres")]
        [DisplayName("Código de la Carrera")]        
        [Remote(action: "ValidateCodigo", controller: "Carreras", AdditionalFields = nameof(CarreraId), ErrorMessage = "Este código de carrera ya existe.")]
        public string Codigo { get; set; }

        [Required(ErrorMessage = "El nombre de la carrera es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede tener más de 100 caracteres")]
        [DisplayName("Nombre de la Carrera")]
        public string Nombre { get; set; }

        [StringLength(100, ErrorMessage = "La descripción no puede tener más de 100 caracteres")]
        [DisplayName("Descripción de la Carrera")]
        public string Descripcion { get; set; }

        [Required(ErrorMessage = "El estado de la carrera es obligatorio")]
        [StringLength(20, ErrorMessage = "El estado no puede tener más de 20 caracteres")]
        [DisplayName("Estado de la Carrera")]
        public string EstadoCarrera { get; set; }

        [ScaffoldColumn(false)]
        public bool CarreraEliminada { get; set; }
    }
}