using SistemaContable.Negocio;
using SistemaContable.Presentacion.Forms;
using SistemaContable.Presentacion.Ui;

namespace SistemaContable.Presentacion
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (_, e) => Mensajes.MostrarExcepcion(e.Exception);
            AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            {
                if (e.ExceptionObject is Exception ex)
                    Mensajes.MostrarExcepcion(ex);
            };

            if (!new SistemaService().ProbarConexion(out string mensaje))
            {
                Mensajes.Error("No se pudo conectar con la base de datos.\n\n" + mensaje);
                return;
            }

            while (true)
            {
                using (var acceso = new FrmAcceso())
                {
                    if (acceso.ShowDialog() != DialogResult.OK)
                        return;
                }

                using var principal = new FrmPrincipal();
                Application.Run(principal);

                if (!principal.CerrarSesionSolicitado)
                    return;

                new UsuarioService().CerrarSesion();
            }
        }
    }
}
