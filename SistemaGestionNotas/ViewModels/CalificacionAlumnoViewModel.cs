using System.ComponentModel.DataAnnotations;

namespace SistemaGestionNotas.ViewModels
{
    public class CalificacionAlumnoViewModel
    {
        public Guid? CalificacionId { get; set; }
        public Guid AlumnoId { get; set; }
        public string NombreAlumno { get; set; }

        [Range(0, 15, ErrorMessage = "El valor debe ser entre 0 y 15")]
        public int NotaParcialUno { get; set; }

        [Range(0, 15, ErrorMessage = "El valor debe ser entre 0 y 15")]
        public int NotaParcialDos { get; set; }

        [Range(0, 35, ErrorMessage = "El valor debe ser entre 0 y 35")]
        public int NotaZona { get; set; }

        [Range(0, 35, ErrorMessage = "El valor debe ser entre 0 y 35")]
        public int NotaExamen { get; set; }
    }
}