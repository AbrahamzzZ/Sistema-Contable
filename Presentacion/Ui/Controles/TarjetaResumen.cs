namespace SistemaContable.Presentacion.Ui.Controles
{
    /// <summary>Tarjeta de indicador: título, monto grande, detalle y franja de color.</summary>
    public class TarjetaResumen : PanelTarjeta
    {
        private readonly Color _acento;
        private readonly Label _valor;
        private readonly Label _detalle;

        public TarjetaResumen(string titulo, Color acento)
        {
            _acento = acento;
            Height = 132;
            Anchor = AnchorStyles.Left | AnchorStyles.Right;
            Margin = new Padding(0, 0, 16, 0);

            var lblTitulo = new Label
            {
                Text = titulo,
                AutoSize = true,
                Font = Tema.FuenteEtiqueta,
                ForeColor = Tema.TextoSecundario,
                BackColor = Color.White,
                Location = new Point(26, 22)
            };

            _valor = new Label
            {
                Text = Tema.Moneda(0),
                AutoSize = true,
                Font = Tema.FuenteValor,
                ForeColor = Tema.Texto,
                BackColor = Color.White,
                Location = new Point(24, 46)
            };

            _detalle = new Label
            {
                AutoSize = true,
                Font = Tema.FuentePequena,
                ForeColor = Tema.TextoSecundario,
                BackColor = Color.White,
                Location = new Point(26, 96)
            };

            Controls.Add(lblTitulo);
            Controls.Add(_valor);
            Controls.Add(_detalle);
        }

        public void MostrarValor(decimal valor, Color? color = null)
        {
            _valor.Text = Tema.Moneda(valor);
            _valor.ForeColor = color ?? Tema.Texto;
        }

        public void MostrarDetalle(string texto) => _detalle.Text = texto;

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            int radio = LogicalToDeviceUnits(12);
            int ancho = LogicalToDeviceUnits(5);
            using var franja = new SolidBrush(_acento);
            e.Graphics.FillRectangle(franja, 0, radio, ancho, Height - radio * 2);
        }
    }
}
