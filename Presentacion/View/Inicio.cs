using SistemaContable.Entidad;
using SistemaContable.Negocio;
using SistemaContable.Presentacion.Ui;
using SistemaContable.Presentacion.Ui.Controles;
using SistemaContable.Utilities;

namespace SistemaContable.Presentacion.View
{
    /// <summary>Panel de inicio: saludo, resumen del mes, acumulado del año y accesos rápidos.</summary>
    public partial class Inicio : UserControl
    {
        private readonly ReporteService _reporteService = new();
        private readonly Action<string> _navegar;

        private readonly Label _lblSaludo = Tema.Titulo(string.Empty);
        private readonly Label _lblPeriodo = Tema.Subtitulo(string.Empty);

        private readonly TarjetaResumen _mesIngresos = new("Ingresos del mes", Tema.Ingreso);
        private readonly TarjetaResumen _mesEgresos = new("Egresos del mes", Tema.Egreso);
        private readonly TarjetaResumen _mesSaldo = new("Saldo del mes", Tema.Primario);
        private readonly TarjetaResumen _anioIngresos = new("Ingresos del año", Tema.Ingreso);
        private readonly TarjetaResumen _anioEgresos = new("Egresos del año", Tema.Egreso);
        private readonly TarjetaResumen _anioSaldo = new("Saldo del año", Tema.Primario);

        public Inicio(Action<string> navegar)
        {
            _navegar = navegar;

            SuspendLayout();
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoScroll = true;

            var columna = Tema.Columna();
            columna.Controls.Add(_lblSaludo);
            columna.Controls.Add(_lblPeriodo);
            columna.Controls.Add(FilaTarjetas(_mesIngresos, _mesEgresos, _mesSaldo));
            columna.Controls.Add(Tema.Seccion($"Acumulado {DateTime.Today.Year}"));
            columna.Controls.Add(FilaTarjetas(_anioIngresos, _anioEgresos, _anioSaldo));
            columna.Controls.Add(Tema.Seccion("Acciones rápidas"));

            var acciones = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new Padding(0) };
            acciones.Controls.Add(BotonAccion("+  Registrar ingreso", EstiloBoton.Exito, Secciones.RegistrarIngreso));
            acciones.Controls.Add(BotonAccion("+  Registrar egreso", EstiloBoton.Peligro, Secciones.RegistrarEgreso));
            acciones.Controls.Add(BotonAccion("Ver movimientos", EstiloBoton.Secundario, Secciones.Movimientos));
            acciones.Controls.Add(BotonAccion("Ver reportes", EstiloBoton.Secundario, Secciones.Reportes));
            columna.Controls.Add(acciones);

            Controls.Add(columna);

            ResumeLayout(false);
            PerformLayout();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            CargarResumen();
        }

        private void CargarResumen()
        {
            DateTime hoy = DateTime.Today;
            _lblSaludo.Text = $"Hola, {Sesion.NombreCompleto}";

            try
            {
                List<ResumenMensualFila> meses = _reporteService.ResumenAnual(hoy.Year);
                ResumenMensualFila mes = meses.FirstOrDefault(f => f.Mes == hoy.Month)
                                         ?? new ResumenMensualFila { Mes = hoy.Month };

                _lblPeriodo.Text = $"Este es el resumen de {mes.NombreMes.ToLower()} de {hoy.Year}.";

                MostrarTrio(_mesIngresos, _mesEgresos, _mesSaldo, mes.Ingresos, mes.Egresos, mes.Saldo);
                MostrarTrio(_anioIngresos, _anioEgresos, _anioSaldo,
                            meses.Sum(m => m.Ingresos), meses.Sum(m => m.Egresos), meses.Sum(m => m.Saldo));
            }
            catch (Exception ex)
            {
                Mensajes.MostrarExcepcion(ex);
            }
        }

        private static void MostrarTrio(TarjetaResumen ingresos, TarjetaResumen egresos, TarjetaResumen saldo,
                                        decimal totalIngresos, decimal totalEgresos, decimal totalSaldo)
        {
            ingresos.MostrarValor(totalIngresos, Tema.Ingreso);
            ingresos.MostrarDetalle("Diezmos, ofrendas y otros");

            egresos.MostrarValor(totalEgresos, Tema.Egreso);
            egresos.MostrarDetalle("Servicios, misceláneos y escuela");

            saldo.MostrarValor(totalSaldo, totalSaldo < 0 ? Tema.Egreso : Tema.Texto);
            saldo.MostrarDetalle(totalSaldo < 0 ? "Los egresos superan los ingresos" : "Ingresos menos egresos");
        }

        private static TableLayoutPanel FilaTarjetas(params TarjetaResumen[] tarjetas)
        {
            var fila = new TableLayoutPanel
            {
                ColumnCount = tarjetas.Length,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Margin = new Padding(0, 0, 0, 24)
            };

            for (int i = 0; i < tarjetas.Length; i++)
            {
                fila.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / tarjetas.Length));
                tarjetas[i].Margin = i == tarjetas.Length - 1 ? new Padding(0) : new Padding(0, 0, 16, 0);
                fila.Controls.Add(tarjetas[i], i, 0);
            }

            return fila;
        }

        private Boton BotonAccion(string texto, EstiloBoton estilo, string seccion)
        {
            var boton = new Boton { Text = texto, Estilo = estilo };
            boton.Click += (_, _) => _navegar(seccion);
            return boton;
        }
    }
}
