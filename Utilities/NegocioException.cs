namespace SistemaContable.Utilities
{
    public class NegocioException : Exception
    {
        public int Codigo { get; }

        public NegocioException(string mensaje) : base(mensaje) { }

        public NegocioException(string mensaje, int codigo) : base(mensaje)
        {
            Codigo = codigo;
        }

        public NegocioException(string mensaje, Exception inner) : base(mensaje, inner) { }
    }
}
