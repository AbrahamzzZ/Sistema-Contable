using SistemaContable.Utilities;

namespace SistemaContable.Presentacion.Ui
{
    /// <summary>
    /// Mensajes al usuario y manejo uniforme de errores en todas las pantallas.
    /// NegocioException -> advertencia con el mensaje tal cual.
    /// Cualquier otro error -> mensaje genérico + detalle guardado en un archivo de log.
    /// </summary>
    internal static class Mensajes
    {
        private const string Titulo = "Sistema Contable";

        private static readonly string RutaLog = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SistemaContable", "errores.log");

        public static void Info(string mensaje) =>
            MessageBox.Show(mensaje, Titulo, MessageBoxButtons.OK, MessageBoxIcon.Information);

        public static void Advertencia(string mensaje) =>
            MessageBox.Show(mensaje, Titulo, MessageBoxButtons.OK, MessageBoxIcon.Warning);

        public static void Error(string mensaje) =>
            MessageBox.Show(mensaje, Titulo, MessageBoxButtons.OK, MessageBoxIcon.Error);

        /// <summary>Pregunta Sí/No. Por defecto queda seleccionado "No".</summary>
        public static bool Confirmar(string mensaje) =>
            MessageBox.Show(mensaje, Titulo, MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                            MessageBoxDefaultButton.Button2) == DialogResult.Yes;

        public static void MostrarExcepcion(Exception ex)
        {
            if (ex is NegocioException)
            {
                Advertencia(ex.Message);
                return;
            }

            RegistrarEnLog(ex);
            Error("Ocurrió un error inesperado.\n\n" +
                  $"Detalle: {ex.Message}\n\n" +
                  $"La información técnica se guardó en:\n{RutaLog}");
        }

        /// <summary>Ejecuta una acción y muestra el error si falla. Devuelve true si terminó bien.</summary>
        public static bool Intentar(Action accion)
        {
            try
            {
                accion();
                return true;
            }
            catch (Exception ex)
            {
                MostrarExcepcion(ex);
                return false;
            }
        }

        private static void RegistrarEnLog(Exception ex)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(RutaLog)!);
                File.AppendAllText(RutaLog,
                    $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {ex}{Environment.NewLine}{Environment.NewLine}");
            }
            catch
            {
                // Si no se puede escribir el log, no se interrumpe la aplicación
            }
        }
    }
}
