using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace SistemaContable.Presentacion.Ui.Controles
{
    /// <summary>Opción del menú lateral: resalta la sección activa con fondo y una barra de color.</summary>
    public class BotonNavegacion : Button
    {
        private bool _hover;
        private bool _seleccionado;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Seleccionado
        {
            get => _seleccionado;
            set { _seleccionado = value; Invalidate(); }
        }

        public BotonNavegacion()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            Font = Tema.Fuente;
            Cursor = Cursors.Hand;
            Height = 42;
            Margin = new Padding(0, 2, 0, 2);
            Anchor = AnchorStyles.Left | AnchorStyles.Right;
        }

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Parent?.BackColor ?? Tema.Barra);

            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            Color? fondo = _seleccionado ? Tema.BarraSeleccion : _hover ? Tema.BarraHover : null;

            if (fondo is Color color)
            {
                using GraphicsPath path = Tema.Redondeado(rect, LogicalToDeviceUnits(8));
                using var brocha = new SolidBrush(color);
                g.FillPath(brocha, path);
            }

            if (_seleccionado)
            {
                int ancho = LogicalToDeviceUnits(4);
                int alto = Height / 2;
                using var acento = new SolidBrush(Tema.Acento);
                g.FillRectangle(acento, 0, (Height - alto) / 2, ancho, alto);
            }

            var area = new Rectangle(LogicalToDeviceUnits(16), 0, Width - LogicalToDeviceUnits(24), Height);
            TextRenderer.DrawText(g, Text, _seleccionado ? Tema.FuenteNavActiva : Font, area,
                _seleccionado ? Color.White : Tema.TextoBarra,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter |
                TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);
        }
    }
}
