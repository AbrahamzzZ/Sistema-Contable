using Dapper;
using System.Data;

namespace SistemaContable.Datos
{
    public static class DbHelper
    {
        private const int TiempoEsperaSegundos = 30;

        static DbHelper()
        {
            DefaultTypeMap.MatchNamesWithUnderscores = true;
        }

        /// <summary>Devuelve una lista (SP_..._LISTAR, reportes).</summary>
        public static List<T> Listar<T>(string procedimiento, object? parametros = null)
        {
            using var cn = Conexion.Crear();
            return cn.Query<T>(procedimiento, parametros, commandType: CommandType.StoredProcedure, commandTimeout: TiempoEsperaSegundos).AsList();
        }

        /// <summary>Devuelve un solo registro o null si no existe (SP_..._OBTENER, LOGIN).</summary>
        public static T? Obtener<T>(string procedimiento, object? parametros = null)
        {
            using var cn = Conexion.Crear();
            return cn.QueryFirstOrDefault<T>(procedimiento, parametros, commandType: CommandType.StoredProcedure, commandTimeout: TiempoEsperaSegundos);
        }

        /// <summary>Devuelve un solo valor (el ID que devuelven los SP_..._INSERTAR).</summary>
        public static T? Escalar<T>(string procedimiento, object? parametros = null)
        {
            using var cn = Conexion.Crear();
            return cn.ExecuteScalar<T>(procedimiento, parametros, commandType: CommandType.StoredProcedure, commandTimeout: TiempoEsperaSegundos);
        }

        /// <summary>Ejecuta sin devolver datos (ACTUALIZAR, CAMBIAR_ESTADO, ANULAR, ELIMINAR).</summary>
        public static void Ejecutar(string procedimiento, object? parametros = null)
        {
            using var cn = Conexion.Crear();
            cn.Execute(procedimiento, parametros, commandType: CommandType.StoredProcedure, commandTimeout: TiempoEsperaSegundos);
        }
    }
}
