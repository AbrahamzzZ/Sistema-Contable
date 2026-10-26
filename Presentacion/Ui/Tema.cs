using System.Drawing.Drawing2D;

namespace SistemaContable.Presentacion.Ui
{
    /// <summary>Paleta, fuentes y utilidades visuales compartidas por toda la aplicación.</summary>
    internal static class Tema
    {
        // Colores principales
        public static readonly Color Primario = Color.FromArgb(37, 99, 235);
        public static readonly Color PrimarioHover = Color.FromArgb(29, 78, 216);
        public static readonly Color PrimarioPresionado = Color.FromArgb(30, 64, 175);
        public static readonly Color PrimarioSuave = Color.FromArgb(219, 234, 254);
        public static readonly Color Acento = Color.FromArgb(96, 165, 250);

        // Barra lateral
        public static readonly Color Barra = Color.FromArgb(15, 23, 42);
        public static readonly Color BarraHover = Color.FromArgb(30, 41, 59);
        public static readonly Color BarraSeleccion = Color.FromArgb(37, 52, 80);
        public static readonly Color TextoBarra = Color.FromArgb(203, 213, 225);
        public static readonly Color TextoBarraTenue = Color.FromArgb(100, 116, 139);

        // Superficies y texto
        public static readonly Color Fondo = Color.FromArgb(241, 245, 249);
        public static readonly Color Superficie = Color.White;
        public static readonly Color Borde = Color.FromArgb(203, 213, 225);
        public static readonly Color BordeSuave = Color.FromArgb(226, 232, 240);
        public static readonly Color Texto = Color.FromArgb(15, 23, 42);
        public static readonly Color TextoSecundario = Color.FromArgb(100, 116, 139);

        // Estados
        public static readonly Color Ingreso = Color.FromArgb(22, 163, 74);
        public static readonly Color IngresoHover = Color.FromArgb(21, 128, 61);
        public static readonly Color Egreso = Color.FromArgb(220, 38, 38);
        public static readonly Color EgresoHover = Color.FromArgb(185, 28, 28);
        public static readonly Color ErrorFondo = Color.FromArgb(254, 242, 242);
        public static readonly Color ErrorTexto = Color.FromArgb(185, 28, 28);
        public static readonly Color ExitoFondo = Color.FromArgb(240, 253, 244);
        public static readonly Color ExitoTexto = Color.FromArgb(21, 128, 61);
        public static readonly Color Deshabilitado = Color.FromArgb(226, 232, 240);
        public static readonly Color TextoDeshabilitado = Color.FromArgb(148, 163, 184);

        // Fuentes
        public static readonly Font Fuente = new("Segoe UI", 10F);
        public static readonly Font FuentePequena = new("Segoe UI", 9F);
        public static readonly Font FuenteMini = new("Segoe UI Semibold", 8F);
        public static readonly Font FuenteEtiqueta = new("Segoe UI Semibold", 9.5F);
        public static readonly Font FuenteCampo = new("Segoe UI", 11F);
        public static readonly Font FuenteBoton = new("Segoe UI Semibold", 10F);
        public static readonly Font FuenteNavActiva = new("Segoe UI Semibold", 10F);
        public static readonly Font FuenteSeccion = new("Segoe UI Semibold", 12F);
        public static readonly Font FuenteEncabezado = new("Segoe UI Semibold", 15F);
        public static readonly Font FuenteTitulo = new("Segoe UI Semibold", 20F);
        public static readonly Font FuenteSubtitulo = new("Segoe UI", 10.5F);
        public static readonly Font FuenteValor = new("Segoe UI Semibold", 20F);
        public static readonly Font FuenteMarca = new("Segoe UI Semibold", 24F);
        public static readonly Font FuenteLogo = new("Segoe UI Semibold", 16F);

        /// <summary>Rectángulo con esquinas redondeadas para dibujar tarjetas, botones y campos.</summary>
        public static GraphicsPath Redondeado(Rectangle r, int radio)
        {
            var path = new GraphicsPath();
            int d = Math.Max(1, Math.Min(radio * 2, Math.Min(r.Width, r.Height)));

            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        public static string Moneda(decimal valor) => valor.ToString("$ #,##0.00;-$ #,##0.00");

        /// <summary>Columna vertical: cada control agregado ocupa una fila y todo el ancho.</summary>
        public static TableLayoutPanel Columna()
        {
            var columna = new TableLayoutPanel
            {
                ColumnCount = 1,
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            columna.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            return columna;
        }

        public static Label Titulo(string texto) => new()
        {
            Text = texto,
            AutoSize = true,
            Font = FuenteTitulo,
            ForeColor = Texto,
            Margin = new Padding(0, 0, 0, 4)
        };

        public static Label Subtitulo(string texto) => new()
        {
            Text = texto,
            AutoSize = true,
            Font = FuenteSubtitulo,
            ForeColor = TextoSecundario,
            Margin = new Padding(0, 0, 0, 24)
        };

        public static Label Seccion(string texto) => new()
        {
            Text = texto,
            AutoSize = true,
            Font = FuenteSeccion,
            ForeColor = Texto,
            Margin = new Padding(0, 8, 0, 12)
        };

        public static Label Nota(string texto) => new()
        {
            Text = texto,
            AutoSize = true,
            Font = FuentePequena,
            ForeColor = TextoSecundario,
            Margin = new Padding(0, 0, 0, 16)
        };
    }
}
