using SistemaContable.Datos.Repository;
using SistemaContable.Entidad;
using SistemaContable.Utilities;

namespace SistemaContable.Negocio
{
    public class ReporteService
    {
        private const int AnioMinimo = 2000;

        private readonly ReporteRepository _repo = new();

        /// <summary>Totales por grupo y categoría de un mes. tipo: "E" gastos (por defecto), "I" ingresos.</summary>
        public List<ReporteMensualFila> Mensual(int anio, int mes, string tipo = TipoMovimiento.Egreso)
        {
            ValidarAnio(anio);
            ValidarTipo(tipo);

            if (mes is < 1 or > 12)
                throw new NegocioException("Seleccione un mes válido.");

            return ManejadorErrores.Ejecutar(() => _repo.Mensual(anio, mes, tipo));
        }

        /// <summary>Categorías con columnas Ene..Dic y Total.</summary>
        public List<ReporteAnualFila> Anual(int anio, string tipo = TipoMovimiento.Egreso)
        {
            ValidarAnio(anio);
            ValidarTipo(tipo);

            return ManejadorErrores.Ejecutar(() => _repo.Anual(anio, tipo));
        }

        /// <summary>Los 12 meses del año con ingresos, egresos y saldo.</summary>
        public List<ResumenMensualFila> ResumenAnual(int anio)
        {
            ValidarAnio(anio);
            return ManejadorErrores.Ejecutar(() => _repo.ResumenAnual(anio));
        }

        /// <summary>Años disponibles para los ComboBox de reportes (del actual hacia atrás).</summary>
        public static List<int> AniosDisponibles(int cantidad = 10) => Enumerable.Range(0, cantidad).Select(i => DateTime.Today.Year - i).Where(a => a >= AnioMinimo).ToList();

        private static void ValidarAnio(int anio)
        {
            if (anio < AnioMinimo || anio > DateTime.Today.Year)
                throw new NegocioException($"Seleccione un año entre {AnioMinimo} y {DateTime.Today.Year}.");
        }

        private static void ValidarTipo(string tipo)
        {
            if (!TipoMovimiento.EsValido(tipo))
                throw new NegocioException("Tipo de reporte no válido.");
        }
    }
}
