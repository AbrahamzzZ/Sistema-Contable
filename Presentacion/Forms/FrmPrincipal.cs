using SistemaContable.Presentacion.Ui;
using SistemaContable.Presentacion.Ui.Controles;
using SistemaContable.Presentacion.View;
using SistemaContable.Utilities;
using System.Globalization;

namespace SistemaContable.Presentacion.Forms
{
    public partial class FrmPrincipal : Form
    {
        private static readonly CultureInfo Es = new("es-EC");

        private readonly Panel _contenido = new()
        {
            Dock = DockStyle.Fill,
            BackColor = Tema.Fondo,
            Padding = new Padding(32, 28, 32, 28)
        };

        private readonly Label _lblTitulo = new() { AutoSize = true, Font = Tema.FuenteEncabezado, ForeColor = Tema.Texto, Anchor = AnchorStyles.Left };
        private readonly Label _lblFecha = new() { AutoSize = true, ForeColor = Tema.TextoSecundario, Anchor = AnchorStyles.Right };
        private readonly Label _lblNombre = new()
        {
            AutoSize = false,
            AutoEllipsis = true,
            Height = 22,
            Font = Tema.FuenteEtiqueta,
            ForeColor = Color.White,
            Anchor = AnchorStyles.Left | AnchorStyles.Right
        };
        private readonly Label _lblUsername = new()
        {
            AutoSize = false,
            AutoEllipsis = true,
            Height = 20,
            Font = Tema.FuentePequena,
            ForeColor = Tema.TextoBarraTenue,
            Anchor = AnchorStyles.Left | AnchorStyles.Right
        };

        private readonly Dictionary<string, BotonNavegacion> _opciones = new();
        private readonly Dictionary<string, string> _titulos = new()
        {
            [Secciones.Inicio] = "Inicio",
            [Secciones.RegistrarIngreso] = "Registrar ingreso",
            [Secciones.RegistrarEgreso] = "Registrar egreso",
            [Secciones.Movimientos] = "Movimientos",
            [Secciones.Reportes] = "Reportes",
            [Secciones.Categorias] = "Categorías",
            [Secciones.Usuarios] = "Usuarios",
            [Secciones.MiClave] = "Cambiar mi clave"
        };

        private Control? _vistaActual;

        /// <summary>True si el usuario eligió "Cerrar sesión" (Program vuelve a mostrar el acceso).</summary>
        public bool CerrarSesionSolicitado { get; private set; }

        public FrmPrincipal()
        {
            SuspendLayout();
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            Text = "Sistema Contable";
            Font = Tema.Fuente;
            BackColor = Tema.Fondo;
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(1220, 760);
            MinimumSize = new Size(1020, 640);
            Recursos.AplicarIcono(this);

            Controls.Add(_contenido);
            Controls.Add(CrearEncabezado());
            Controls.Add(CrearBarraLateral());

            Load += (_, _) =>
            {
                _lblNombre.Text = Sesion.NombreCompleto;
                _lblUsername.Text = "@" + Sesion.Username;
                string fecha = DateTime.Today.ToString("dddd, d 'de' MMMM 'de' yyyy", Es);
                _lblFecha.Text = char.ToUpper(fecha[0]) + fecha[1..];
                Navegar(Secciones.Inicio);
            };

            ResumeLayout(false);
            PerformLayout();
        }

        private Panel CrearEncabezado()
        {
            var encabezado = new Panel
            {
                Dock = DockStyle.Top,
                Height = 72,
                BackColor = Tema.Superficie,
                Padding = new Padding(32, 0, 32, 1)   
            };

            var fila = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
            fila.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            fila.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            fila.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            fila.Controls.Add(_lblTitulo, 0, 0);
            fila.Controls.Add(_lblFecha, 1, 0);
            encabezado.Controls.Add(fila);

            encabezado.Paint += (_, e) =>
            {
                using var linea = new Pen(Tema.BordeSuave);
                e.Graphics.DrawLine(linea, 0, encabezado.Height - 1, encabezado.Width, encabezado.Height - 1);
            };

            return encabezado;
        }

