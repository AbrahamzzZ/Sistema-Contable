using SistemaContable.Entidad;
using SistemaContable.Negocio;
using SistemaContable.Presentacion.Ui;
using SistemaContable.Presentacion.Ui.Controles;
using SistemaContable.Utilities;

namespace SistemaContable.Presentacion.View.Categorias
{
    public class VistaCategorias : UserControl, IVistaConAccion
    {
        private enum Modo { Editar, NuevoGrupo, NuevaCategoria }

        private static readonly Font FuenteGrupo = new(Tema.Fuente, FontStyle.Bold);

        private readonly CategoriaService _service = new();
        private readonly ComboBox _filtroTipo = Tema.Combo();
        private readonly CheckBox _chkInactivos = new() { Text = "Mostrar inactivos", AutoSize = true, Checked = true, Margin = new Padding(16, 10, 0, 0) };
        private readonly Boton _btnNuevoGrupo = new() { Text = "+ Nuevo grupo", Estilo = EstiloBoton.Primario };
        private readonly Boton _btnNuevaCategoria = new() { Text = "+ Nueva categoría", Estilo = EstiloBoton.Secundario };
        private readonly DataGridView _grilla = new();
        private readonly Label _lblTitulo = Tema.Seccion(string.Empty);
        private readonly Label _lblInfo = Tema.Nota(string.Empty);
        private readonly Aviso _aviso = new();
        private readonly ComboBox _tipo = Tema.Combo();
        private readonly ComboBox _grupo = Tema.Combo();
        private readonly CampoTexto _nombre = new() { Titulo = "Nombre", MaxLength = 100 };
        private readonly NumericUpDown _orden = new() { Minimum = 0, Maximum = 999, Font = Tema.FuenteCampo };
        private readonly Label _lblNotaOrden = Tema.Nota("Orden 0 = se ubica al final de la lista.");
        private readonly CampoControl _campoTipo;
        private readonly CampoControl _campoGrupo;
        private readonly CampoControl _campoOrden;
        private readonly Boton _btnGuardar = new() { Text = "Guardar cambios", Estilo = EstiloBoton.Primario };
        private readonly Boton _btnCancelar = new() { Text = "Cancelar", Estilo = EstiloBoton.Secundario };
        private readonly Boton _btnEstado = new() { Text = "Desactivar", Estilo = EstiloBoton.Secundario };
        private readonly Boton _btnEliminar = new() { Text = "Eliminar", Estilo = EstiloBoton.Peligro };

        private List<Categoria> _datos = new();
        private Modo _modo = Modo.Editar;
        private bool _recargando;

        public IButtonControl BotonPrincipal => _btnGuardar;

        private Categoria? Seleccionada => _grilla.CurrentRow?.DataBoundItem as Categoria;

        public VistaCategorias()
        {
            SuspendLayout();
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoScroll = true;

            _filtroTipo.Items.Add(new Opcion<string?>(null, "Todos los tipos"));
            _filtroTipo.Items.Add(new Opcion<string?>(TipoMovimiento.Ingreso, "Ingresos"));
            _filtroTipo.Items.Add(new Opcion<string?>(TipoMovimiento.Egreso, "Egresos"));
            _filtroTipo.SelectedIndex = 0;
            _filtroTipo.Width = 190;
            _filtroTipo.Anchor = AnchorStyles.Left;
            _filtroTipo.Margin = new Padding(24, 6, 0, 0);

            _tipo.Items.Add(new Opcion<string>(TipoMovimiento.Ingreso, "Ingreso"));
            _tipo.Items.Add(new Opcion<string>(TipoMovimiento.Egreso, "Egreso"));
            _tipo.SelectedIndex = 1;

            _campoTipo = new CampoControl("Tipo", _tipo);
            _campoGrupo = new CampoControl("Grupo al que pertenece", _grupo);
            _campoOrden = new CampoControl("Orden en las listas", _orden);
            _lblInfo.MaximumSize = new Size(340, 0);

            var barra = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new Padding(0, 0, 0, 16) };
            barra.Controls.Add(_btnNuevoGrupo);
            barra.Controls.Add(_btnNuevaCategoria);
            barra.Controls.Add(_filtroTipo);
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

            // Eventos
            _filtroTipo.SelectedIndexChanged += (_, _) => Recargar(Seleccionada?.Id_Categoria);
            _chkInactivos.CheckedChanged += (_, _) => Recargar(Seleccionada?.Id_Categoria);
            _btnNuevoGrupo.Click += (_, _) => ModoNuevoGrupo();
            _btnNuevaCategoria.Click += (_, _) => ModoNuevaCategoria();
            _grilla.SelectionChanged += (_, _) =>
            {
                if (!_recargando) _aviso.Ocultar();
                ModoEditar(Seleccionada);
            };
            _grilla.CellFormatting += FormatearCelda;
            _btnGuardar.Click += (_, _) => Guardar();
            _btnCancelar.Click += (_, _) => { _aviso.Ocultar(); ModoEditar(Seleccionada); };
            _btnEstado.Click += (_, _) => CambiarEstado();
            _btnEliminar.Click += (_, _) => Eliminar();

