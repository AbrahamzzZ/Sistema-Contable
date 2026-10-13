namespace SistemaContable.Entidad
{
    public class Categoria
    {
        public int Id_Categoria { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Tipo { get; set; } = TipoMovimiento.Egreso;
        public int? Id_Padre { get; set; }
        public int Orden { get; set; }
        public bool Activo { get; set; } = true;
        public string? Grupo { get; set; }
        public bool Es_Grupo { get; set; }
        public int Cantidad_Movimientos { get; set; }
        public bool SePuedeEliminar => Cantidad_Movimientos == 0;
        public string TipoTexto => TipoMovimiento.Texto(Tipo);
        public string Estado => Activo ? "Activo" : "Inactivo";
        public override string ToString() => Nombre;
    }
}
