using System.Drawing.Drawing2D;

namespace SistemaContable.Presentacion.Ui.Controles
{
    /// <summary>Panel izquierdo de la ventana de acceso: degradado, logo y descripción del sistema.</summary>
    public class PanelMarca : Panel
    {
        public PanelMarca()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
            BackColor = Tema.Barra;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (Width <= 0 || Height <= 0)
                return;

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Fondo degradado
            using (var degradado = new LinearGradientBrush(ClientRectangle, Tema.Primario, Tema.Barra, 135f))
                g.FillRectangle(degradado, ClientRectangle);

            // Círculos decorativos
            using (var circulo = new SolidBrush(Color.FromArgb(20, Color.White)))
            {
                int d1 = LogicalToDeviceUnits(340);
                int d2 = LogicalToDeviceUnits(180);
                g.FillEllipse(circulo, Width - d1 / 2, Height - d1 / 2 - LogicalToDeviceUnits(40), d1, d1);
                g.FillEllipse(circulo, -d2 / 3, -d2 / 3, d2, d2);
            }

            int x = LogicalToDeviceUnits(44);
            int y = LogicalToDeviceUnits(72);
            y = DibujarTexto(g, "Sistema Contable", Tema.FuenteMarca, Color.White, x, y) + LogicalToDeviceUnits(8);
            y = DibujarTexto(g, "Administración de ingresos y egresos de la iglesia.",
                             Tema.FuenteSubtitulo, Tema.TextoBarra, x, y) + LogicalToDeviceUnits(40);

            string[] puntos =
            {
                "Registro de diezmos y ofrendas",
                "Control de gastos por categoría",
                "Reportes mensuales y anuales"
            };

            foreach (string punto in puntos)
            {
                int d = LogicalToDeviceUnits(8);
                using (var vineta = new SolidBrush(Tema.Acento))
                    g.FillEllipse(vineta, x, y + LogicalToDeviceUnits(7), d, d);

                y = DibujarTexto(g, punto, Tema.Fuente, Color.White, x + LogicalToDeviceUnits(20), y)
                    + LogicalToDeviceUnits(14);
            }

            TextRenderer.DrawText(g, $"© {DateTime.Today.Year}  ·  Versión 1.0", Tema.FuentePequena,
                new Point(x, Height - LogicalToDeviceUnits(48)), Tema.TextoBarraTenue);
        }

        private int DibujarTexto(Graphics g, string texto, Font fuente, Color color, int x, int y)
        {
            int ancho = Math.Max(50, Width - x - LogicalToDeviceUnits(36));
            Size medida = TextRenderer.MeasureText(g, texto, fuente, new Size(ancho, 0), TextFormatFlags.WordBreak);

            TextRenderer.DrawText(g, texto, fuente, new Rectangle(x, y, ancho, medida.Height), color,
                TextFormatFlags.WordBreak | TextFormatFlags.Left | TextFormatFlags.Top);

            return y + medida.Height;
        }
    }
}
