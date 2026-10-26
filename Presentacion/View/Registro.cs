using SistemaContable.Entidad;
using SistemaContable.Negocio;
using SistemaContable.Presentacion.Ui;
using SistemaContable.Presentacion.Ui.Controles;
using SistemaContable.Utilities;

namespace SistemaContable.Presentacion.View
{
    public partial class Registro : UserControl, IVistaConAccion
    {
        private readonly UsuarioService _usuarioService = new();

        private readonly CampoTexto _usuario = new() { Titulo = "Usuario", Placeholder = "Para iniciar sesión", MaxLength = 50 };
        private readonly CampoTexto _nombre = new() { Titulo = "Nombre completo", Placeholder = "Ej. Juan Pérez", MaxLength = 100 };
        private readonly CampoTexto _correo = new() { Titulo = "Correo electrónico (opcional)", Placeholder = "correo@ejemplo.com", MaxLength = 150 };
        private readonly CampoTexto _clave = new() { Titulo = "Clave", MaxLength = 100, EsClave = true };
        private readonly CampoTexto _confirmar = new() { Titulo = "Confirmar clave", MaxLength = 100, EsClave = true };
        private readonly Aviso _aviso = new();
        private readonly Boton _btnCrear = new() { Text = "Crear usuario", Estilo = EstiloBoton.Primario };
        private readonly Boton _btnVolver = new() { Text = "← Volver al inicio de sesión", Estilo = EstiloBoton.Enlace };

        /// <summary>Se dispara con el username creado.</summary>
        public event EventHandler<string>? UsuarioRegistrado;
        public event EventHandler? Cancelado;

        public IButtonControl BotonPrincipal => _btnCrear;

        public Registro()
        {
            SuspendLayout();
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoScroll = true;

            var filaClaves = new TableLayoutPanel
            {
                ColumnCount = 2,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Margin = new Padding(0)
            };
            filaClaves.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            filaClaves.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            _clave.Margin = new Padding(0, 0, 8, 6);
            _confirmar.Margin = new Padding(8, 0, 0, 6);
            filaClaves.Controls.Add(_clave, 0, 0);
            filaClaves.Controls.Add(_confirmar, 1, 0);

            _btnCrear.AutoSize = false;
            _btnCrear.Height = 46;
            _btnCrear.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            _btnCrear.Margin = new Padding(0, 4, 0, 10);
            _btnVolver.Anchor = AnchorStyles.None;
            _btnVolver.Margin = new Padding(0);

            var columna = Tema.Columna();
            columna.Controls.Add(Tema.Titulo("Crear usuario"));
            columna.Controls.Add(Tema.Subtitulo("Complete los datos para registrar un nuevo usuario."));
            columna.Controls.Add(_aviso);
            columna.Controls.Add(_usuario);
            columna.Controls.Add(_nombre);
            columna.Controls.Add(_correo);
            columna.Controls.Add(filaClaves);
            columna.Controls.Add(Tema.Nota($"Mínimo {Validaciones.LongitudMinimaClave} caracteres, con letras y números."));
            columna.Controls.Add(_btnCrear);
            columna.Controls.Add(_btnVolver);
            Controls.Add(columna);

            _btnCrear.Click += (_, _) => Guardar();
            _btnVolver.Click += (_, _) => Cancelado?.Invoke(this, EventArgs.Empty);

            ResumeLayout(false);
            PerformLayout();
        }

        public void Enfocar() => _usuario.Enfocar();

        /// <summary>Deja el formulario vacío (se reutiliza cada vez que se muestra).</summary>
        public void Reiniciar()
        {
            _aviso.Ocultar();
            _usuario.Limpiar();
            _nombre.Limpiar();
            _correo.Limpiar();
            _clave.Limpiar();
            _confirmar.Limpiar();
            AutoScrollPosition = Point.Empty;
        }

        private void Guardar()
        {
            _aviso.Ocultar();

            var nuevo = new Usuario
            {
                Username = _usuario.Texto,
                Nombre_Completo = _nombre.Texto,
                Correo_Electronico = _correo.Texto
            };

            try
            {
                _usuarioService.Registrar(nuevo, _clave.Texto, _confirmar.Texto);
                UsuarioRegistrado?.Invoke(this, nuevo.Username.Trim());
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
