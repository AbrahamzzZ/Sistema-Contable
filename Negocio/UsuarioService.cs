using SistemaContable.Datos.Repository;
using SistemaContable.Entidad;
using SistemaContable.Utilities;
using Utilities;

namespace SistemaContable.Negocio
{
    public class UsuarioService
    {
        private const string MensajeLoginFallido = "Usuario o clave incorrectos.";
        private static readonly Lazy<string> HashRelleno = new(() => PasswordHasher.Hashear("relleno-no-valido-1"));

        private readonly UsuarioRepository _repo = new();

        /// <summary>Valida las credenciales e inicia la sesión. Devuelve el usuario sin la clave.</summary>
        public Usuario IniciarSesion(string username, string clave)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(clave))
                throw new NegocioException("Ingrese el usuario y la clave.");

            Usuario? usuario = ManejadorErrores.Ejecutar(() => _repo.ObtenerParaLogin(username.Trim()));

            bool claveCorrecta = PasswordHasher.Verificar(clave, usuario?.Clave ?? HashRelleno.Value);

            if (usuario is null || !claveCorrecta || !usuario.Activo)
                throw new NegocioException(MensajeLoginFallido);

            ManejadorErrores.Ejecutar(() => _repo.RegistrarAcceso(usuario.Id_Usuario));
            Sesion.Iniciar(usuario.Id_Usuario, usuario.Username, usuario.Nombre_Completo);

            usuario.Clave = string.Empty;   // el hash no sale de esta capa
            return usuario;
        }

        public void CerrarSesion() => Sesion.Cerrar();


        /// <summary>Crea un usuario. Recibe la clave en texto plano y la guarda hasheada. Devuelve el ID.</summary>
        public int Registrar(Usuario usuario, string clave, string confirmacionClave)
        {
            ValidarDatos(usuario);

            string? error = Validaciones.ValidarUsername(usuario.Username);
            if (error is not null) throw new NegocioException(error);

            ValidarClaveNueva(clave, confirmacionClave);

            var nuevo = new Usuario
            {
                Username = usuario.Username.Trim(),
                Nombre_Completo = usuario.Nombre_Completo.Trim(),
                Correo_Electronico = LimpiarCorreo(usuario.Correo_Electronico),
                Clave = PasswordHasher.Hashear(clave)
            };

            return ManejadorErrores.Ejecutar(() => _repo.Insertar(nuevo));
        }

        public List<Usuario> Listar(bool incluirInactivos = true) =>
            ManejadorErrores.Ejecutar(() => _repo.Listar(incluirInactivos));

        /// <summary>Actualiza nombre completo y correo.</summary>
        public void Actualizar(Usuario usuario)
        {
            if (usuario.Id_Usuario <= 0)
                throw new NegocioException("Seleccione un usuario.");

            ValidarDatos(usuario);

            var datos = new Usuario
            {
                Id_Usuario = usuario.Id_Usuario,
                Nombre_Completo = usuario.Nombre_Completo.Trim(),
                Correo_Electronico = LimpiarCorreo(usuario.Correo_Electronico)
            };

            ManejadorErrores.Ejecutar(() => _repo.Actualizar(datos));

            // Si el usuario se editó a sí mismo, refrescar el nombre que muestra la pantalla
            if (datos.Id_Usuario == Sesion.IdUsuario)
                Sesion.ActualizarNombre(datos.Nombre_Completo);
        }

        /// <summary>El usuario en sesión cambia su propia clave: exige la clave actual.</summary>
        public void CambiarMiClave(string claveActual, string claveNueva, string confirmacionClave)
        {
            int idUsuario = Sesion.ObtenerIdUsuarioActual();

            if (string.IsNullOrEmpty(claveActual))
                throw new NegocioException("Ingrese su clave actual.");

            Usuario? usuario = ManejadorErrores.Ejecutar(() => _repo.ObtenerParaLogin(Sesion.Username));

            if (usuario is null || !PasswordHasher.Verificar(claveActual, usuario.Clave))
                throw new NegocioException("La clave actual no es correcta.");

            if (claveActual == claveNueva)
                throw new NegocioException("La clave nueva debe ser diferente de la actual.");

            ValidarClaveNueva(claveNueva, confirmacionClave);

            string hash = PasswordHasher.Hashear(claveNueva);
            ManejadorErrores.Ejecutar(() => _repo.CambiarClave(idUsuario, hash));
        }

        /// <summary>
        /// Un usuario restablece la clave de OTRO usuario que la olvidó (no pide la clave actual).
        /// Para la clave propia se debe usar CambiarMiClave.
        /// </summary>
        public void RestablecerClave(int idUsuario, string claveNueva, string confirmacionClave)
        {
            Sesion.ObtenerIdUsuarioActual();

            if (idUsuario <= 0)
                throw new NegocioException("Seleccione un usuario.");

            if (idUsuario == Sesion.IdUsuario)
                throw new NegocioException("Para cambiar su propia clave use la opción \"Cambiar mi clave\".");

            ValidarClaveNueva(claveNueva, confirmacionClave);

            string hash = PasswordHasher.Hashear(claveNueva);
            ManejadorErrores.Ejecutar(() => _repo.CambiarClave(idUsuario, hash));
        }

        public void CambiarEstado(int idUsuario, bool activo)
        {
            int idSesion = Sesion.ObtenerIdUsuarioActual();

            if (idUsuario <= 0)
                throw new NegocioException("Seleccione un usuario.");

            // El procedimiento también valida: no desactivarse a sí mismo ni al último activo
            ManejadorErrores.Ejecutar(() => _repo.CambiarEstado(idUsuario, activo, idSesion));
        }

        private static void ValidarDatos(Usuario usuario)
        {
            if (string.IsNullOrWhiteSpace(usuario.Nombre_Completo))
                throw new NegocioException("El nombre completo es obligatorio.");

            if (usuario.Nombre_Completo.Trim().Length > 100)
                throw new NegocioException("El nombre completo no puede superar 100 caracteres.");

            if (!string.IsNullOrWhiteSpace(usuario.Correo_Electronico))
            {
                if (usuario.Correo_Electronico.Trim().Length > 150)
                    throw new NegocioException("El correo no puede superar 150 caracteres.");

                if (!Validaciones.EsCorreoValido(usuario.Correo_Electronico))
                    throw new NegocioException("El correo electrónico no tiene un formato válido.");
            }
        }

        private static void ValidarClaveNueva(string clave, string confirmacion)
        {
            string? error = Validaciones.ValidarClave(clave);
            if (error is not null) throw new NegocioException(error);

            if (clave != confirmacion)
                throw new NegocioException("La clave y su confirmación no coinciden.");
        }

        private static string? LimpiarCorreo(string? correo) =>
            string.IsNullOrWhiteSpace(correo) ? null : correo.Trim().ToLowerInvariant();
    }
}
