using System.Collections.Generic;

namespace SistemaGestionNotas.Models.ViewModels
{
    // Datos base para cualquier gráfico (etiquetas, valores, colores)
    public class ChartData
    {
        public List<string> Labels { get; set; } = new List<string>();
        public List<decimal> Data { get; set; } = new List<decimal>();
        public List<string> BackgroundColors { get; set; } = new List<string>();
    }

    // Modelo contenedor para los 4 gráficos del dashboard
    public class DashboardViewModel
    {
        public ChartData MatriculaPorCarrera { get; set; } = new ChartData();
        public ChartData RendimientoPromedioPorCurso { get; set; } = new ChartData();
        public ChartData TasaAprobacionPorCarrera { get; set; } = new ChartData();
        public ChartData DistribucionNotas { get; set; } = new ChartData();
    }
}