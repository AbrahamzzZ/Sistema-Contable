using SistemaContable.Datos;

namespace SistemaContable.Negocio
{
    /// <summary>
    /// Operaciones generales del sistema. Existe para que Presentación pueda probar la conexión
    /// al arrancar sin referenciar a la capa Datos.
    /// </summary>
    public class SistemaService
    {
        public bool ProbarConexion(out string mensaje) => Conexion.Probar(out mensaje);
    }
}
