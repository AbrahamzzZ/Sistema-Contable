namespace SistemaContable.Entidad
{
    public class MovimientoDetalle
    {
        public int Id_Movimiento { get; set; }
        public DateTime Fecha_Movimiento { get; set; }
        public string Tipo { get; set; } = string.Empty;          // "I" o "E"
        public string Grupo { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public int Id_Categoria { get; set; }
        public decimal Monto { get; set; }
        public string? Descripcion { get; set; }
        public string? Numero_Comprobante { get; set; }
        public FormaPago FormaPago { get; set; }
        public bool Anulado { get; set; }
        public string? Motivo_Anulacion { get; set; }
        public string RegistradoPor { get; set; } = string.Empty;
        public DateTime Fecha_Registro { get; set; }
        public string? Anulado_Por { get; set; }
        public DateTime? Fecha_Anulacion { get; set; }
        public string TipoTexto => TipoMovimiento.Texto(Tipo);
        public string FormaPagoTexto => FormaPago.TextoPantalla();
        public string Estado => Anulado ? "Anulado" : "Vigente";
    }
}
