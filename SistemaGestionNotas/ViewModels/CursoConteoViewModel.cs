// Ruta: Models/ViewModels/CursoConteoViewModel.cs

namespace SistemaGestionNotas.Models.ViewModels
{
    public class CursoConteoViewModel
    {
        public Guid CursoId { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string Seccion { get; set; }
        public int CantidadAlumnos { get; set; }
    }
}