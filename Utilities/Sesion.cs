namespace SistemaContable.Utilities
{
    public static class Sesion
    {
        public static int IdUsuario { get; private set; }
        public static string Username { get; private set; } = string.Empty;
        public static string NombreCompleto { get; private set; } = string.Empty;
        public static DateTime? FechaInicio { get; private set; }
        public static bool Activa => IdUsuario > 0;

        public static void Iniciar(int idUsuario, string username, string nombreCompleto)
        {
            if (idUsuario <= 0)
                throw new ArgumentException("El ID de usuario no es válido.", nameof(idUsuario));

            IdUsuario = idUsuario;
            Username = username;
            NombreCompleto = nombreCompleto;
            FechaInicio = DateTime.Now;
        }

        /// <summary>Si el usuario edita su propio nombre, refresca el que se muestra en pantalla.</summary>
        public static void ActualizarNombre(string nombreCompleto)
        {
            if (Activa)
                NombreCompleto = nombreCompleto;
        }

        public static void Cerrar()
        {
            IdUsuario = 0;
            Username = string.Empty;
            NombreCompleto = string.Empty;
            FechaInicio = null;
        }

        /// <summary>Úsalo en Negocio antes de guardar datos: falla si no hay sesión activa.</summary>
        public static int ObtenerIdUsuarioActual()
        {
            if (!Activa)
                throw new NegocioException("No hay una sesión activa. Vuelva a iniciar sesión.");

            return IdUsuario;
        }
    }
}
