using SistemaContable.Entidad;
using SistemaContable.Negocio;
using SistemaContable.Presentacion.Ui;
using SistemaContable.Presentacion.Ui.Controles;
using SistemaContable.Utilities;

namespace SistemaContable.Presentacion.View.Movimientos
{
    public class VistaMovimientos : UserControl, IVistaConAccion
    {
        private static readonly Font FuenteAnulado = new(Tema.Fuente, FontStyle.Strikeout);

        private readonly MovimientoService _movimientoService = new();
        private readonly CategoriaService _categoriaService = new();

        // Filtros
        private readonly DateTimePicker _desde = Tema.SelectorFecha();
        private readonly DateTimePicker _hasta = Tema.SelectorFecha();
        private readonly ComboBox _tipo = Tema.Combo();
        private readonly ComboBox _categoria = Tema.Combo();
        private readonly CheckBox _chkAnulados = new()
        {
            Text = "Incluir movimientos anulados",
            AutoSize = true,
            ForeColor = Tema.Texto,
            Anchor = AnchorStyles.Left,
            Margin = new Padding(0, 6, 0, 0)
        };
        private readonly Boton _btnBuscar = new() { Text = "Buscar", Estilo = EstiloBoton.Primario };
        private readonly Boton _btnMesActual = new() { Text = "Mes actual", Estilo = EstiloBoton.Secundario };
        private readonly Aviso _avisoFiltro = new();

        // Totales
        private readonly TarjetaResumen _totIngresos = new("Ingresos", Tema.Ingreso);
        private readonly TarjetaResumen _totEgresos = new("Egresos", Tema.Egreso);
        private readonly TarjetaResumen _totSaldo = new("Saldo", Tema.Primario);

        // Listado
        private readonly DataGridView _grilla = new();
        private readonly Label _lblConteo = Tema.Nota(string.Empty);

        // Detalle y anulación
        private readonly TableLayoutPanel _detalle = new()
        {
            ColumnCount = 4,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Anchor = AnchorStyles.Left | AnchorStyles.Right,
            Margin = new Padding(0, 0, 0, 8)
        };
        private readonly TableLayoutPanel _zonaAnular = Tema.Columna();
        private readonly CampoTexto _motivo = new() { Titulo = "Motivo de la anulación", Placeholder = "Ej. Monto registrado por error", MaxLength = 250 };
        private readonly Boton _btnAnular = new() { Text = "Anular movimiento", Estilo = EstiloBoton.Peligro };
        private readonly Aviso _avisoDetalle = new();

        private List<MovimientoDetalle> _datos = new();

        public IButtonControl BotonPrincipal => _btnBuscar;

        private MovimientoDetalle? Seleccionado => _grilla.CurrentRow?.DataBoundItem as MovimientoDetalle;

        public VistaMovimientos()
        {
            SuspendLayout();
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoScroll = true;

            _desde.Value = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            _hasta.Value = DateTime.Today;
            _tipo.Items.Add(new Opcion<string?>(null, "Todos"));
            _tipo.Items.Add(new Opcion<string?>(TipoMovimiento.Ingreso, "Ingresos"));
            _tipo.Items.Add(new Opcion<string?>(TipoMovimiento.Egreso, "Egresos"));
            _tipo.SelectedIndex = 0;
            _categoria.DropDownWidth = 340;

            var columna = Tema.Columna();
            columna.Controls.Add(CrearFiltros());
            columna.Controls.Add(Tema.FilaTarjetas(_totIngresos, _totEgresos, _totSaldo));
            columna.Controls.Add(_lblConteo);
            columna.Controls.Add(CrearListado());
            columna.Controls.Add(CrearDetalle());
            Controls.Add(columna);

            // Eventos
            _tipo.SelectedIndexChanged += (_, _) => CargarCategoriasFiltro();
            _btnBuscar.Click += (_, _) => Buscar();
            _btnMesActual.Click += (_, _) =>
            {
                _desde.Value = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                _hasta.Value = DateTime.Today;
                Buscar();
            };
            _grilla.SelectionChanged += (_, _) => MostrarDetalle(Seleccionado);
            _grilla.CellFormatting += FormatearCelda;
            _btnAnular.Click += (_, _) => Anular();

            ResumeLayout(false);
            PerformLayout();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            CargarCategoriasFiltro();
            Buscar();
        }

