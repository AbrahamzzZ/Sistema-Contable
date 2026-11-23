using Entidad;
using SistemaContable.Entidad;
using SistemaContable.Negocio;
using SistemaContable.Presentacion.Ui;
using SistemaContable.Presentacion.Ui.Controles;
using SistemaContable.Utilities;
using System.Globalization;

namespace SistemaContable.Presentacion.View.Movimientos
{
    public class VistaMovimiento : UserControl, IVistaConAccion
    {
        private readonly string _tipo;
        private readonly Action<string> _navegar;
        private readonly CategoriaService _categoriaService = new();
        private readonly MovimientoService _movimientoService = new();

        private readonly DateTimePicker _fecha = Tema.SelectorFecha();
        private readonly CampoTexto _monto = new() { Titulo = "Monto ($)", Placeholder = "0.00", MaxLength = 15 };
        private readonly ComboBox _grupo = Tema.Combo();
        private readonly ComboBox _categoria = Tema.Combo();
        private readonly ComboBox _formaPago = Tema.Combo();
        private readonly CampoTexto _comprobante = new() { Titulo = "N.º de comprobante (opcional)", Placeholder = "Factura, recibo...", MaxLength = 50 };
        private readonly CampoTexto _descripcion = new() { Titulo = "Descripción (opcional)", Placeholder = "Detalle del movimiento", MaxLength = 250 };
        private readonly Aviso _aviso = new();
        private readonly Boton _btnGuardar;
        private readonly Boton _btnLimpiar = new() { Text = "Limpiar", Estilo = EstiloBoton.Secundario };
        private readonly Boton _btnVer = new() { Text = "Ver movimientos", Estilo = EstiloBoton.Enlace };

        private bool EsIngreso => _tipo == TipoMovimiento.Ingreso;

        public IButtonControl BotonPrincipal => _btnGuardar;

        public VistaMovimiento(string tipo, Action<string> navegar)
        {
            _tipo = tipo;
            _navegar = navegar;
            _btnGuardar = new Boton
            {
                Text = EsIngreso ? "Guardar ingreso" : "Guardar egreso",
                Estilo = EsIngreso ? EstiloBoton.Exito : EstiloBoton.Peligro
            };

            SuspendLayout();
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoScroll = true;

            // Valores iniciales
            _monto.PermitirSoloMonto();
            _fecha.MaxDate = DateTime.Today;
            _fecha.Value = DateTime.Today;
            foreach (FormaPago forma in Enum.GetValues<FormaPago>())
                _formaPago.Items.Add(new Opcion<FormaPago>(forma, forma.TextoPantalla()));
            _formaPago.SelectedIndex = 0;

            // Diseño
            var tarjeta = new PanelTarjeta { Width = 780, Location = Point.Empty };
            var columna = Tema.Columna();
            columna.Controls.Add(Tema.Seccion(EsIngreso ? "Datos del ingreso" : "Datos del egreso"));
            columna.Controls.Add(Tema.Nota(EsIngreso
                ? "Registre diezmos, ofrendas y otros ingresos de la iglesia."
                : "Registre los gastos de servicios básicos, misceláneos y escuela dominical."));
            columna.Controls.Add(_aviso);

            var rejilla = Tema.Rejilla(2);
            Tema.Agregar(rejilla, new CampoControl("Fecha", _fecha), 0, 0);
            Tema.Agregar(rejilla, _monto, 1, 0);
            Tema.Agregar(rejilla, new CampoControl("Grupo", _grupo), 0, 1);
            Tema.Agregar(rejilla, new CampoControl("Categoría", _categoria), 1, 1);
            Tema.Agregar(rejilla, new CampoControl("Forma de pago", _formaPago), 0, 2);
            Tema.Agregar(rejilla, _comprobante, 1, 2);
            columna.Controls.Add(rejilla);
            columna.Controls.Add(_descripcion);

            var botones = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new Padding(0, 6, 0, 0) };
            botones.Controls.Add(_btnGuardar);
            botones.Controls.Add(_btnLimpiar);
            botones.Controls.Add(_btnVer);
            columna.Controls.Add(botones);

