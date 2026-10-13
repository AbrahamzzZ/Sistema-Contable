using SistemaContable.Entidad;
using System;

namespace Entidad
{
    public class Movimiento
    {
        public int Id_Movimiento { get; set; }
        public DateTime Fecha_Movimiento { get; set; } = DateTime.Today;
        public int Id_Categoria { get; set; }
        public decimal Monto { get; set; }
        public string? Descripcion { get; set; }
        public string Numero_Comprobante { get; set; }
        public FormaPago FormaPago { get; set; } = FormaPago.Efectivo;
        public bool Anulado { get; set; }
        public string Motivo_Anulacion { get; set; }
        public int Usuario_Registro { get; set; }
        public DateTime Fecha_Registro { get; set; }

        public int? Usuario_Anulacion { get; set; }
        public DateTime? Fecha_Anulacion { get; set; }
    }
}
