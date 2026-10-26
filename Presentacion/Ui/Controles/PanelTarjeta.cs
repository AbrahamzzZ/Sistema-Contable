using System.Drawing.Drawing2D;

namespace SistemaContable.Presentacion.Ui.Controles
{
    /// <summary>Contenedor blanco con esquinas redondeadas y borde suave.</summary>
    public class PanelTarjeta : Panel
    {
        public PanelTarjeta()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
            BackColor = Color.White;
            Padding = new Padding(32, 28, 32, 28);
        }

        /// <summary>Ajusta el alto de la tarjeta al de su contenido cada vez que este cambia.</summary>
        public void AjustarAlContenido(Control contenido)
        {
            contenido.SizeChanged += (_, _) => Height = contenido.Height + Padding.Vertical;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Parent?.BackColor ?? Tema.Fondo);

            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using GraphicsPath path = Tema.Redondeado(rect, LogicalToDeviceUnits(12));

            using (var fondo = new SolidBrush(BackColor))
                g.FillPath(fondo, path);

            using (var borde = new Pen(Tema.BordeSuave))
                g.DrawPath(borde, path);
        }
    }
}