        private Panel CrearBarraLateral()
        {
            var barra = new Panel { Dock = DockStyle.Left, Width = 250, BackColor = Tema.Barra };

            // Logo
            var logo = new Panel { Dock = DockStyle.Top, Height = 84 };
            logo.Paint += (_, e) => DibujarLogo(logo, e.Graphics);

            // Opciones de navegación
            var menu = Tema.Columna();
            menu.Padding = new Padding(12, 4, 12, 4);
            AgregarTituloMenu(menu, "PRINCIPAL");
            AgregarOpcion(menu, Secciones.Inicio, "Inicio");
            AgregarTituloMenu(menu, "MOVIMIENTOS");
            AgregarOpcion(menu, Secciones.RegistrarIngreso, "Registrar ingreso");
            AgregarOpcion(menu, Secciones.RegistrarEgreso, "Registrar egreso");
            AgregarOpcion(menu, Secciones.Movimientos, "Listado de movimientos");
            AgregarTituloMenu(menu, "INFORMES");
            AgregarOpcion(menu, Secciones.Reportes, "Reportes");
            AgregarTituloMenu(menu, "ADMINISTRACIÓN");
            AgregarOpcion(menu, Secciones.Categorias, "Categorías");
            AgregarOpcion(menu, Secciones.Usuarios, "Usuarios");
            AgregarTituloMenu(menu, "MI CUENTA");
            AgregarOpcion(menu, Secciones.MiClave, "Cambiar mi clave");

            // Usuario y cerrar sesión
            var pie = new TableLayoutPanel
            {
                Dock = DockStyle.Bottom,
                ColumnCount = 1,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(16, 14, 16, 16)
            };
            pie.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            var btnCerrar = new Boton
            {
                Text = "Cerrar sesión",
                Estilo = EstiloBoton.Barra,
                AutoSize = false,
                Height = 38,
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Margin = new Padding(0, 12, 0, 0)
            };
            btnCerrar.Click += (_, _) => CerrarSesion();

            pie.Controls.Add(_lblNombre);
            pie.Controls.Add(_lblUsername);
            pie.Controls.Add(btnCerrar);
            pie.Paint += (_, e) =>
            {
                using var linea = new Pen(Tema.BarraHover);
                e.Graphics.DrawLine(linea, 0, 0, pie.Width, 0);
            };

            barra.Controls.Add(menu); 
            barra.Controls.Add(logo); 
            barra.Controls.Add(pie); 
            return barra;
        }

        private static void DibujarLogo(Control panel, Graphics g)
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

            int x = panel.LogicalToDeviceUnits(20);
            int lado = panel.LogicalToDeviceUnits(44);
            var cuadro = new Rectangle(x, (panel.Height - lado) / 2, lado, lado);

            Image? imagen = Recursos.Logo;

            using (var path = Tema.Redondeado(cuadro, panel.LogicalToDeviceUnits(10)))
            using (var fondo = new SolidBrush(imagen is null ? Tema.Primario : Color.White))
                g.FillPath(fondo, path);

            if (imagen is not null)
            {
                int margen = panel.LogicalToDeviceUnits(4);
                g.DrawImage(imagen, Recursos.Ajustar(imagen.Size, Rectangle.Inflate(cuadro, -margen, -margen)));
            }
            else
            {
                TextRenderer.DrawText(g, "SC", Tema.FuenteBoton, cuadro, Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
            }

            int inicioTexto = cuadro.Right + panel.LogicalToDeviceUnits(12);
            var areaTexto = new Rectangle(inicioTexto, 0, panel.Width - inicioTexto - panel.LogicalToDeviceUnits(12), panel.Height);
            TextRenderer.DrawText(g, "Sistema Contable", Tema.FuenteSeccion, areaTexto, Color.White,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);
        }

        private static void AgregarTituloMenu(TableLayoutPanel menu, string texto)
        {
            menu.Controls.Add(new Label
            {
                Text = texto,
                AutoSize = true,
                Font = Tema.FuenteMini,
                ForeColor = Tema.TextoBarraTenue,
                Margin = new Padding(12, 16, 0, 6)
            });
        }

        private void AgregarOpcion(TableLayoutPanel menu, string seccion, string texto)
        {
            var opcion = new BotonNavegacion { Text = texto };
            opcion.Click += (_, _) => Navegar(seccion);
            _opciones[seccion] = opcion;
            menu.Controls.Add(opcion);
        }

        private void Navegar(string seccion)
        {
            Control vista = seccion switch
            {
                Secciones.Inicio => new Inicio(Navegar),
                Secciones.MiClave => new CambiarClave(),
                _ => new Pendiente(_titulos[seccion])
            };

            Control? anterior = _vistaActual;

            // Se agrega al fondo (detrás de la vista actual) para que se cree y acomode sin verse
            vista.Dock = DockStyle.Fill;
            _contenido.Controls.Add(vista);
            vista.SendToBack();

            // Ya lista: se quita la anterior y queda visible la nueva
            if (anterior is not null)
                _contenido.Controls.Remove(anterior);

            _vistaActual = vista;
            _lblTitulo.Text = _titulos[seccion];
            AcceptButton = (vista as IVistaConAccion)?.BotonPrincipal;

            foreach (var (clave, opcion) in _opciones)
                opcion.Seleccionado = clave == seccion;

            // La vista anterior se libera después: puede ser la que disparó este clic
            if (anterior is not null)
                BeginInvoke(new MethodInvoker(anterior.Dispose));
        }

        private void CerrarSesion()
        {
            if (!Mensajes.Confirmar("¿Desea cerrar la sesión?"))
                return;

            CerrarSesionSolicitado = true;
            Close();
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;  
                return cp;
            }
        }
    }
}