        private Control CrearFiltros()
        {
            var tarjeta = new PanelTarjeta
            {
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Padding = new Padding(24, 20, 24, 16),
                Margin = new Padding(0, 0, 0, 20)
            };

            var rejilla = Tema.Rejilla(4);
            Tema.Agregar(rejilla, new CampoControl("Desde", _desde), 0, 0);
            Tema.Agregar(rejilla, new CampoControl("Hasta", _hasta), 1, 0);
            Tema.Agregar(rejilla, new CampoControl("Tipo", _tipo), 2, 0);
            Tema.Agregar(rejilla, new CampoControl("Categoría", _categoria), 3, 0);

            var botones = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                AutoSize = true,
                WrapContents = false,
                Anchor = AnchorStyles.Right,
                Margin = new Padding(0)
            };
            _btnBuscar.Margin = new Padding(10, 0, 0, 0);
            _btnMesActual.Margin = new Padding(10, 0, 0, 0);
            botones.Controls.Add(_btnBuscar);   
            botones.Controls.Add(_btnMesActual);

            rejilla.Controls.Add(_chkAnulados, 0, 1);
            rejilla.SetColumnSpan(_chkAnulados, 2);
            rejilla.Controls.Add(botones, 2, 1);
            rejilla.SetColumnSpan(botones, 2);

            var contenido = Tema.Columna();
            contenido.Controls.Add(_avisoFiltro);
            contenido.Controls.Add(rejilla);

            tarjeta.Controls.Add(contenido);
            tarjeta.AjustarAlContenido(contenido);
            return tarjeta;
        }

        private Control CrearListado()
        {
            var tarjeta = new PanelTarjeta
            {
                Height = 400,
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Padding = new Padding(16, 12, 16, 12),
                Margin = new Padding(0, 0, 0, 20)
            };

            Tema.EstilizarGrilla(_grilla);
            _grilla.Dock = DockStyle.Fill;

            AgregarColumna(nameof(MovimientoDetalle.Id_Movimiento), "N.º", 60);
            AgregarColumna(nameof(MovimientoDetalle.Fecha_Movimiento), "Fecha", 100, "dd/MM/yyyy");
            AgregarColumna(nameof(MovimientoDetalle.TipoTexto), "Tipo", 80);
            AgregarColumna(nameof(MovimientoDetalle.Categoria), "Categoría", 160);
            AgregarColumna(nameof(MovimientoDetalle.Descripcion), "Descripción", 0);   // ocupa el resto
            AgregarColumna(nameof(MovimientoDetalle.FormaPagoTexto), "Forma de pago", 120);
            AgregarColumna(nameof(MovimientoDetalle.Monto), "Monto", 120, "$ #,##0.00", derecha: true);
            AgregarColumna(nameof(MovimientoDetalle.Estado), "Estado", 85);

            tarjeta.Controls.Add(_grilla);
            return tarjeta;
        }

        private void AgregarColumna(string propiedad, string titulo, int ancho, string? formato = null, bool derecha = false)
        {
            var columna = new DataGridViewTextBoxColumn
            {
                Name = propiedad,
                DataPropertyName = propiedad,
                HeaderText = titulo,
                SortMode = DataGridViewColumnSortMode.NotSortable
            };

            if (ancho > 0)
            {
                columna.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                columna.Width = ancho;
            }
            else
            {
                columna.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                columna.MinimumWidth = 140;
            }

            if (formato is not null)
                columna.DefaultCellStyle.Format = formato;

            if (derecha)
            {
                columna.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                columna.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            }

            _grilla.Columns.Add(columna);
        }

        private Control CrearDetalle()
        {
            var tarjeta = new PanelTarjeta
            {
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Padding = new Padding(24, 20, 24, 20),
                Margin = new Padding(0)
            };

            _detalle.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            _detalle.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            _detalle.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            _detalle.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            _zonaAnular.Dock = DockStyle.None;
            _zonaAnular.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            _btnAnular.Margin = new Padding(0);
            _zonaAnular.Controls.Add(_motivo);
            _zonaAnular.Controls.Add(_btnAnular);

            var contenido = Tema.Columna();
            contenido.Controls.Add(Tema.Seccion("Detalle del movimiento"));
            contenido.Controls.Add(_avisoDetalle);
            contenido.Controls.Add(_detalle);
            contenido.Controls.Add(_zonaAnular);

            tarjeta.Controls.Add(contenido);
            tarjeta.AjustarAlContenido(contenido);
            return tarjeta;
        }


