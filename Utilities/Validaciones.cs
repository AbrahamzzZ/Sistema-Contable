using System.Net.Mail;

namespace SistemaContable.Utilities
{
    public static class Validaciones
    {
        public const int LongitudMinimaClave = 8;

        public static bool EsCorreoValido(string? correo)
        {
            if (string.IsNullOrWhiteSpace(correo))
                return false;

            try
            {
                var direccion = new MailAddress(correo.Trim());
                return direccion.Address == correo.Trim() && direccion.Host.Contains('.');
            }
            catch (FormatException)
            {
                return false;
            }
        }

        /// <summary>Mínimo 8 caracteres, con al menos una letra y un número.</summary>
        public static string? ValidarClave(string? clave)
        {
            if (string.IsNullOrEmpty(clave))
                return "La clave es obligatoria.";

            if (clave.Length < LongitudMinimaClave)
                return $"La clave debe tener al menos {LongitudMinimaClave} caracteres.";

            if (!clave.Any(char.IsLetter) || !clave.Any(char.IsDigit))
                return "La clave debe contener al menos una letra y un número.";

            if (clave.Contains(' '))
                return "La clave no puede contener espacios.";

            return null;
        }

        /// <summary>Usuario: 4 a 50 caracteres, solo letras, números, punto y guion bajo.</summary>
        public static string? ValidarUsername(string? username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return "El nombre de usuario es obligatorio.";

            username = username.Trim();

            if (username.Length is < 4 or > 50)
                return "El nombre de usuario debe tener entre 4 y 50 caracteres.";

            if (!username.All(c => char.IsLetterOrDigit(c) || c is '.' or '_'))
                return "El nombre de usuario solo puede tener letras, números, punto (.) y guion bajo (_).";

            return null;
        }
    }
}
