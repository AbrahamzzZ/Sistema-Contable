using Entidad;
using SistemaContable.Entidad;

namespace SistemaContable.Datos.Repository
{
    public class MovimientoRepository
    {
        /// <summary>
        /// Registra un ingreso o egreso. mov.UsuarioRegistro lo asigna Negocio desde la sesión.
        /// Devuelve el ID nuevo.
        /// </summary>
        public int Insertar(Movimiento mov) =>
            DbHelper.Escalar<int>("SP_MOVIMIENTO_INSERTAR", new
            {
                FECHA_MOVIMIENTO = mov.Fecha_Movimiento.Date,
                ID_CATEGORIA = mov.Id_Categoria,
                MONTO = mov.Monto,
                DESCRIPCION = mov.Descripcion,
                NUM_COMPROBANTE = mov.Numero_Comprobante,
                FORMA_PAGO = mov.FormaPago.ACodigoBD(),  
                USUARIO_REGISTRO = mov.Usuario_Registro
            });

        public void Anular(int idMovimiento, string motivo, int idUsuarioAnulacion) =>
            DbHelper.Ejecutar("SP_MOVIMIENTO_ANULAR", new
            {
                ID_MOVIMIENTO = idMovimiento,
                MOTIVO_ANULACION = motivo,
                USUARIO_ANULACION = idUsuarioAnulacion
            });

        public MovimientoDetalle? Obtener(int idMovimiento) =>
            DbHelper.Obtener<MovimientoDetalle>("SP_MOVIMIENTO_OBTENER", new
            {
                ID_MOVIMIENTO = idMovimiento
            });

        public List<MovimientoDetalle> Listar(FiltroMovimiento filtro) =>
            DbHelper.Listar<MovimientoDetalle>("SP_MOVIMIENTO_LISTAR", new
            {
                DESDE = filtro.Desde.Date,
                HASTA = filtro.Hasta.Date,
                TIPO = filtro.Tipo,
                ID_CATEGORIA = filtro.IdCategoria,
                INCLUIR_ANULADOS = filtro.IncluirAnulados
            });
    }
}
