namespace SistemaContable.Entidad
{
    public class ReporteMensualFila
    {
        public string Grupo { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal Total { get; set; }
    }
}