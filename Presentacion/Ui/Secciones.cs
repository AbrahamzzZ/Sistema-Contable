namespace SistemaContable.Presentacion.Ui
{
    /// <summary>Claves de navegación del menú lateral.</summary>
    internal static class Secciones
    {
        public const string Inicio = "inicio";
        public const string RegistrarIngreso = "ingreso";
        public const string RegistrarEgreso = "egreso";
        public const string Movimientos = "movimientos";
        public const string Reportes = "reportes";
        public const string Categorias = "categorias";
        public const string Usuarios = "usuarios";
        public const string MiClave = "mi-clave";
    }

    /// <summary>Vista con un botón principal que se activa con la tecla Enter.</summary>
    internal interface IVistaConAccion
    {
        IButtonControl BotonPrincipal { get; }
    }
}