            ResumeLayout(false);
            PerformLayout();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Recargar(null);
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
            Tema.AgregarColumna(_grilla, nameof(Categoria.Nombre), "Nombre", 0);
            Tema.AgregarColumna(_grilla, nameof(Categoria.TipoTexto), "Tipo", 80);
            Tema.AgregarColumna(_grilla, nameof(Categoria.Orden), "Orden", 65, derecha: true);
            Tema.AgregarColumna(_grilla, nameof(Categoria.Cantidad_Movimientos), "Movs.", 65, derecha: true);
            Tema.AgregarColumna(_grilla, nameof(Categoria.Estado), "Estado", 85);

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
            botonesAcciones.Controls.Add(_btnEstado);
            botonesAcciones.Controls.Add(_btnEliminar);

            var contenido = Tema.Columna();
            contenido.Controls.Add(_lblTitulo);
            contenido.Controls.Add(_lblInfo);
            contenido.Controls.Add(_aviso);
            contenido.Controls.Add(_campoTipo);
            contenido.Controls.Add(_campoGrupo);
            contenido.Controls.Add(_nombre);
            contenido.Controls.Add(_campoOrden);
            contenido.Controls.Add(_lblNotaOrden);
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

            if (_grilla.Rows[e.RowIndex].DataBoundItem is not Categoria c)
                return;

            if (c.Es_Grupo)
            {
                // Grupos: negrita y fondo gris
                e.CellStyle.Font = FuenteGrupo;
                e.CellStyle.BackColor = Tema.Fondo;
            }
            else if (_grilla.Columns[e.ColumnIndex].Name == nameof(Categoria.Nombre))
            {
                // Categorías: con sangría debajo de su grupo
                e.CellStyle.Padding = new Padding(LogicalToDeviceUnits(30), 0, 8, 0);
            }

            if (!c.Activo)
            {
                e.CellStyle.ForeColor = Tema.TextoDeshabilitado;
                e.CellStyle.SelectionForeColor = Tema.TextoDeshabilitado;
            }
        }

