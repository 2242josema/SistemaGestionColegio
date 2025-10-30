using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc; 

namespace SistemaGestionNotas.Models
{
    public class Alumno
    {
        [Key]
        public Guid AlumnoId { get; set; } = Guid.NewGuid();

        [Required(ErrorMessage = "Debe seleccionar un usuario.")]
        [DisplayName("Usuario Asociado")]
        public string UsuarioId { get; set; }
        [ForeignKey("UsuarioId")]
        public virtual UsuarioAplicacion? Usuario { get; set; }

        [Required(ErrorMessage = "El código es obligatorio")]
        [StringLength(20, ErrorMessage = "El código no puede tener más de 20 caracteres")]
        [DisplayName("Código del Alumno")]
        
        [Remote(action: "ValidateCodigo", controller: "Alumnos", AdditionalFields = nameof(AlumnoId), ErrorMessage = "Este código de alumno ya existe.")]
        public string Codigo { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(200, ErrorMessage = "El nombre no puede tener más de 100 caracteres")]
        [DisplayName("Nombre del Alumno")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El no. de identificación es obligatorio")]
        [StringLength(30)]
        [DisplayName("Número de Identificación")]
        
        [Remote(action: "ValidateIdentificacion", controller: "Alumnos", AdditionalFields = nameof(AlumnoId), ErrorMessage = "Este número de identificación ya está en uso.")]
        public string Identificacion { get; set; }

        [Required(ErrorMessage = "El número de teléfono es obligatorio")]
        [DisplayName("Número de Teléfono")]
        [Range(10000000, 9999999999, ErrorMessage = "Ingrese un número de teléfono válido")]
        public int NumeroTelefono { get; set; }

        [StringLength(200, ErrorMessage = "La dirección no puede tener más de 200 caracteres")]
        [DisplayName("Dirección del Alumno")]
        public string Direccion { get; set; }

        [Required(ErrorMessage = "La fecha de nacimiento es obligatoria")]
        [DataType(DataType.Date)]
        [DisplayName("Fecha de Nacimiento")]
        public DateTime FechaNacimiento { get; set; }

        [Required(ErrorMessage = "La fecha de ingreso es obligatoria")]
        [DataType(DataType.Date)]
        [DisplayName("Fecha de Ingreso")]
        public DateTime FechaIngreso { get; set; } = DateTime.Now;

        
        [Required(ErrorMessage = "Debe seleccionar una carrera.")]
        [DisplayName("Carrera Asociada")]
        public Guid CarreraId { get; set; }
        [ForeignKey("CarreraId")]
        public virtual Carrera? Carrera { get; set; }

        [Required(ErrorMessage = "El estado del alumno es obligatorio")]
        [StringLength(20, ErrorMessage = "El estado no puede tener más de 20 caracteres")]
        [DisplayName("Estado del Alumno")]
        public string EstadoAlumno { get; set; }



        [DataType(DataType.Date)]
        [DisplayName("Fecha de Inicio de Solvencia")]
        public DateTime? FechaInicioSolvencia { get; set; }

        [DataType(DataType.Date)]
        [DisplayName("Fecha de Fin de Solvencia")]
        public DateTime? FechaFinSolvencia { get; set; }

        [ScaffoldColumn(false)]
        public bool AlumnoEliminado { get; set; }
    }
}