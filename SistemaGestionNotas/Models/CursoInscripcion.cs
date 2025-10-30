using SistemaGestionNotas.Models;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaGestionNotas.Models
{
    public class CursoInscripcion
    {
        [Key]
        public Guid CursoInscripcionId { get; set; } = Guid.NewGuid();

        // Relación con Curso
        public Guid CursoId { get; set; }

        [ForeignKey("CursoId")]
        [DisplayName("Curso")]
        public virtual Curso Curso { get; set; }

        // Relación con Alumno
        public Guid AlumnoId { get; set; }

        [ForeignKey("AlumnoId")]
        [DisplayName("Alumno")]
        public virtual Alumno Alumno { get; set; }

        [Required(ErrorMessage = "La fecha de inscripción es obligatoria")]
        [DataType(DataType.Date)]
        [DisplayName("Fecha de Inscripción")]
        public DateTime FechaInscripcion { get; set; }

        [ScaffoldColumn(false)]
        public bool InscripcionEliminada { get; set; }
    }
}
