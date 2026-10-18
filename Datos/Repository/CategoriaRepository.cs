using SistemaContable.Entidad;

namespace SistemaContable.Datos.Repository
{
    public class CategoriaRepository
    {
        /// <summary>Primer ComboBox: grupos activos de un tipo ("I" o "E").</summary>
        public List<Categoria> ListarGrupos(string tipo) =>
            DbHelper.Listar<Categoria>("SP_CATEGORIA_LISTAR_GRUPOS", new
            {
                TIPO = tipo
            });

        /// <summary>Segundo ComboBox: categorías activas de un grupo.</summary>
        public List<Categoria> ListarPorGrupo(int idGrupo) =>
            DbHelper.Listar<Categoria>("SP_CATEGORIA_LISTAR_POR_GRUPO", new
            {
                ID_PADRE = idGrupo
            });

        /// <summary>Pantalla de administración. tipo = null trae ingresos y egresos.</summary>
        public List<Categoria> Listar(string? tipo = null, bool incluirInactivos = true) =>
            DbHelper.Listar<Categoria>("SP_CATEGORIA_LISTAR", new
            {
                TIPO = tipo,
                INCLUIR_INACTIVOS = incluirInactivos
            });

        public Categoria? Obtener(int idCategoria) =>
            DbHelper.Obtener<Categoria>("SP_CATEGORIA_OBTENER", new
            {
                ID_CATEGORIA = idCategoria
            });

        /// <summary>
        /// Grupo: IdPadre = null y Tipo obligatorio.
        /// Categoría: IdPadre = grupo (el tipo se hereda del grupo).
        /// Orden = 0 deja que el procedimiento la ubique al final. Devuelve el ID nuevo.
        /// </summary>
        public int Insertar(Categoria categoria) =>
            DbHelper.Escalar<int>("SP_CATEGORIA_INSERTAR", new
            {
                NOMBRE = categoria.Nombre,
                TIPO = categoria.Tipo,
                ID_PADRE = categoria.Id_Padre,
                ORDEN = categoria.Orden > 0 ? categoria.Orden : (int?)null
            });

        /// <summary>Solo nombre y orden. El tipo y el grupo no se cambian.</summary>
        public void Actualizar(Categoria categoria) =>
            DbHelper.Ejecutar("SP_CATEGORIA_ACTUALIZAR", new
            {
                ID_CATEGORIA = categoria.Id_Categoria,
                NOMBRE = categoria.Nombre,
                ORDEN = categoria.Orden
            });

        /// <summary>Eliminación lógica. Desactivar un grupo desactiva también sus categorías.</summary>
        public void CambiarEstado(int idCategoria, bool activo) =>
            DbHelper.Ejecutar("SP_CATEGORIA_CAMBIAR_ESTADO", new
            {
                ID_CATEGORIA = idCategoria,
                ACTIVO = activo
            });

        /// <summary>Eliminación física: solo si no tiene movimientos ni categorías hijas.</summary>
        public void Eliminar(int idCategoria) =>
            DbHelper.Ejecutar("SP_CATEGORIA_ELIMINAR", new
            {
                ID_CATEGORIA = idCategoria
            });
    }
}
