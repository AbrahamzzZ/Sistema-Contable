using Microsoft.Data.SqlClient;
using System.Configuration;

namespace SistemaContable.Datos
{
    public static class Conexion
    {
        private const string NombreCadena = "SistemaContable";

        public static string CadenaConexion
        {
            get
            {
                var cadena = ConfigurationManager.ConnectionStrings[NombreCadena]?.ConnectionString;

                if (string.IsNullOrWhiteSpace(cadena))
                    throw new InvalidOperationException(
                        $"No se encontró la cadena de conexión '{NombreCadena}' en App.config.");

                return cadena;
            }
        }

        /// <summary>Crea una conexión nueva (cerrada). Úsala siempre con 'using'.</summary>
        public static SqlConnection Crear() => new SqlConnection(CadenaConexion);

        /// <summary>
        /// Verifica que el servidor y la base de datos respondan. Se usa al arrancar la aplicación, antes del login.
        /// </summary>
        public static bool Probar(out string mensaje)
        {
            try
            {
                using var cn = Crear();
                cn.Open();
                mensaje = "Conexión exitosa.";
                return true;
            }
            catch (SqlException ex)
            {
                mensaje = ex.Number switch
                {
                    -1 or 2 or 53 => "No se pudo encontrar el servidor SQL Server. Verifique que el servicio esté iniciado y el nombre del servidor en App.config.",
                    4060 => "El servidor responde, pero no existe la base de datos SISTEMA_CONTABLE.",
                    18456 => "Usuario o contraseña de SQL Server incorrectos.",
                    _ => $"Error de conexión ({ex.Number}): {ex.Message}"
                };
                return false;
            }
            catch (InvalidOperationException ex)
            {
                mensaje = ex.Message;
                return false;
            }
        }
    }
}
