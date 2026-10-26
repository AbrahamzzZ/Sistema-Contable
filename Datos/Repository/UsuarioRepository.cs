using SistemaContable.Entidad;

namespace SistemaContable.Datos.Repository
{
    public class UsuarioRepository
    {
        public Usuario? ObtenerParaLogin(string username) =>
            DbHelper.Obtener<Usuario>("SP_LOGIN", new
            {
                USERNAME = username
            });

        public void RegistrarAcceso(int idUsuario) =>
            DbHelper.Ejecutar("SP_USUARIO_REGISTRAR_ACCESO", new
            {
                ID_USUARIO = idUsuario
            });

        /// <summary>usuario.Clave debe llegar YA hasheada desde Negocio. Devuelve el ID nuevo.</summary>
        public int Insertar(Usuario usuario) =>
            DbHelper.Escalar<int>("SP_REGISTRAR_USUARIO", new
            {
                USERNAME = usuario.Username,
                NOMBRE_COMPLETO = usuario.Nombre_Completo,
                CORREO_ELECTRONICO = usuario.Correo_Electronico,
                CLAVE = usuario.Clave
            });

        public List<Usuario> Listar(bool incluirInactivos = true) =>
            DbHelper.Listar<Usuario>("SP_USUARIO_LISTAR", new
            {
                INCLUIR_INACTIVOS = incluirInactivos
            });

        /// <summary>Actualiza nombre completo y correo (no la clave ni el username).</summary>
        public void Actualizar(Usuario usuario) =>
            DbHelper.Ejecutar("SP_USUARIO_ACTUALIZAR", new
            {
                ID_USUARIO = usuario.Id_Usuario,
                NOMBRE_COMPLETO = usuario.Nombre_Completo,
                CORREO_ELECTRONICO = usuario.Correo_Electronico
            });

        /// <summary>claveHash debe llegar YA hasheada desde Negocio.</summary>
        public void CambiarClave(int idUsuario, string claveHash) =>
            DbHelper.Ejecutar("SP_USUARIO_CAMBIAR_CLAVE", new
            {
                ID_USUARIO = idUsuario,
                CLAVE = claveHash
            });

        /// <summary>Eliminación lógica. idUsuarioSesion = quien ejecuta la acción.</summary>
        public void CambiarEstado(int idUsuario, bool activo, int idUsuarioSesion) =>
            DbHelper.Ejecutar("SP_USUARIO_CAMBIAR_ESTADO", new
            {
                ID_USUARIO = idUsuario,
                ACTIVO = activo,
                ID_USUARIO_SESION = idUsuarioSesion
            });
    }
}
