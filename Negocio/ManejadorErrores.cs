using Microsoft.Data.SqlClient;
using SistemaContable.Utilities;

namespace SistemaContable.Negocio
{
    public class ManejadorErrores
    {
        public static T Ejecutar<T>(Func<T> accion)
        {
            try
            {
                return accion();
            }
            catch (SqlException ex)
            {
                throw Traducir(ex);
            }
        }

        public static void Ejecutar(Action accion)
        {
            try
            {
                accion();
            }
            catch (SqlException ex)
            {
                throw Traducir(ex);
            }
        }

        private static Exception Traducir(SqlException ex)
        {
            // Errores lanzados con THROW 50xxx en los procedimientos: el mensaje ya es para el usuario
            if (ex.Number >= 50000)
                return new NegocioException(ex.Message, ex.Number);

            string? mensaje = ex.Number switch
            {
                -1 or 2 or 53 => "No se pudo conectar con el servidor de base de datos. Verifique que SQL Server esté encendido.",
                -2 => "La base de datos tardó demasiado en responder. Intente nuevamente.",
                4060 => "No se encontró la base de datos SISTEMA_CONTABLE.",
                18456 => "No se pudo iniciar sesión en SQL Server. Revise la cadena de conexión.",
                2627 or 2601 => "Ya existe un registro con esos datos.",
                547 => "La operación no es válida porque afecta datos relacionados.",
                _ => null
            };

            // Error técnico no previsto: se conserva el original para poder diagnosticarlo
            return mensaje is null ? ex : new NegocioException(mensaje, ex);
        }
    }
}
