namespace SistemaContable.Entidad
{
    public enum FormaPago
    {
        Efectivo,
        Transferencia,
        Deposito
    }

    public static class FormaPagoExtensions
    {
        public static string ACodigoBD(this FormaPago forma) => forma.ToString().ToUpperInvariant();
        public static FormaPago DesdeCodigoBD(string codigo) => Enum.Parse<FormaPago>(codigo.Trim(), ignoreCase: true);

        public static string TextoPantalla(this FormaPago forma) => forma switch
        {
            FormaPago.Efectivo => "Efectivo",
            FormaPago.Transferencia => "Transferencia",
            FormaPago.Deposito => "Depósito",
            _ => forma.ToString()
        };
    }
}
