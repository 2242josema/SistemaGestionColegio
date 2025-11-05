using Microsoft.AspNetCore.Mvc;
using SistemaGestionNotas.Data;
using SistemaGestionNotas.Models.ViewModels;
using System.Linq;
using System.Collections.Generic;
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace SistemaGestionNotas.Controllers
{
[Authorize(Roles = "Administrador")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const int NotaMinimaAprobacion = 61;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public JsonResult GetMatriculaPorCarrera()
        {
            var data = _context.Alumnos
                .Include(a => a.Carrera)
                .Where(a => a.AlumnoEliminado == false && a.Carrera != null)
                .GroupBy(a => a.Carrera.Nombre)
                .Select(g => new
                {
                    CarreraNombre = g.Key,
                    TotalAlumnos = g.Count()
                })
                .ToList();

            var chartData = new ChartData
            {
                Labels = data.Select(x => x.CarreraNombre).ToList(),
                Data = data.Select(x => (decimal)x.TotalAlumnos).ToList(),
                BackgroundColors = data.Select(x => $"rgba({new Random().Next(0, 255)}, {new Random().Next(0, 255)}, {new Random().Next(0, 255)}, 0.8)").ToList()
            };

            return Json(chartData);
        }

        [HttpGet]
        public JsonResult GetRendimientoPromedioPorCurso()
        {
            var data = _context.Calificaciones
                .Include(c => c.Curso)
                .Where(c => c.CalificacionEliminada == false && c.Curso != null)
                .GroupBy(c => c.Curso.Nombre)
                .Select(g => new
                {
                    CursoNombre = g.Key,
                    PromedioNota = Math.Round(g.Average(c => (decimal)c.NotaFinal), 2)
                })
                .OrderByDescending(r => r.PromedioNota)
                .Take(10)
                .ToList();

            var chartData = new ChartData
            {
                Labels = data.Select(x => x.CursoNombre).ToList(),
                Data = data.Select(x => x.PromedioNota).ToList(),
                BackgroundColors = data.Select(x => x.PromedioNota >= NotaMinimaAprobacion ?
                                                    "rgba(40, 167, 69, 0.8)" :
                                                    "rgba(220, 53, 69, 0.8)").ToList()
            };

            return Json(chartData);
        }

        [HttpGet]
        public JsonResult GetTasaAprobacionPorCarrera()
        {
            var data = _context.Calificaciones
                .Include(c => c.Alumno)
                .ThenInclude(a => a.Carrera)
                .Where(c =>
                    c.CalificacionEliminada == false &&
                    c.Alumno.AlumnoEliminado == false &&
                    c.Alumno.Carrera != null)
                .GroupBy(c => c.Alumno.Carrera.Nombre)
                .Select(g => new
                {
                    CarreraNombre = g.Key,
                    TotalCalificaciones = g.Count(),
                    Aprobadas = g.Count(c => c.NotaFinal >= NotaMinimaAprobacion)
                })
                .ToList();

            var random = new Random();
            var chartData = new ChartData();

            foreach (var item in data)
            {
                decimal tasaAprobacion = 0;
                if (item.TotalCalificaciones > 0)
                {
                    tasaAprobacion = Math.Round((decimal)item.Aprobadas * 100 / item.TotalCalificaciones, 2);
                }

                chartData.Labels.Add(item.CarreraNombre);
                chartData.Data.Add(tasaAprobacion);

                string randomColor = $"rgba({random.Next(0, 255)}, {random.Next(0, 255)}, {random.Next(0, 255)}, 0.8)";
                chartData.BackgroundColors.Add(randomColor);
            }

            return Json(chartData);
        }

        [HttpGet]
        public JsonResult GetAlumnosPorEstadoAcademico()
        {
            var promediosAlumnos = _context.Calificaciones
                .Where(c => c.CalificacionEliminada == false)
                .GroupBy(c => c.AlumnoId)
                .Select(g => new
                {
                    AlumnoId = g.Key,
                    PromedioGeneral = g.Average(c => c.NotaFinal)
                })
                .ToList();

            int UmbralReprobado = 60;
            int UmbralExcelente = 90;

            int reprobadoCount = promediosAlumnos.Count(p => p.PromedioGeneral <= UmbralReprobado);
            int excelenteCount = promediosAlumnos.Count(p => p.PromedioGeneral >= UmbralExcelente);
            int aprobadoCount = promediosAlumnos.Count - reprobadoCount - excelenteCount;

            var labels = new List<string> {
                "Reprobación (< 61)",
                "Aprobado (61 - 89)",
                "Excelente (>= 90)"
            };
            var data = new List<decimal> {
                reprobadoCount,
                aprobadoCount,
                excelenteCount
            };
            var colors = new List<string> {
                "rgba(220, 53, 69, 0.8)",
                "rgba(40, 167, 69, 0.8)",
                "rgba(30, 144, 255, 0.8)"
            };

            var chartData = new ChartData
            {
                Labels = labels,
                Data = data,
                BackgroundColors = colors
            };

            return Json(chartData);
        }
    }
}