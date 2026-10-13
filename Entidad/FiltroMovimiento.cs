namespace SistemaContable.Entidad
{
    public class FiltroMovimiento
    {
        public DateTime Desde { get; set; } = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        public DateTime Hasta { get; set; } = DateTime.Today;
        public string? Tipo { get; set; }            // null = ambos, "I" o "E"
        public int? IdCategoria { get; set; }        // null = todas
        public bool IncluirAnulados { get; set; }
    }
}
