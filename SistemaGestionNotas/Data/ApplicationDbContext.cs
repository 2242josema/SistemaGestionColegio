using SistemaGestionNotas.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace SistemaGestionNotas.Data
{
    public class ApplicationDbContext : IdentityDbContext<UsuarioAplicacion, RolAplicacion, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Carrera> Carreras { get; set; }
        public DbSet<Profesor> Profesores { get; set; }
        public DbSet<Alumno> Alumnos { get; set; }
        public DbSet<Curso> Cursos { get; set; }
        public DbSet<Calificacion> Calificaciones { get; set; }
        public DbSet<CursoInscripcion> CursoInscripciones { get; set; }


    }
}
