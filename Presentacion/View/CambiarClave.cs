using SistemaContable.Negocio;
using SistemaContable.Presentacion.Ui;
using SistemaContable.Presentacion.Ui.Controles;
using SistemaContable.Utilities;

namespace SistemaContable.Presentacion.View
{
    public partial class CambiarClave : UserControl, IVistaConAccion
    {
        private readonly UsuarioService _usuarioService = new();

        private readonly CampoTexto _actual = new() { Titulo = "Clave actual", MaxLength = 100, EsClave = true };
        private readonly CampoTexto _nueva = new() { Titulo = "Clave nueva", MaxLength = 100, EsClave = true };
        private readonly CampoTexto _confirmar = new() { Titulo = "Confirmar clave nueva", MaxLength = 100, EsClave = true };
        private readonly Aviso _aviso = new();
        private readonly Boton _btnGuardar = new() { Text = "Guardar cambios", Estilo = EstiloBoton.Primario };

        public IButtonControl BotonPrincipal => _btnGuardar;

        public CambiarClave()
        {
            SuspendLayout();
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoScroll = true;

            var tarjeta = new PanelTarjeta { Width = 560, Location = new Point(0, 0) };

            var columna = Tema.Columna();
            columna.Controls.Add(Tema.Seccion("Actualice su clave de acceso"));
            columna.Controls.Add(Tema.Nota($"Usuario: {Sesion.NombreCompleto} ({Sesion.Username})"));
            columna.Controls.Add(_aviso);
            columna.Controls.Add(_actual);
            columna.Controls.Add(_nueva);
            columna.Controls.Add(_confirmar);
            columna.Controls.Add(Tema.Nota($"Mínimo {Validaciones.LongitudMinimaClave} caracteres, con letras y números."));
            columna.Controls.Add(_btnGuardar);

            tarjeta.Controls.Add(columna);
            tarjeta.AjustarAlContenido(columna);
            Controls.Add(tarjeta);

            _btnGuardar.Click += (_, _) => Guardar();

            ResumeLayout(false);
            PerformLayout();
        }

        private void Guardar()
        {
            _aviso.Ocultar();

            try
            {
                _usuarioService.CambiarMiClave(_actual.Texto, _nueva.Texto, _confirmar.Texto);
                _actual.Limpiar();
                _nueva.Limpiar();
                _confirmar.Limpiar();
                _aviso.MostrarExito("La clave se cambió correctamente.");
            }
            catch (NegocioException ex)
            {
                _aviso.MostrarError(ex.Message);
            }
            catch (Exception ex)
            {
                Mensajes.MostrarExcepcion(ex);
            }
        }
    }
}
