using SistemaContable.Entidad;

namespace SistemaContable.Datos.Repository
{
    public class ReporteRepository
    {
        /// <summary>Totales por grupo y categoría de un mes. tipo: "E" gastos, "I" ingresos.</summary>
        public List<ReporteMensualFila> Mensual(int anio, int mes, string tipo) =>
            DbHelper.Listar<ReporteMensualFila>("SP_REPORTE_MENSUAL", new
            {
                ANIO = anio,
                MES = mes,
                TIPO = tipo
            });

        /// <summary>Categorías con columnas Ene..Dic y Total.</summary>
        public List<ReporteAnualFila> Anual(int anio, string tipo) =>
            DbHelper.Listar<ReporteAnualFila>("SP_REPORTE_ANUAL", new
            {
                ANIO = anio,
                TIPO = tipo
            });

        /// <summary>Los 12 meses del año con ingresos, egresos y saldo.</summary>
        public List<ResumenMensualFila> ResumenAnual(int anio) =>
            DbHelper.Listar<ResumenMensualFila>("SP_REPORTE_RESUMEN_ANUAL", new
            {
                ANIO = anio
            });
    }
}
