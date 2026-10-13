namespace SistemaContable.Entidad
{
    public class Usuario
    {
        public int Id_Usuario { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Nombre_Completo { get; set; } = string.Empty;
        public string? Correo_Electronico { get; set; }
        public string Clave { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;
        public DateTime? Ultimo_Acceso { get; set; }
        public DateTime? Fecha_Crecion { get; set; }
        public string Estado => Activo ? "Activo" : "Inactivo";
    }
}