        private void CargarCategoriasFiltro()
        {
            string? tipo = (_tipo.SelectedItem as Opcion<string?>)?.Valor;

            _categoria.Items.Clear();
            _categoria.Items.Add(new Opcion<int?>(null, "Todas las categorías"));

            try
            {
                foreach (Categoria c in _categoriaService.Listar(tipo, incluirInactivos: true).Where(c => !c.Es_Grupo))
                    _categoria.Items.Add(new Opcion<int?>(c.Id_Categoria, $"{c.Grupo} › {c.Nombre}"));
            }
            catch (Exception ex)
            {
                Mensajes.MostrarExcepcion(ex);
            }

            _categoria.SelectedIndex = 0;
        }

        private void Buscar()
        {
            _avisoFiltro.Ocultar();

            var filtro = new FiltroMovimiento
            {
                Desde = _desde.Value.Date,
                Hasta = _hasta.Value.Date,
                Tipo = (_tipo.SelectedItem as Opcion<string?>)?.Valor,
                IdCategoria = (_categoria.SelectedItem as Opcion<int?>)?.Valor,
                IncluirAnulados = _chkAnulados.Checked
            };

            try
            {
                int? idSeleccionado = Seleccionado?.Id_Movimiento;

                _datos = _movimientoService.Listar(filtro);
                _grilla.DataSource = null;
                _grilla.DataSource = _datos;

                var (ingresos, egresos, saldo) = MovimientoService.CalcularTotales(_datos);
                int cantIngresos = _datos.Count(m => !m.Anulado && m.Tipo == TipoMovimiento.Ingreso);
                int cantEgresos = _datos.Count(m => !m.Anulado && m.Tipo == TipoMovimiento.Egreso);

                _totIngresos.MostrarValor(ingresos, Tema.Ingreso);
                _totIngresos.MostrarDetalle($"{cantIngresos} movimiento(s)");
                _totEgresos.MostrarValor(egresos, Tema.Egreso);
                _totEgresos.MostrarDetalle($"{cantEgresos} movimiento(s)");
                _totSaldo.MostrarValor(saldo, saldo < 0 ? Tema.Egreso : Tema.Texto);
                _totSaldo.MostrarDetalle("Sin contar anulados");

                _lblConteo.Text = _datos.Count == 0
                    ? "No hay movimientos con los filtros seleccionados."
                    : $"{_datos.Count} registro(s) del {filtro.Desde:dd/MM/yyyy} al {filtro.Hasta:dd/MM/yyyy}";

                Reseleccionar(idSeleccionado);
                if (_datos.Count == 0)
                    MostrarDetalle(null);
            }
            catch (NegocioException ex)
            {
                _avisoFiltro.MostrarError(ex.Message);
            }
            catch (Exception ex)
            {
                Mensajes.MostrarExcepcion(ex);
            }
        }

        private void Reseleccionar(int? idMovimiento)
        {
            if (idMovimiento is null)
                return;

            foreach (DataGridViewRow fila in _grilla.Rows)
            {
                if (fila.DataBoundItem is MovimientoDetalle m && m.Id_Movimiento == idMovimiento)
                {
                    _grilla.CurrentCell = fila.Cells[0];
                    return;
                }
            }
        }

        private void FormatearCelda(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.CellStyle is null)
                return;

            if (_grilla.Rows[e.RowIndex].DataBoundItem is not MovimientoDetalle m)
                return;

            // Anulados: gris y tachados
            if (m.Anulado)
            {
                e.CellStyle.ForeColor = Tema.TextoDeshabilitado;
                e.CellStyle.SelectionForeColor = Tema.TextoDeshabilitado;
                e.CellStyle.Font = FuenteAnulado;
                return;
            }

