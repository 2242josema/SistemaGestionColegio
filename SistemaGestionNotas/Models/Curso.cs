using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace SistemaGestionNotas.Models
{
    public class Curso
    {
        [Key]
        public Guid CursoId { get; set; } = Guid.NewGuid();

        [Required(ErrorMessage = "El codigo del curso es obligatorio")]
        [StringLength(20, ErrorMessage = "El codigo no puede tener mas de 20 caracteres")]
        [DisplayName("Código del Curso")]
        // 👇 2. AÑADIR ESTE ATRIBUTO
        [Remote(action: "ValidateCodigo", controller: "Cursos", AdditionalFields = nameof(CursoId), ErrorMessage = "Este código de curso ya existe.")]
        public string Codigo { get; set; }

        [Required(ErrorMessage = "El nombre del curso es obligatorio")]
        [StringLength(50, ErrorMessage = "El codigo no puede tener mas de 50 caracteres")]
        [DisplayName("Nombre del Curso")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "La sección es obligatoria")]
        [StringLength(10, ErrorMessage = "La sección no puede tener más de 10 caracteres")]
        [DisplayName("Sección")]
        public string Seccion { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una carrera.")]
        [DisplayName("Carrera")]
        public Guid CarreraId { get; set; }

        [ForeignKey("CarreraId")]
        public virtual Carrera? Carrera { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un profesor.")]
        [DisplayName("Profesor")]
        public Guid ProfesorId { get; set; }

        [ForeignKey("ProfesorId")]
        public virtual Profesor? Profesor { get; set; }

        [Required(ErrorMessage = "El ciclo es obligatorio")]
        [StringLength(20, ErrorMessage = "El ciclo no puede tener mas de 20 caracteres")]
        [DisplayName("Ciclo")]
        public string Ciclo { get; set; }

        [Required(ErrorMessage = "El estado del curso es obligatorio")]
        [StringLength(20, ErrorMessage = "El estado no puede tener mas de 20 caracteres")]
        [DisplayName("Estado del Curso")]
        public string EstadoCurso { get; set; }

        [ScaffoldColumn(false)]
        public bool CursoEliminado { get; set; }


        public virtual ICollection<CursoInscripcion> CursoInscripciones { get; set; }


        public Curso()
        {
            CursoInscripciones = new HashSet<CursoInscripcion>();
        }
        
    }
}