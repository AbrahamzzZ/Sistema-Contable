namespace SistemaContable.Presentacion.Ui.Controles
{
    /// <summary>Mensaje de error o éxito dentro de la pantalla (reemplaza las ventanas emergentes).</summary>
    public class Aviso : Label
    {
        public Aviso()
        {
            AutoSize = false;
            Visible = false;
            Font = Tema.Fuente;
            Padding = new Padding(14, 10, 14, 10);
            Margin = new Padding(0, 0, 0, 16);
            TextAlign = ContentAlignment.MiddleLeft;
            Anchor = AnchorStyles.Left | AnchorStyles.Right;
        }

        public void MostrarError(string mensaje) => Mostrar(mensaje, Tema.ErrorFondo, Tema.ErrorTexto);

        public void MostrarExito(string mensaje) => Mostrar(mensaje, Tema.ExitoFondo, Tema.ExitoTexto);

        public void Ocultar() => Visible = false;

        private void Mostrar(string mensaje, Color fondo, Color texto)
        {
            BackColor = fondo;
            ForeColor = texto;
            Text = mensaje;
            Visible = true;

            // Primero se ubica para conocer el ancho real; luego se calcula el alto según el texto
            Parent?.PerformLayout();
            int ancho = Math.Max(100, Width - Padding.Horizontal);
            Size medida = TextRenderer.MeasureText(mensaje, Font, new Size(ancho, 0), TextFormatFlags.WordBreak);
            Height = medida.Height + Padding.Vertical;
        }
    }
}
