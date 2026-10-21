using SistemaContable.Datos.Repository;
using SistemaContable.Entidad;
using SistemaContable.Utilities;

namespace SistemaContable.Negocio
{
    public class CategoriaService
    {
        private const int LongitudMaximaNombre = 100;

        private readonly CategoriaRepository _repo = new();

        /// <summary>Primer ComboBox del registro: grupos activos de ingreso ("I") o egreso ("E").</summary>
        public List<Categoria> ListarGrupos(string tipo)
        {
            ValidarTipo(tipo);
            return ManejadorErrores.Ejecutar(() => _repo.ListarGrupos(tipo));
        }

        /// <summary>Segundo ComboBox del registro: categorías activas del grupo.</summary>
        public List<Categoria> ListarPorGrupo(int idGrupo)
        {
            if (idGrupo <= 0)
                return new List<Categoria>();

            return ManejadorErrores.Ejecutar(() => _repo.ListarPorGrupo(idGrupo));
        }

        /// <summary>Pantalla de administración. tipo = null trae ingresos y egresos.</summary>
        public List<Categoria> Listar(string? tipo = null, bool incluirInactivos = true)
        {
            if (tipo is not null)
                ValidarTipo(tipo);

            return ManejadorErrores.Ejecutar(() => _repo.Listar(tipo, incluirInactivos));
        }

        public Categoria Obtener(int idCategoria)
        {
            if (idCategoria <= 0)
                throw new NegocioException("Seleccione una categoría.");

            return ManejadorErrores.Ejecutar(() => _repo.Obtener(idCategoria))
                   ?? throw new NegocioException("La categoría no existe.");
        }

        /// <summary>Crea un grupo (ej. MISIONES). Devuelve el ID.</summary>
        public int CrearGrupo(string nombre, string tipo, int orden = 0)
        {
            ValidarTipo(tipo);
            ValidarNombre(nombre);
            ValidarOrden(orden);

            var grupo = new Categoria
            {
                Nombre = nombre.Trim(),
                Tipo = tipo,
                Id_Padre = null,
                Orden = orden
            };

            return ManejadorErrores.Ejecutar(() => _repo.Insertar(grupo));
        }

        /// <summary>Crea una categoría dentro de un grupo. El tipo se hereda del grupo. Devuelve el ID.</summary>
        public int CrearCategoria(int idGrupo, string nombre, int orden = 0)
        {
            if (idGrupo <= 0)
                throw new NegocioException("Seleccione el grupo al que pertenece la categoría.");

            ValidarNombre(nombre);
            ValidarOrden(orden);

            var categoria = new Categoria
            {
                Nombre = nombre.Trim(),
                Id_Padre = idGrupo,
                Orden = orden
            };

            return ManejadorErrores.Ejecutar(() => _repo.Insertar(categoria));
        }

        /// <summary>Solo cambia nombre y orden. El tipo y el grupo no se pueden cambiar.</summary>
        public void Actualizar(int idCategoria, string nombre, int orden)
        {
            if (idCategoria <= 0)
                throw new NegocioException("Seleccione una categoría.");

            ValidarNombre(nombre);
            ValidarOrden(orden);

            var categoria = new Categoria
            {
                Id_Categoria = idCategoria,
                Nombre = nombre.Trim(),
                Orden = orden
            };

            ManejadorErrores.Ejecutar(() => _repo.Actualizar(categoria));
        }

        /// <summary>Activa o desactiva. Desactivar un grupo desactiva también sus categorías.</summary>
        public void CambiarEstado(int idCategoria, bool activo)
        {
            if (idCategoria <= 0)
                throw new NegocioException("Seleccione una categoría.");

            ManejadorErrores.Ejecutar(() => _repo.CambiarEstado(idCategoria, activo));
        }

        /// <summary>Elimina solo si no tiene movimientos ni categorías hijas (lo valida el procedimiento).</summary>
        public void Eliminar(int idCategoria)
        {
            if (idCategoria <= 0)
                throw new NegocioException("Seleccione una categoría.");

            ManejadorErrores.Ejecutar(() => _repo.Eliminar(idCategoria));
        }

        private static void ValidarTipo(string? tipo)
        {
            if (!TipoMovimiento.EsValido(tipo))
                throw new NegocioException("Indique si es de ingreso o de egreso.");
        }

        private static void ValidarNombre(string? nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new NegocioException("El nombre es obligatorio.");

            if (nombre.Trim().Length > LongitudMaximaNombre)
                throw new NegocioException($"El nombre no puede superar {LongitudMaximaNombre} caracteres.");
        }

        private static void ValidarOrden(int orden)
        {
            if (orden < 0)
                throw new NegocioException("El orden no puede ser negativo.");
        }
    }
}
