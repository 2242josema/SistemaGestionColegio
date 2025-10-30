namespace SistemaGestionNotas.ViewModels
{
    public class DetallesCursoViewModel
    {
        public Guid CursoId { get; set; }
        public string NombreCurso { get; set; }


        public List<DetalleAlumnoViewModel> Alumnos { get; set; } = new List<DetalleAlumnoViewModel>();


        public bool EsInsolvente { get; set; } = false;
    }
}