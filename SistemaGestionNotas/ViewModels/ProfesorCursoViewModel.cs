using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;

namespace SistemaGestionNotas.ViewModels
{
    public class ProfesorCursoViewModel
    {
        public Guid CursoId { get; set; }
        public string CodigoCurso { get; set; }
        public string NombreCurso { get; set; }
        public string Seccion { get; set; }
        public string NombreCarrera { get; set; }
        public string Ciclo { get; set; }
        public string NombreProfesor { get; set; }

        
    }
}