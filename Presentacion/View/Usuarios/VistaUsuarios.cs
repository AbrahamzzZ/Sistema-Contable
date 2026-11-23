using SistemaContable.Entidad;
using SistemaContable.Negocio;
using SistemaContable.Presentacion.Ui;
using SistemaContable.Presentacion.Ui.Controles;
using SistemaContable.Utilities;

namespace SistemaContable.Presentacion.View.Usuarios
{
    public class VistaUsuarios : UserControl, IVistaConAccion
    {
        private enum Modo { Editar, Nuevo, RestablecerClave }

        private readonly UsuarioService _service = new();

        private readonly CheckBox _chkInactivos = new() { Text = "Mostrar inactivos", AutoSize = true, Checked = true, Margin = new Padding(24, 10, 0, 0) };
        private readonly Boton _btnNuevo = new() { Text = "+ Nuevo usuario", Estilo = EstiloBoton.Primario };

        private readonly DataGridView _grilla = new();

        private readonly Label _lblTitulo = Tema.Seccion(string.Empty);
        private readonly Label _lblInfo = Tema.Nota(string.Empty);
        private readonly Aviso _aviso = new();
        private readonly CampoTexto _username = new() { Titulo = "Usuario", Placeholder = "Para iniciar sesión", MaxLength = 50 };
        private readonly CampoTexto _nombre = new() { Titulo = "Nombre completo", MaxLength = 100 };
        private readonly CampoTexto _correo = new() { Titulo = "Correo electrónico (opcional)", MaxLength = 150 };
        private readonly CampoTexto _clave = new() { Titulo = "Clave", MaxLength = 100, EsClave = true };
        private readonly CampoTexto _confirmar = new() { Titulo = "Confirmar clave", MaxLength = 100, EsClave = true };
        private readonly Boton _btnGuardar = new() { Text = "Guardar cambios", Estilo = EstiloBoton.Primario };
        private readonly Boton _btnCancelar = new() { Text = "Cancelar", Estilo = EstiloBoton.Secundario };
        private readonly Boton _btnRestablecer = new() { Text = "Restablecer clave", Estilo = EstiloBoton.Secundario };
        private readonly Boton _btnEstado = new() { Text = "Desactivar", Estilo = EstiloBoton.Secundario };

        private List<Usuario> _datos = new();
        private Modo _modo = Modo.Editar;
        private bool _recargando;

        public IButtonControl BotonPrincipal => _btnGuardar;

        private Usuario? Seleccionado => _grilla.CurrentRow?.DataBoundItem as Usuario;

        public VistaUsuarios()
        {
            SuspendLayout();
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoScroll = true;

            _lblInfo.MaximumSize = new Size(340, 0);

            var barra = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new Padding(0, 0, 0, 16) };
            barra.Controls.Add(_btnNuevo);
            barra.Controls.Add(_chkInactivos);

            var cuerpo = new TableLayoutPanel
            {
                ColumnCount = 2,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Margin = new Padding(0)
            };
            cuerpo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            cuerpo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            cuerpo.Controls.Add(CrearListado(), 0, 0);
            cuerpo.Controls.Add(CrearPanelEdicion(), 1, 0);

            var columna = Tema.Columna();
            columna.Controls.Add(barra);
            columna.Controls.Add(cuerpo);
            Controls.Add(columna);

            _chkInactivos.CheckedChanged += (_, _) => Recargar(Seleccionado?.Id_Usuario);
            _btnNuevo.Click += (_, _) => ModoNuevo();
            _grilla.SelectionChanged += (_, _) =>
            {
                if (!_recargando) _aviso.Ocultar();
                ModoEditar(Seleccionado);
            };
            _grilla.CellFormatting += FormatearCelda;
            _btnGuardar.Click += (_, _) => Guardar();
            _btnCancelar.Click += (_, _) => { _aviso.Ocultar(); ModoEditar(Seleccionado); };
            _btnRestablecer.Click += (_, _) => ModoRestablecer();
            _btnEstado.Click += (_, _) => CambiarEstado();

            ResumeLayout(false);
            PerformLayout();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Recargar(Sesion.IdUsuario);
        }

