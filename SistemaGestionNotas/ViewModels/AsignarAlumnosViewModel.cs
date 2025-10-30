using System.Collections.Generic;
using System;

namespace SistemaGestionNotas.Models.ViewModels
{
    // Este modelo representa a un solo alumno en la lista de asignación.
    public class AlumnoAsignacionViewModel
    {
        public Guid AlumnoId { get; set; }
        public string Nombre { get; set; }
        public bool EstaAsignado { get; set; }
    }

    // Este es el modelo principal para la vista de EDICIÓN de un curso.
    public class AsignarAlumnosViewModel
    {
        // El ID del curso que estamos editando.
        public Guid CursoId { get; set; }

        // El nombre del curso para mostrarlo en la vista.
        public string NombreCurso { get; set; }

        // La lista de todos los alumnos con su checkbox.
        public List<AlumnoAsignacionViewModel> Alumnos { get; set; }

        public AsignarAlumnosViewModel()
        {
            Alumnos = new List<AlumnoAsignacionViewModel>();
        }
    }
}