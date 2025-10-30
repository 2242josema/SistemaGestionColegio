using SistemaGestionNotas.Models;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaGestionNotas.Models
{
    public class Calificacion
    {
        [Key]
        public Guid CalificacionId { get; set; } = Guid.NewGuid();


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

        [Required(ErrorMessage = "La nota del parcial uno es obligatoria")]
        [Range(0, 15, ErrorMessage = "La nota debe estar entre 0 y 100")]
        [DisplayName("Nota Parcial Uno")]
        public int NotaParcialUno { get; set; }

        [Required(ErrorMessage = "La nota del parcial dos es obligatoria")]
        [Range(0, 15, ErrorMessage = "La nota debe estar entre 0 y 100")]
        [DisplayName("Nota Parcial Dos")]
        public int NotaParcialDos { get; set; }

        [Required(ErrorMessage = "La nota de zona es obligatoria")]
        [Range(0, 35, ErrorMessage = "La nota debe estar entre 0 y 100")]
        [DisplayName("Nota Zona")]
        public int NotaZona { get; set; }

        [Required(ErrorMessage = "La nota del examen es obligatoria")]
        [Range(0, 35, ErrorMessage = "La nota debe estar entre 0 y 100")]
        [DisplayName("Nota Examen")]
        public int NotaExamen { get; set; }

        
        [Range(0, 100, ErrorMessage = "La nota debe estar entre 0 y 100")]
        [DisplayName("Nota Final")]
        public int NotaFinal { get; set; }

        [ScaffoldColumn(false)]
        public bool CalificacionEliminada { get; set; }
    }
}
