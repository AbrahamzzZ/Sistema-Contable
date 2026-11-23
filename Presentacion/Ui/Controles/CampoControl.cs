namespace SistemaContable.Presentacion.Ui.Controles
{
    /// <summary>Título + control con el mismo estilo que CampoTexto.</summary>
    public class CampoControl : TableLayoutPanel
    {
        public Label Etiqueta { get; }

        public CampoControl(string titulo, Control control)
        {
            ColumnCount = 1;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Anchor = AnchorStyles.Left | AnchorStyles.Right;
            Margin = new Padding(0, 0, 0, 14);
            ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            Etiqueta = new Label
            {
                Text = titulo,
                AutoSize = true,
                Font = Tema.FuenteEtiqueta,
                ForeColor = Tema.Texto,
                Margin = new Padding(0, 0, 0, 6)
            };

            control.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            control.Margin = new Padding(0);

            Controls.Add(Etiqueta);
            Controls.Add(control);
        }
    }
}
