using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace SistemaContable.Presentacion.Ui.Controles
{
    /// <summary>
    /// Campo de texto moderno: título arriba, caja con esquinas redondeadas,
    /// borde azul al escribir y opción "Mostrar/Ocultar" para claves.
    /// </summary>
    public class CampoTexto : UserControl
    {
        private readonly Label _titulo = new()
        {
            AutoSize = true,
            Font = Tema.FuenteEtiqueta,
            ForeColor = Tema.Texto,
            BackColor = Color.Transparent
        };

        private readonly TextBox _caja = new()
        {
            BorderStyle = BorderStyle.None,
            Font = Tema.FuenteCampo,
            BackColor = Color.White,
            ForeColor = Tema.Texto
        };

        private readonly Label _mostrar = new()
        {
            AutoSize = true,
            Text = "Mostrar",
            Font = Tema.FuenteEtiqueta,
            ForeColor = Tema.Primario,
            BackColor = Color.White,
            Cursor = Cursors.Hand,
            Visible = false
        };

        private Rectangle _marco;
        private bool _enfocado;
        private bool _esClave;

        public event EventHandler? TextoCambiado;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Titulo
        {
            get => _titulo.Text;
            set { _titulo.Text = value; PerformLayout(); }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Texto
        {
            get => _caja.Text;
            set => _caja.Text = value ?? string.Empty;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Placeholder
        {
            get => _caja.PlaceholderText;
            set => _caja.PlaceholderText = value;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int MaxLength
        {
            get => _caja.MaxLength;
            set => _caja.MaxLength = value;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool EsClave
        {
            get => _esClave;
            set
            {
                _esClave = value;
                _caja.UseSystemPasswordChar = value;
                _mostrar.Visible = value;
                PerformLayout();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool SoloLectura
        {
            get => _caja.ReadOnly;
            set
            {
                _caja.ReadOnly = value;
                _caja.BackColor = value ? Tema.Fondo : Color.White;
                _caja.TabStop = !value;
                Invalidate();
            }
        }

        public CampoTexto()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);

            Height = 74;
            Margin = new Padding(0, 0, 0, 14);
            Anchor = AnchorStyles.Left | AnchorStyles.Right;

            Controls.Add(_titulo);
            Controls.Add(_caja);
            Controls.Add(_mostrar);

            _caja.Enter += (_, _) => { _enfocado = true; Invalidate(); };
            _caja.Leave += (_, _) => { _enfocado = false; Invalidate(); };
            _caja.TextChanged += (_, _) => TextoCambiado?.Invoke(this, EventArgs.Empty);
            _mostrar.Click += (_, _) => AlternarVisibilidad();
        }

        public void Enfocar()
        {
            _caja.Focus();
            _caja.SelectAll();
        }

        public void Limpiar() => _caja.Clear();

        private void AlternarVisibilidad()
        {
            _caja.UseSystemPasswordChar = !_caja.UseSystemPasswordChar;
            _mostrar.Text = _caja.UseSystemPasswordChar ? "Mostrar" : "Ocultar";
            PerformLayout();
            _caja.Focus();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            _caja.Focus();
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            base.OnLayout(e);

            int separacion = LogicalToDeviceUnits(6);
            int relleno = LogicalToDeviceUnits(12);

            _titulo.Location = new Point(0, 0);
            int arriba = _titulo.Height + separacion;

            _marco = new Rectangle(0, arriba, Math.Max(1, Width - 1), Math.Max(1, Height - arriba - 1));

            int anchoMostrar = _esClave ? _mostrar.Width + relleno : 0;
            _caja.Location = new Point(relleno, _marco.Top + (_marco.Height - _caja.Height) / 2);
            _caja.Width = Math.Max(10, Width - relleno * 2 - anchoMostrar);

            _mostrar.Location = new Point(Width - relleno - _mostrar.Width,
                                          _marco.Top + (_marco.Height - _mostrar.Height) / 2);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using GraphicsPath path = Tema.Redondeado(_marco, LogicalToDeviceUnits(8));
            using (var fondo = new SolidBrush(_caja.BackColor))
                g.FillPath(fondo, path);

            using var borde = new Pen(_enfocado ? Tema.Primario : Tema.Borde, _enfocado ? 2f : 1f);
            g.DrawPath(borde, path);
        }
    }
}
