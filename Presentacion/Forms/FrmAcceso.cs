using SistemaContable.Presentacion.Ui;
using SistemaContable.Presentacion.Ui.Controles;
using SistemaContable.Presentacion.View;

namespace SistemaContable.Presentacion.Forms
{
    public partial class FrmAcceso : Form
    {
        private readonly Panel _contenido = new()
        {
            Dock = DockStyle.Fill,
            BackColor = Tema.Superficie,
            Padding = new Padding(56, 44, 56, 32)
        };

        private readonly Login _vistaLogin = new();
        private readonly Registro _vistaRegistro = new();

        public FrmAcceso()
        {
            SuspendLayout();
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            Text = "Sistema Contable - Iniciar sesión";
            Font = Tema.Fuente;
            BackColor = Tema.Superficie;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(940, 700);
            Recursos.AplicarIcono(this);

            var marca = new PanelMarca { Dock = DockStyle.Left, Width = 400 };
            Controls.Add(_contenido);
            Controls.Add(marca);

            _vistaLogin.IngresoExitoso += (_, _) => DialogResult = DialogResult.OK;
            _vistaLogin.RegistroSolicitado += (_, _) => MostrarRegistro();
            _vistaLogin.SalidaSolicitada += (_, _) => DialogResult = DialogResult.Cancel;
            _vistaRegistro.UsuarioRegistrado += (_, username) => MostrarLogin(username);
            _vistaRegistro.Cancelado += (_, _) => MostrarLogin();

            Load += (_, _) => PrepararVistas();
            Shown += (_, _) => _vistaLogin.Enfocar();

            ResumeLayout(false);
            PerformLayout();
        }

        /// <summary>Doble búfer de toda la ventana: evita parpadeos al cambiar de vista.</summary>
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;   // WS_EX_COMPOSITED
                return cp;
            }
        }

        private void PrepararVistas()
        {
            _vistaRegistro.Dock = DockStyle.Fill;
            _vistaLogin.Dock = DockStyle.Fill;
            _contenido.Controls.Add(_vistaRegistro);
            _contenido.Controls.Add(_vistaLogin);

            MostrarLogin();
        }

        private void MostrarLogin(string? usernameCreado = null)
        {
            Activar(_vistaLogin, _vistaRegistro, "Sistema Contable - Iniciar sesión");

            if (usernameCreado is not null)
                _vistaLogin.PrecargarUsuario(usernameCreado);
            else if (Visible)
                _vistaLogin.Enfocar();

            _vistaRegistro.Reiniciar();
        }

        private void MostrarRegistro()
        {
            _vistaRegistro.Reiniciar();
            Activar(_vistaRegistro, _vistaLogin, "Sistema Contable - Crear usuario");
            _vistaRegistro.Enfocar();
        }

        private void Activar(UserControl mostrar, UserControl ocultar, string titulo)
        {
            mostrar.Visible = true;
            mostrar.BringToFront();
            ocultar.Visible = false;

            AcceptButton = (mostrar as IVistaConAccion)?.BotonPrincipal;
            Text = titulo;
        }
    }
}