        private Control CrearListado()
        {
            var tarjeta = new PanelTarjeta
            {
                Height = 520,
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top,
                Padding = new Padding(16, 12, 16, 12),
                Margin = new Padding(0, 0, 10, 0)
            };

            Tema.EstilizarGrilla(_grilla);
            _grilla.Dock = DockStyle.Fill;
            Tema.AgregarColumna(_grilla, nameof(Usuario.Username), "Usuario", 130);
            Tema.AgregarColumna(_grilla, nameof(Usuario.Nombre_Completo), "Nombre", 0);
            Tema.AgregarColumna(_grilla, nameof(Usuario.Ultimo_Acceso), "Último acceso", 140, "dd/MM/yyyy HH:mm");
            Tema.AgregarColumna(_grilla, nameof(Usuario.Estado), "Estado", 85);

            tarjeta.Controls.Add(_grilla);
            return tarjeta;
        }

        private Control CrearPanelEdicion()
        {
            var tarjeta = new PanelTarjeta
            {
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top,
                Padding = new Padding(24, 20, 24, 20),
                Margin = new Padding(10, 0, 0, 0)
            };

            var botonesGuardar = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new Padding(0, 4, 0, 0) };
            botonesGuardar.Controls.Add(_btnGuardar);
            botonesGuardar.Controls.Add(_btnCancelar);

