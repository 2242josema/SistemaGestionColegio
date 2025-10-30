using System;
using System.Collections.Generic;
using SistemaGestionNotas.Models;

namespace SistemaGestionNotas.Models.ViewModels
{
    public class CursoInscripcionesDetailsViewModel
    {
        public Guid CursoId { get; set; }
        public string NombreCurso { get; set; }
        public string CodigoCurso { get; set; }
        public string Seccion { get; set; }

        // 👇 CAMBIO AQUÍ: La lista ahora es de tipo CursoInscripcion
        public List<CursoInscripcion> AlumnosInscritos { get; set; }

        public CursoInscripcionesDetailsViewModel()
        {
            // 👇 CAMBIO AQUÍ: Se inicializa la nueva lista
            AlumnosInscritos = new List<CursoInscripcion>();
        }
    }
}