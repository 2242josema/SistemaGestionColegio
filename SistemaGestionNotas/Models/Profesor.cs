using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc; // <-- AÑADIR ESTE USING

namespace SistemaGestionNotas.Models
{
    public class Profesor
    {
        [Key]
        public Guid ProfesorId { get; set; } = Guid.NewGuid();

        [Required(ErrorMessage = "Debe seleccionar un usuario.")]
        [DisplayName("Usuario Asociado")]        
        [Remote(action: "ValidateUsuarioId", controller: "Profesores", AdditionalFields = nameof(ProfesorId), ErrorMessage = "Este usuario ya está asignado.")]
        public string UsuarioId { get; set; }

        [ForeignKey("UsuarioId")]
        public virtual UsuarioAplicacion? Usuario { get; set; } // Se hace nullable

        [Required(ErrorMessage = "El código es obligatorio")]
        [StringLength(20, ErrorMessage = "El código no puede tener más de 20 caracteres")]
        [DisplayName("Código del Profesor")]
        [Remote(action: "ValidateCodigo", controller: "Profesores", AdditionalFields = nameof(ProfesorId), ErrorMessage = "Este código ya existe.")]
        public string Codigo { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(200, ErrorMessage = "El nombre no puede tener más de 100 caracteres")]
        [DisplayName("Nombre del Profesor")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El no. de identificación es obligatorio")]
        [StringLength(30)]
        [DisplayName("Número de Identificación")]
        // 👇 VALIDACIÓN REMOTA PARA IDENTIFICACIÓN
        [Remote(action: "ValidateIdentificacion", controller: "Profesores", AdditionalFields = nameof(ProfesorId), ErrorMessage = "Esta identificación ya está en uso.")]
        public string Identificacion { get; set; }

        // ... resto de las propiedades sin cambios ...
        [Required(ErrorMessage = "El número de teléfono es obligatorio")]
        [DisplayName("Número de Teléfono")]
        [Range(10000000, 9999999999, ErrorMessage = "Ingrese un número de teléfono válido")]
        public int NumeroTelefono { get; set; }
        [Required(ErrorMessage = "La fecha de nacimiento es obligatoria")]
        [DataType(DataType.Date)]
        [DisplayName("Fecha de Nacimiento")]
        public DateTime FechaNacimiento { get; set; }
        [StringLength(200, ErrorMessage = "La dirección no puede tener más de 200 caracteres")]
        [DisplayName("Dirección del Profesor")]
        public string Direccion { get; set; }
        [StringLength(100, ErrorMessage = "El título no puede tener más de 100 caracteres")]
        [DisplayName("Título Académico")]
        public string Titulo { get; set; }
        [StringLength(100, ErrorMessage = "La especialidad no puede tener más de 100 caracteres")]
        [DisplayName("Especialidad del Profesor")]
        public string Especialidad { get; set; }
        [Required(ErrorMessage = "La fecha de ingreso es obligatoria")]
        [DataType(DataType.Date)]
        [DisplayName("Fecha de Ingreso")]
        public DateTime FechaIngreso { get; set; }
        [Required(ErrorMessage = "El estado del profesor es obligatorio")]
        [StringLength(20, ErrorMessage = "El estado no puede tener más de 20 caracteres")]
        [DisplayName("Estado del Profesor")]
        public string EstadoProfesor { get; set; }
        [ScaffoldColumn(false)]
        public bool ProfesorEliminado { get; set; }
    }
}