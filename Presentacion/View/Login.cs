using SistemaContable.Negocio;
using SistemaContable.Presentacion.Ui;
using SistemaContable.Presentacion.Ui.Controles;
using SistemaContable.Utilities;

namespace SistemaContable.Presentacion.View
{
    public partial class Login : UserControl, IVistaConAccion
    {
        private const int MaximoIntentos = 5;

        private readonly UsuarioService _usuarioService = new();

        private readonly CampoTexto _usuario = new() { Titulo = "Usuario", Placeholder = "Ej. tesorero", MaxLength = 50 };
        private readonly CampoTexto _clave = new() { Titulo = "Clave", Placeholder = "Ingrese su clave", MaxLength = 100, EsClave = true };
        private readonly Aviso _aviso = new();
        private readonly Boton _btnIngresar = new() { Text = "Ingresar", Estilo = EstiloBoton.Primario };
        private readonly Boton _btnRegistrar = new() { Text = "Crear un usuario nuevo", Estilo = EstiloBoton.Enlace };

        private int _intentosFallidos;

        public event EventHandler? IngresoExitoso;
        public event EventHandler? RegistroSolicitado;
        public event EventHandler? SalidaSolicitada;

        public IButtonControl BotonPrincipal => _btnIngresar;

        public Login()
        {
            SuspendLayout();
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoScroll = true;

            _btnIngresar.AutoSize = false;
            _btnIngresar.Height = 46;
            _btnIngresar.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            _btnIngresar.Margin = new Padding(0, 8, 0, 14);
            _btnRegistrar.Anchor = AnchorStyles.None; 
            _btnRegistrar.Margin = new Padding(0);

            var columna = Tema.Columna();
            columna.Controls.Add(Tema.Titulo("Bienvenido"));
            columna.Controls.Add(Tema.Subtitulo("Inicie sesión con su usuario y clave."));
            columna.Controls.Add(_aviso);
            columna.Controls.Add(_usuario);
            columna.Controls.Add(_clave);
            columna.Controls.Add(_btnIngresar);
            columna.Controls.Add(_btnRegistrar);
            Controls.Add(columna);

            _btnIngresar.Click += (_, _) => Ingresar();
            _btnRegistrar.Click += (_, _) => RegistroSolicitado?.Invoke(this, EventArgs.Empty);

            ResumeLayout(false);
            PerformLayout();
        }

        public void Enfocar()
        {
            if (string.IsNullOrWhiteSpace(_usuario.Texto))
                _usuario.Enfocar();
            else
                _clave.Enfocar();
        }

        /// <summary>Después de registrar un usuario: deja escrito el username y pide la clave.</summary>
        public void PrecargarUsuario(string username)
        {
            _usuario.Texto = username;
            _clave.Limpiar();
            _aviso.MostrarExito("Usuario creado correctamente. Ingrese su clave para continuar.");
            _clave.Enfocar();
        }

        private void Ingresar()
        {
            _aviso.Ocultar();
            _btnIngresar.Enabled = false;
            _btnIngresar.Text = "Ingresando...";
            _btnIngresar.Refresh();
            UseWaitCursor = true;

            try
            {
                _usuarioService.IniciarSesion(_usuario.Texto, _clave.Texto);
                IngresoExitoso?.Invoke(this, EventArgs.Empty);
            }
            catch (NegocioException ex)
            {
                if (!string.IsNullOrWhiteSpace(_usuario.Texto) && _clave.Texto.Length > 0)
                    _intentosFallidos++;

                if (_intentosFallidos >= MaximoIntentos)
                {
                    Mensajes.Error("Demasiados intentos fallidos. La aplicación se cerrará.");
                    SalidaSolicitada?.Invoke(this, EventArgs.Empty);
                    return;
                }

                _aviso.MostrarError(ex.Message);
                _clave.Limpiar();
                _clave.Enfocar();
            }
            catch (Exception ex)
            {
                Mensajes.MostrarExcepcion(ex);
            }
            finally
            {
                UseWaitCursor = false;
                _btnIngresar.Enabled = true;
                _btnIngresar.Text = "Ingresar";
            }
        }
    }
}