            var botonesAcciones = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new Padding(0, 16, 0, 0) };
            botonesAcciones.Controls.Add(_btnRestablecer);
            botonesAcciones.Controls.Add(_btnEstado);

            var contenido = Tema.Columna();
            contenido.Controls.Add(_lblTitulo);
            contenido.Controls.Add(_lblInfo);
            contenido.Controls.Add(_aviso);
            contenido.Controls.Add(_username);
            contenido.Controls.Add(_nombre);
            contenido.Controls.Add(_correo);
            contenido.Controls.Add(_clave);
            contenido.Controls.Add(_confirmar);
            contenido.Controls.Add(botonesGuardar);
            contenido.Controls.Add(botonesAcciones);

            tarjeta.Controls.Add(contenido);
            tarjeta.AjustarAlContenido(contenido);
            return tarjeta;
        }

        private void FormatearCelda(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.CellStyle is null)
                return;

            if (_grilla.Rows[e.RowIndex].DataBoundItem is not Usuario u)
                return;

            string columna = _grilla.Columns[e.ColumnIndex].Name;

            if (columna == nameof(Usuario.Nombre_Completo) && u.Id_Usuario == Sesion.IdUsuario)
            {
                e.Value = $"{u.Nombre_Completo} (usted)";
                e.FormattingApplied = true;
            }

            if (columna == nameof(Usuario.Ultimo_Acceso) && u.Ultimo_Acceso is null)
            {
                e.Value = "Nunca";
                e.FormattingApplied = true;
            }

            if (!u.Activo)
            {
                e.CellStyle.ForeColor = Tema.TextoDeshabilitado;
                e.CellStyle.SelectionForeColor = Tema.TextoDeshabilitado;
            }
        }

        private void Recargar(int? idSeleccionar)
        {
            try
            {
                _recargando = true;
                _datos = _service.Listar(_chkInactivos.Checked);
                _grilla.DataSource = null;
                _grilla.DataSource = _datos;

                if (idSeleccionar is int id)
                    Seleccionar(id);

                if (_datos.Count == 0)
                    ModoEditar(null);
            }
            catch (Exception ex)
            {
                Mensajes.MostrarExcepcion(ex);
            }
            finally
            {
                _recargando = false;
            }
        }

        private void Seleccionar(int idUsuario)
        {
            foreach (DataGridViewRow fila in _grilla.Rows)
            {
                if (fila.DataBoundItem is Usuario u && u.Id_Usuario == idUsuario)
                {
                    _grilla.CurrentCell = fila.Cells[0];
                    return;
                }
            }
        }

        private void ModoEditar(Usuario? u)
        {
            _modo = Modo.Editar;
            _clave.Visible = false;
            _confirmar.Visible = false;
            _btnCancelar.Visible = false;

            if (u is null)
            {
                _lblTitulo.Text = "Sin selección";
                _lblInfo.Text = "Elija un usuario de la lista o cree uno nuevo.";
                _username.Visible = _nombre.Visible = _correo.Visible = false;
                _btnGuardar.Visible = _btnRestablecer.Visible = _btnEstado.Visible = false;
                return;
            }

            bool esUsted = u.Id_Usuario == Sesion.IdUsuario;

            _lblTitulo.Text = "Editar usuario";
            _lblInfo.Text = esUsted
                ? "Este es su usuario. Para cambiar su clave use «Cambiar mi clave» en el menú."
                : $"Último acceso: {u.Ultimo_Acceso?.ToString("dd/MM/yyyy HH:mm") ?? "nunca"}  ·  {u.Estado}";

            _username.Visible = true;
            _username.Texto = u.Username;
            _username.SoloLectura = true;
            _nombre.Visible = true;
            _nombre.Texto = u.Nombre_Completo;
            _correo.Visible = true;
            _correo.Texto = u.Correo_Electronico ?? string.Empty;

            _btnGuardar.Visible = true;
            _btnGuardar.Text = "Guardar cambios";
            _btnRestablecer.Visible = true;
            _btnRestablecer.Enabled = !esUsted && u.Activo;
            _btnEstado.Visible = true;
            _btnEstado.Text = u.Activo ? "Desactivar" : "Activar";
            _btnEstado.Enabled = !esUsted;  
        }

        private void ModoNuevo()
        {
            _modo = Modo.Nuevo;
            _aviso.Ocultar();

            _lblTitulo.Text = "Nuevo usuario";
            _lblInfo.Text = $"La clave debe tener mínimo {Validaciones.LongitudMinimaClave} caracteres, con letras y números.";

            _username.Visible = true;
            _username.SoloLectura = false;
            _username.Limpiar();
            _nombre.Visible = true;
            _nombre.Limpiar();
            _correo.Visible = true;
            _correo.Limpiar();
            MostrarClaves("Clave", "Confirmar clave");

            _btnGuardar.Text = "Crear usuario";
            _btnGuardar.Visible = true;
            _btnCancelar.Visible = true;
            _btnRestablecer.Visible = false;
            _btnEstado.Visible = false;

            _username.Enfocar();
        }

        private void ModoRestablecer()
        {
            if (Seleccionado is not Usuario u)
                return;

            _modo = Modo.RestablecerClave;
            _aviso.Ocultar();

            _lblTitulo.Text = "Restablecer clave";
            _lblInfo.Text = $"Usuario: {u.Nombre_Completo} ({u.Username}). " +
                              "Comunique la nueva clave al usuario; podrá cambiarla luego en «Cambiar mi clave».";

            _username.Visible = _nombre.Visible = _correo.Visible = false;
            MostrarClaves("Clave nueva", "Confirmar clave nueva");

            _btnGuardar.Text = "Guardar clave";
            _btnGuardar.Visible = true;
            _btnCancelar.Visible = true;
            _btnRestablecer.Visible = false;
            _btnEstado.Visible = false;

            _clave.Enfocar();
        }

        private void MostrarClaves(string titulo, string tituloConfirmar)
        {
            _clave.Titulo = titulo;
            _confirmar.Titulo = tituloConfirmar;
            _clave.Limpiar();
            _confirmar.Limpiar();
            _clave.Visible = true;
            _confirmar.Visible = true;
        }

        private void Guardar()
        {
            _aviso.Ocultar();

            try
            {
                switch (_modo)
                {
                    case Modo.Nuevo:
                        {
                            var nuevo = new Usuario { Username = _username.Texto, Nombre_Completo = _nombre.Texto, Correo_Electronico = _correo.Texto };
                            int id = _service.Registrar(nuevo, _clave.Texto, _confirmar.Texto);
                            Recargar(id);
                            _aviso.MostrarExito($"Usuario «{nuevo.Username.Trim()}» creado correctamente.");
                            break;
                        }

                    case Modo.RestablecerClave:
                        {
                            if (Seleccionado is not Usuario u)
                                return;
                            _service.RestablecerClave(u.Id_Usuario, _clave.Texto, _confirmar.Texto);
                            ModoEditar(u);
                            _aviso.MostrarExito($"Se restableció la clave de {u.Nombre_Completo}. Comuníquesela al usuario.");
                            break;
                        }

                    default:
                        {
                            if (Seleccionado is not Usuario u)
                                return;
                            var datos = new Usuario { Id_Usuario = u.Id_Usuario, Nombre_Completo = _nombre.Texto, Correo_Electronico = _correo.Texto };
                            _service.Actualizar(datos);
                            Recargar(u.Id_Usuario);
                            _aviso.MostrarExito("Datos actualizados correctamente.");
                            break;
                        }
                }
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

        private void CambiarEstado()
        {
            if (Seleccionado is not Usuario u)
                return;

            bool activar = !u.Activo;

            if (!activar && !Mensajes.Confirmar($"¿Desactivar al usuario «{u.Username}»?\n\n" +
                                                "No podrá iniciar sesión. Sus registros se conservan."))
                return;

            try
            {
                _service.CambiarEstado(u.Id_Usuario, activar);
                Recargar(u.Id_Usuario);
                _aviso.MostrarExito(activar ? "Usuario activado." : "Usuario desactivado.");
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
