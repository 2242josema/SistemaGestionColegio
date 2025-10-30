// Ruta: Models/ViewModels/CursoConAlumnosViewModel.cs

using System;
using System.Collections.Generic;

namespace SistemaGestionNotas.Models.ViewModels
{
    public class CursoConAlumnosViewModel
    {
        public Guid CursoId { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string Seccion { get; set; }

        // Esta propiedad tendrá la lista completa de nombres para el PDF
        public List<string> NombresAlumnos { get; set; }
    }
}