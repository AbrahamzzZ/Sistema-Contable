using System.Globalization;

namespace SistemaContable.Entidad
{
    public class ResumenMensualFila
    {
        private static readonly CultureInfo Es = new("es-EC");

        public int Mes { get; set; }
        public decimal Ingresos { get; set; }
        public decimal Egresos { get; set; }
        public decimal Saldo { get; set; }

        public string NombreMes => Mes is >= 1 and <= 12 ? Es.TextInfo.ToTitleCase(Es.DateTimeFormat.GetMonthName(Mes)) : string.Empty;
    }
}