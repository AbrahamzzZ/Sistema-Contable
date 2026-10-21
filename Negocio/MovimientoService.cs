using Entidad;
using SistemaContable.Datos.Repository;
using SistemaContable.Entidad;
using SistemaContable.Utilities;

namespace SistemaContable.Negocio
{
    public class MovimientoService
    {
        private const decimal MontoMaximo = 9_999_999_999.99m;

        private readonly MovimientoRepository _repo = new();

        /// <summary>Registra un ingreso o egreso. El usuario se toma de la sesión. Devuelve el ID.</summary>
        public int Registrar(Movimiento movimiento)
        {
            int idUsuario = Sesion.ObtenerIdUsuarioActual();

            if (movimiento.Id_Categoria <= 0)
                throw new NegocioException("Seleccione la categoría.");

            if (movimiento.Monto <= 0)
                throw new NegocioException("El monto debe ser mayor a cero.");

            if (movimiento.Monto > MontoMaximo)
                throw new NegocioException("El monto supera el máximo permitido.");

            if (decimal.Round(movimiento.Monto, 2) != movimiento.Monto)
                throw new NegocioException("El monto solo puede tener hasta 2 decimales.");

            if (movimiento.Fecha_Movimiento.Date > DateTime.Today)
                throw new NegocioException("La fecha del movimiento no puede ser futura.");

            string? descripcion = Limpiar(movimiento.Descripcion);
            if (descripcion?.Length > 250)
                throw new NegocioException("La descripción no puede superar 250 caracteres.");

            string? comprobante = Limpiar(movimiento.Numero_Comprobante);
            if (comprobante?.Length > 50)
                throw new NegocioException("El número de comprobante no puede superar 50 caracteres.");

            if (!Enum.IsDefined(movimiento.FormaPago))
                throw new NegocioException("Seleccione una forma de pago válida.");

            var nuevo = new Movimiento
            {
                Fecha_Movimiento = movimiento.Fecha_Movimiento.Date,
                Id_Categoria = movimiento.Id_Categoria,
                Monto = movimiento.Monto,
                Descripcion = descripcion,
                Numero_Comprobante = comprobante,
                FormaPago = movimiento.FormaPago,
                Usuario_Registro = idUsuario
            };

            // La regla "descripción obligatoria en Otros" la valida el procedimiento
            return ManejadorErrores.Ejecutar(() => _repo.Insertar(nuevo));
        }

        /// <summary>Anula un movimiento con motivo obligatorio. El usuario se toma de la sesión.</summary>
        public void Anular(int idMovimiento, string motivo)
        {
            int idUsuario = Sesion.ObtenerIdUsuarioActual();

            if (idMovimiento <= 0)
                throw new NegocioException("Seleccione el movimiento a anular.");

            string? motivoLimpio = Limpiar(motivo);

            if (motivoLimpio is null)
                throw new NegocioException("Debe indicar el motivo de la anulación.");

            if (motivoLimpio.Length > 250)
                throw new NegocioException("El motivo no puede superar 250 caracteres.");

            ManejadorErrores.Ejecutar(() => _repo.Anular(idMovimiento, motivoLimpio, idUsuario));
        }

        public MovimientoDetalle Obtener(int idMovimiento)
        {
            if (idMovimiento <= 0)
                throw new NegocioException("Seleccione un movimiento.");

            return ManejadorErrores.Ejecutar(() => _repo.Obtener(idMovimiento))
                   ?? throw new NegocioException("El movimiento no existe.");
        }

        /// <summary>Listado con filtros. Sin filtro: mes actual, ingresos y egresos, sin anulados.</summary>
        public List<MovimientoDetalle> Listar(FiltroMovimiento? filtro = null)
        {
            filtro ??= new FiltroMovimiento();

            if (filtro.Desde.Date > filtro.Hasta.Date)
                throw new NegocioException("La fecha inicial no puede ser mayor que la fecha final.");

            if (filtro.Tipo is not null && !TipoMovimiento.EsValido(filtro.Tipo))
                throw new NegocioException("Tipo de movimiento no válido.");

            return ManejadorErrores.Ejecutar(() => _repo.Listar(filtro));
        }

        /// <summary>Totales de un listado (para mostrar al pie de la grilla). No cuenta los anulados.</summary>
        public static (decimal Ingresos, decimal Egresos, decimal Saldo) CalcularTotales(IEnumerable<MovimientoDetalle> movimientos)
        {
            var vigentes = movimientos.Where(m => !m.Anulado).ToList();

            decimal ingresos = vigentes.Where(m => m.Tipo == TipoMovimiento.Ingreso).Sum(m => m.Monto);
            decimal egresos = vigentes.Where(m => m.Tipo == TipoMovimiento.Egreso).Sum(m => m.Monto);

            return (ingresos, egresos, ingresos - egresos);
        }

        private static string? Limpiar(string? texto) =>
            string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();
    }
}
