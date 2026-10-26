using SistemaContable.Presentacion.Ui;
using SistemaContable.Presentacion.Ui.Controles;

namespace SistemaContable.Presentacion.View
{
    public partial class Pendiente : UserControl
    {
        public Pendiente(string seccion)
        {
            SuspendLayout();
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;

            var tarjeta = new PanelTarjeta { Width = 560, Location = new Point(0, 0) };
            var columna = Tema.Columna();
            columna.Controls.Add(Tema.Seccion(seccion));
            columna.Controls.Add(Tema.Nota("Esta sección se agregará en la siguiente entrega."));

            tarjeta.Controls.Add(columna);
            tarjeta.AjustarAlContenido(columna);
            Controls.Add(tarjeta);

            ResumeLayout(false);
            PerformLayout();
        }
    }
}
