using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace SistemaContable.Presentacion.Ui.Controles
{
    public enum EstiloBoton { Primario, Secundario, Exito, Peligro, Enlace, Barra }

    /// <summary>Botón con esquinas redondeadas y colores para normal, hover, presionado y deshabilitado.</summary>
    public class Boton : Button
    {
        private EstiloBoton _estilo = EstiloBoton.Primario;
        private bool _hover;
        private bool _presionado;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public EstiloBoton Estilo
        {
            get => _estilo;
            set { _estilo = value; Invalidate(); }
        }

        public Boton()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            Font = Tema.FuenteBoton;
            Cursor = Cursors.Hand;
            AutoSize = true;
            MinimumSize = new Size(120, 42);
            Padding = new Padding(18, 0, 18, 0);
            Margin = new Padding(0, 0, 10, 0);
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            Size texto = TextRenderer.MeasureText(Text, Font);
            return new Size(Math.Max(MinimumSize.Width, texto.Width + Padding.Horizontal),
                            Math.Max(MinimumSize.Height, texto.Height + LogicalToDeviceUnits(16)));
        }

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; _presionado = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnEnabledChanged(EventArgs e) { Invalidate(); base.OnEnabledChanged(e); }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) { _presionado = true; Invalidate(); }
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            _presionado = false;
            Invalidate();
            base.OnMouseUp(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Parent?.BackColor ?? Tema.Fondo);

            (Color fondo, Color texto, Color borde) = ObtenerColores();
            var rect = new Rectangle(0, 0, Width - 1, Height - 1);

            using (GraphicsPath path = Tema.Redondeado(rect, LogicalToDeviceUnits(8)))
            {
                if (fondo != Color.Empty)
                {
                    using var brocha = new SolidBrush(fondo);
                    g.FillPath(brocha, path);
                }
                if (borde != Color.Empty)
                {
                    using var lapiz = new Pen(borde);
                    g.DrawPath(lapiz, path);
                }
            }

            // Indicador de foco al navegar con teclado (Tab)
            if (Focused && ShowFocusCues)
            {
                var foco = Rectangle.Inflate(rect, -3, -3);
                using GraphicsPath pathFoco = Tema.Redondeado(foco, LogicalToDeviceUnits(6));
                using var lapizFoco = new Pen(Color.FromArgb(150, texto)) { DashStyle = DashStyle.Dot };
                g.DrawPath(lapizFoco, pathFoco);
            }

            TextRenderer.DrawText(g, Text, Font, ClientRectangle, texto,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);
        }

        private (Color Fondo, Color Texto, Color Borde) ObtenerColores()
        {
            if (!Enabled)
            {
                return _estilo is EstiloBoton.Enlace or EstiloBoton.Barra
                    ? (Color.Empty, Tema.TextoDeshabilitado, Color.Empty)
                    : (Tema.Deshabilitado, Tema.TextoDeshabilitado, Color.Empty);
            }

            return _estilo switch
            {
                EstiloBoton.Primario => (Elegir(Tema.Primario, Tema.PrimarioHover, Tema.PrimarioPresionado), Color.White, Color.Empty),
                EstiloBoton.Exito => (Elegir(Tema.Ingreso, Tema.IngresoHover, Tema.IngresoHover), Color.White, Color.Empty),
                EstiloBoton.Peligro => (Elegir(Tema.Egreso, Tema.EgresoHover, Tema.EgresoHover), Color.White, Color.Empty),
                EstiloBoton.Secundario => (Elegir(Color.White, Tema.Fondo, Tema.BordeSuave), Tema.Texto, Tema.Borde),
                EstiloBoton.Enlace => (_hover ? Tema.PrimarioSuave : Color.Empty, Tema.Primario, Color.Empty),
                EstiloBoton.Barra => (_hover ? Tema.BarraHover : Color.Empty, Tema.TextoBarra, Tema.BarraHover),
                _ => (Tema.Primario, Color.White, Color.Empty)
            };
        }

        private Color Elegir(Color normal, Color hover, Color presionado) =>
            _presionado ? presionado : _hover ? hover : normal;
    }
}
