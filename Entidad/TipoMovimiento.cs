namespace SistemaContable.Entidad
{
    public static class TipoMovimiento
    {
        public const string Ingreso = "I";
        public const string Egreso = "E";

        public static bool EsValido(string? tipo) => tipo is Ingreso or Egreso;

        public static string Texto(string? tipo) => tipo switch
        {
            Ingreso => "Ingreso",
            Egreso => "Egreso",
            _ => string.Empty
        };
    }
}
