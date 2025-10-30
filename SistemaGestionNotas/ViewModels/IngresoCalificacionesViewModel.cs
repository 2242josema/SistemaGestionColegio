namespace SistemaGestionNotas.ViewModels
{
    public class IngresoCalificacionesViewModel
    {
        public Guid CursoId { get; set; }
        public string NombreCurso { get; set; }
        public List<CalificacionAlumnoViewModel> CalificacionesAlumnos { get; set; }

        public IngresoCalificacionesViewModel()
        {
            CalificacionesAlumnos = new List<CalificacionAlumnoViewModel>();
        }
    }
}