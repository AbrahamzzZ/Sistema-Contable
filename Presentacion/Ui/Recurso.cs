namespace SistemaContable.Presentacion.Ui
{
    /// <summary>Icono y logo de la aplicación. Si no se encuentran, la app funciona igual sin ellos.</summary>
    internal static class Recursos
    {
        private static readonly Lazy<Icon?> _icono = new(() =>
        {
            try
            {
                // Icono configurado en Propiedades del proyecto -> Recursos de Win32 -> Icono
                return Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            }
            catch
            {
                return null;
            }
        });

        private static readonly Lazy<Image?> _logo = new(() =>
        {
            try
            {
                string ruta = Path.Combine(AppContext.BaseDirectory, "Resource", "Logo-Iglesia.png");
                if (!File.Exists(ruta))
                    return null;

                using Image archivo = Image.FromFile(ruta);
                return new Bitmap(archivo);
            }
            catch
            {
                return null;
            }
        });

        public static Image? Logo => _logo.Value;

        public static void AplicarIcono(Form form)
        {
            if (_icono.Value is Icon icono)
                form.Icon = (Icon)icono.Clone(); 
        }

        /// <summary>Rectángulo para dibujar una imagen dentro de un área sin deformarla.</summary>
        public static Rectangle Ajustar(Size imagen, Rectangle area)
        {
            if (imagen.Width <= 0 || imagen.Height <= 0)
                return area;

            float escala = Math.Min((float)area.Width / imagen.Width, (float)area.Height / imagen.Height);
            int ancho = (int)(imagen.Width * escala);
            int alto = (int)(imagen.Height * escala);

            return new Rectangle(area.X + (area.Width - ancho) / 2, area.Y + (area.Height - alto) / 2, ancho, alto);
        }
    }
}
