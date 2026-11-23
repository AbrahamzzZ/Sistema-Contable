namespace SistemaContable.Presentacion.Ui
{
    /// <summary>Elemento de un ComboBox: guarda un valor y muestra un texto.</summary>
    internal sealed class Opcion<T>
    {
        public T Valor { get; }
        public string Texto { get; }

        public Opcion(T valor, string texto)
        {
            Valor = valor;
            Texto = texto;
        }

        public override string ToString() => Texto;
    }
}