        private void Recargar(int? idSeleccionar)
        {
            string? tipo = (_filtroTipo.SelectedItem as Opcion<string?>)?.Valor;

            try
            {
                _recargando = true;
                _datos = _service.Listar(tipo, _chkInactivos.Checked);
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

        private void Seleccionar(int idCategoria)
        {
            foreach (DataGridViewRow fila in _grilla.Rows)
            {
                if (fila.DataBoundItem is Categoria c && c.Id_Categoria == idCategoria)
                {
                    _grilla.CurrentCell = fila.Cells[0];
                    return;
                }
            }
        }

        private void ModoEditar(Categoria? c)
        {
            _modo = Modo.Editar;
            _campoTipo.Visible = false;
            _campoGrupo.Visible = false;
            _lblNotaOrden.Visible = false;
            _btnCancelar.Visible = false;

            if (c is null)
            {
                _lblTitulo.Text = "Sin selección";
                _lblInfo.Text = "Elija un grupo o categoría de la lista para editarlo, o cree uno nuevo.";
                _nombre.Visible = _campoOrden.Visible = false;
                _btnGuardar.Visible = _btnEstado.Visible = _btnEliminar.Visible = false;
                return;
            }

            _lblTitulo.Text = c.Es_Grupo ? "Editar grupo" : "Editar categoría";
            _lblInfo.Text = c.Es_Grupo
                ? $"Tipo: {c.TipoTexto}  ·  {c.Cantidad_Movimientos} movimiento(s)  ·  {c.Estado}"
                : $"Grupo: {c.Grupo}  ·  Tipo: {c.TipoTexto}  ·  {c.Cantidad_Movimientos} movimiento(s)  ·  {c.Estado}";

            _nombre.Visible = true;
            _nombre.Texto = c.Nombre;
            _campoOrden.Visible = true;
            _orden.Value = Math.Clamp(c.Orden, 0, 999);

            _btnGuardar.Visible = true;
            _btnGuardar.Text = "Guardar cambios";
            _btnEstado.Visible = true;
            _btnEstado.Text = c.Activo ? "Desactivar" : "Activar";
            _btnEliminar.Visible = true;
            _btnEliminar.Enabled = c.SePuedeEliminar;
        }

        private void ModoNuevoGrupo()
        {
            _modo = Modo.NuevoGrupo;
            _aviso.Ocultar();

            _lblTitulo.Text = "Nuevo grupo";
            _lblInfo.Text = "Ejemplo: MISIONES. Los grupos se guardan en mayúsculas.";

            string? tipoFiltro = (_filtroTipo.SelectedItem as Opcion<string?>)?.Valor;
            _tipo.SelectedIndex = tipoFiltro == TipoMovimiento.Ingreso ? 0 : 1;

            MostrarFormularioNuevo(mostrarTipo: true, mostrarGrupo: false, textoBoton: "Crear grupo");
        }

        private void ModoNuevaCategoria()
        {
            _modo = Modo.NuevaCategoria;
            _aviso.Ocultar();

            _lblTitulo.Text = "Nueva categoría";
            _lblInfo.Text = "El tipo (ingreso o egreso) se toma del grupo seleccionado.";

            _grupo.Items.Clear();
            try
            {
                foreach (string tipo in new[] { TipoMovimiento.Ingreso, TipoMovimiento.Egreso })
                    foreach (Categoria g in _service.ListarGrupos(tipo))
                        _grupo.Items.Add(new Opcion<int>(g.Id_Categoria, $"{g.Nombre} ({TipoMovimiento.Texto(tipo)})"));
            }
            catch (Exception ex)
            {
                Mensajes.MostrarExcepcion(ex);
                return;
            }

            if (_grupo.Items.Count == 0)
            {
                _aviso.MostrarError("No hay grupos activos. Cree primero un grupo.");
                return;
            }

            int? idGrupo = Seleccionada is Categoria sel ? (sel.Es_Grupo ? sel.Id_Categoria : sel.Id_Padre) : null;
            _grupo.SelectedIndex = 0;
            for (int i = 0; i < _grupo.Items.Count; i++)
            {
                if (_grupo.Items[i] is Opcion<int> op && op.Valor == idGrupo)
                {
                    _grupo.SelectedIndex = i;
                    break;
                }
            }

            MostrarFormularioNuevo(mostrarTipo: false, mostrarGrupo: true, textoBoton: "Crear categoría");
        }

        private void MostrarFormularioNuevo(bool mostrarTipo, bool mostrarGrupo, string textoBoton)
        {
            _campoTipo.Visible = mostrarTipo;
            _campoGrupo.Visible = mostrarGrupo;
            _nombre.Visible = true;
            _nombre.Limpiar();
            _campoOrden.Visible = true;
            _orden.Value = 0;
            _lblNotaOrden.Visible = true;

            _btnGuardar.Visible = true;
            _btnGuardar.Text = textoBoton;
            _btnCancelar.Visible = true;
            _btnEstado.Visible = false;
            _btnEliminar.Visible = false;

            _nombre.Enfocar();
        }

        private void Guardar()
        {
            _aviso.Ocultar();
            int orden = (int)_orden.Value;

            try
            {
                int id;
                string mensaje;

                switch (_modo)
                {
                    case Modo.NuevoGrupo:
                        string tipo = (_tipo.SelectedItem as Opcion<string>)?.Valor ?? TipoMovimiento.Egreso;
                        id = _service.CrearGrupo(_nombre.Texto, tipo, orden);
                        mensaje = "Grupo creado correctamente.";
                        break;

                    case Modo.NuevaCategoria:
                        if (_grupo.SelectedItem is not Opcion<int> grupo)
                        {
                            _aviso.MostrarError("Seleccione el grupo.");
                            return;
                        }
                        id = _service.CrearCategoria(grupo.Valor, _nombre.Texto, orden);
                        mensaje = "Categoría creada correctamente.";
                        break;

                    default:
                        if (Seleccionada is not Categoria actual)
                            return;
                        _service.Actualizar(actual.Id_Categoria, _nombre.Texto, orden);
                        id = actual.Id_Categoria;
                        mensaje = "Cambios guardados correctamente.";
                        break;
                }

                Recargar(id);
                _aviso.MostrarExito(mensaje);
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
            if (Seleccionada is not Categoria c)
                return;

            bool activar = !c.Activo;

            if (!activar)
            {
                string pregunta = c.Es_Grupo
                    ? $"¿Desactivar el grupo «{c.Nombre}»?\n\nTambién se desactivarán todas sus categorías. " +
                      "Dejarán de aparecer al registrar movimientos, pero su historial se conserva."
                    : $"¿Desactivar la categoría «{c.Nombre}»?\n\n" +
                      "Dejará de aparecer al registrar movimientos, pero su historial se conserva.";

                if (!Mensajes.Confirmar(pregunta))
                    return;
            }

            try
            {
                _service.CambiarEstado(c.Id_Categoria, activar);
                Recargar(c.Id_Categoria);
                _aviso.MostrarExito(activar ? "Activado correctamente." : "Desactivado correctamente.");
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

        private void Eliminar()
        {
            if (Seleccionada is not Categoria c)
                return;

            if (!Mensajes.Confirmar($"¿Eliminar definitivamente «{c.Nombre}»?\n\nEsta acción no se puede deshacer."))
                return;

            try
            {
                _service.Eliminar(c.Id_Categoria);
                Recargar(null);
                _aviso.MostrarExito($"«{c.Nombre}» fue eliminado.");
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