            // Monto y tipo: verde para ingresos, rojo para egresos
            string columna = _grilla.Columns[e.ColumnIndex].Name;
            if (columna is nameof(MovimientoDetalle.Monto) or nameof(MovimientoDetalle.TipoTexto))
            {
                Color color = m.Tipo == TipoMovimiento.Ingreso ? Tema.Ingreso : Tema.Egreso;
                e.CellStyle.ForeColor = color;
                e.CellStyle.SelectionForeColor = color;
            }
        }

        // ---------------------------------------------------------------
        // DETALLE Y ANULACIÓN
        // ---------------------------------------------------------------

        private void MostrarDetalle(MovimientoDetalle? m)
        {
            _detalle.SuspendLayout();

            foreach (Control control in _detalle.Controls.Cast<Control>().ToList())
                control.Dispose();
            _detalle.Controls.Clear();
            _detalle.RowStyles.Clear();
            _detalle.RowCount = 0;

            if (m is null)
            {
                var nota = Tema.Nota("Seleccione un movimiento de la lista para ver su detalle.");
                _detalle.Controls.Add(nota, 0, 0);
                _detalle.SetColumnSpan(nota, 4);
                _zonaAnular.Visible = false;
            }
            else
            {
                var datos = new List<(string Campo, string Valor)>
                {
                    ("N.º",            m.Id_Movimiento.ToString()),
                    ("Fecha",          m.Fecha_Movimiento.ToString("dd/MM/yyyy")),
                    ("Tipo",           m.TipoTexto),
                    ("Monto",          Tema.Moneda(m.Monto)),
                    ("Grupo",          m.Grupo),
                    ("Categoría",      m.Categoria),
                    ("Forma de pago",  m.FormaPagoTexto),
                    ("Comprobante",    m.Numero_Comprobante ?? "—"),
                    ("Registrado por", $"{m.RegistradoPor} · {m.Fecha_Registro:dd/MM/yyyy HH:mm}"),
                    ("Estado",         m.Estado),
                    ("Descripción",    m.Descripcion ?? "—")
                };

                if (m.Anulado)
                {
                    datos.Add(("Anulado por", $"{m.Anulado_Por} · {m.Fecha_Anulacion:dd/MM/yyyy HH:mm}"));
                    datos.Add(("Motivo", m.Motivo_Anulacion ?? "—"));
                }

                for (int i = 0; i < datos.Count; i++)
                {
                    int columna = (i % 2) * 2;
                    int fila = i / 2;
                    _detalle.Controls.Add(EtiquetaDato(datos[i].Campo), columna, fila);
                    _detalle.Controls.Add(ValorDato(datos[i].Valor), columna + 1, fila);
                }

                _zonaAnular.Visible = !m.Anulado;
            }

            _detalle.ResumeLayout(true);
        }

        private static Label EtiquetaDato(string texto) => new()
        {
            Text = texto,
            AutoSize = true,
            Font = Tema.FuenteEtiqueta,
            ForeColor = Tema.TextoSecundario,
            Margin = new Padding(0, 4, 16, 8)
        };

        private Label ValorDato(string texto) => new()
        {
            Text = texto,
            AutoSize = true,
            ForeColor = Tema.Texto,
            MaximumSize = new Size(LogicalToDeviceUnits(320), 0),
            Margin = new Padding(0, 4, 24, 8)
        };

        private void Anular()
        {
            _avisoDetalle.Ocultar();

            if (Seleccionado is not MovimientoDetalle m)
                return;

            if (string.IsNullOrWhiteSpace(_motivo.Texto))
            {
                _avisoDetalle.MostrarError("Indique el motivo de la anulación.");
                _motivo.Enfocar();
                return;
            }

            if (!Mensajes.Confirmar($"¿Anular el movimiento N.º {m.Id_Movimiento} por {Tema.Moneda(m.Monto)}?\n\n" +
                                    "Esta acción no se puede deshacer."))
                return;

            try
            {
                _movimientoService.Anular(m.Id_Movimiento, _motivo.Texto);
                _motivo.Limpiar();
                Buscar();
                _avisoDetalle.MostrarExito($"El movimiento N.º {m.Id_Movimiento} fue anulado. " +
                                           "Active «Incluir movimientos anulados» para verlo en la lista.");
            }
            catch (NegocioException ex)
            {
                _avisoDetalle.MostrarError(ex.Message);
            }
            catch (Exception ex)
            {
                Mensajes.MostrarExcepcion(ex);
            }
        }
    }
}