            tarjeta.Controls.Add(columna);
            tarjeta.AjustarAlContenido(columna);
            Controls.Add(tarjeta);

            // Eventos
            _grupo.SelectedIndexChanged += (_, _) => CargarCategorias();
            _categoria.SelectedIndexChanged += (_, _) => ActualizarDescripcion();
            _btnGuardar.Click += (_, _) => Guardar();
            _btnLimpiar.Click += (_, _) => Limpiar(todo: true);
            _btnVer.Click += (_, _) => _navegar(Secciones.Movimientos);

            ResumeLayout(false);
            PerformLayout();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            CargarGrupos();
        }

        private void CargarGrupos()
        {
            try
            {
                _grupo.Items.Clear();
                foreach (Categoria grupo in _categoriaService.ListarGrupos(_tipo))
                    _grupo.Items.Add(grupo);

                if (_grupo.Items.Count > 0)
                {
                    _grupo.SelectedIndex = 0;
                }
                else
                {
                    _aviso.MostrarError("No hay grupos activos para este tipo. Créelos en Administración → Categorías.");
                    _btnGuardar.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                Mensajes.MostrarExcepcion(ex);
            }
        }

        private void CargarCategorias()
        {
            _categoria.Items.Clear();

            if (_grupo.SelectedItem is Categoria grupo)
            {
                try
                {
                    foreach (Categoria categoria in _categoriaService.ListarPorGrupo(grupo.Id_Categoria))
                        _categoria.Items.Add(categoria);
                }
                catch (Exception ex)
                {
                    Mensajes.MostrarExcepcion(ex);
                }
            }

            if (_categoria.Items.Count > 0)
                _categoria.SelectedIndex = 0;

            ActualizarDescripcion();
        }

        private void ActualizarDescripcion()
        {
            bool esOtros = _categoria.SelectedItem is Categoria c && c.Nombre is "Otros" or "Otro";
            _descripcion.Titulo = esOtros ? "Descripción (obligatoria para «Otros»)" : "Descripción (opcional)";
        }

        private void Guardar()
        {
            _aviso.Ocultar();

            if (!decimal.TryParse(_monto.Texto, NumberStyles.Number, CultureInfo.CurrentCulture, out decimal monto))
            {
                _aviso.MostrarError("Ingrese un monto válido, por ejemplo 25.50.");
                _monto.Enfocar();
                return;
            }

            if (_categoria.SelectedItem is not Categoria categoria)
            {
                _aviso.MostrarError("Seleccione la categoría.");
                return;
            }

            var movimiento = new Movimiento
            {
                Fecha_Movimiento = _fecha.Value.Date,
                Id_Categoria = categoria.Id_Categoria,
                Monto = monto,
                Descripcion = _descripcion.Texto,
                Numero_Comprobante = _comprobante.Texto,
                FormaPago = (_formaPago.SelectedItem as Opcion<FormaPago>)?.Valor ?? FormaPago.Efectivo
            };

            try
            {
                int id = _movimientoService.Registrar(movimiento);
                _aviso.MostrarExito($"{(EsIngreso ? "Ingreso" : "Egreso")} N.º {id} registrado: " +
                                    $"{Tema.Moneda(monto)} en {categoria.Nombre}.");
                Limpiar(todo: false);
                _monto.Enfocar();
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

        /// <summary>
        /// todo = false: tras guardar, conserva fecha, grupo, categoría y forma de pago
        /// para registrar el siguiente rápido. todo = true: deja el formulario como nuevo.
        /// </summary>
        private void Limpiar(bool todo)
        {
            _monto.Limpiar();
            _comprobante.Limpiar();
            _descripcion.Limpiar();

            if (!todo)
                return;

            _aviso.Ocultar();
            _fecha.Value = DateTime.Today;
            _formaPago.SelectedIndex = 0;
            if (_grupo.Items.Count > 0)
                _grupo.SelectedIndex = 0;
        }
    }
}
