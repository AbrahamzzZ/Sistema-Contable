using System.Security.Cryptography;

namespace Utilities
{
    public static class PasswordHasher
    {
        private const string Algoritmo = "PBKDF2-SHA256";
        private const int Iteraciones = 600_000;
        private const int TamanoSalt = 16;  
        private const int TamanoHash = 32;

        /// <summary>Genera el hash para guardar en la BD. Cada llamada produce un resultado distinto.</summary>
        public static string Hashear(string clave)
        {
            if (string.IsNullOrEmpty(clave))
                throw new ArgumentException("La clave no puede estar vacía.", nameof(clave));

            byte[] salt = RandomNumberGenerator.GetBytes(TamanoSalt);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(clave, salt, Iteraciones, HashAlgorithmName.SHA256, TamanoHash);

            return $"{Algoritmo}${Iteraciones}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
        }

        /// <summary>
        /// Compara la clave escrita con el hash guardado.
        /// Devuelve false (no lanza error) si el hash guardado está vacío o mal formado.
        /// </summary>
        public static bool Verificar(string clave, string? hashGuardado)
        {
            if (string.IsNullOrEmpty(clave) || string.IsNullOrWhiteSpace(hashGuardado))
                return false;

            string[] partes = hashGuardado.Split('$');
            if (partes.Length != 4 || partes[0] != Algoritmo)
                return false;

            try
            {
                int iteraciones = int.Parse(partes[1]);
                byte[] salt = Convert.FromBase64String(partes[2]);
                byte[] hashEsperado = Convert.FromBase64String(partes[3]);

                byte[] hashCalculado = Rfc2898DeriveBytes.Pbkdf2(
                    clave, salt, iteraciones, HashAlgorithmName.SHA256, hashEsperado.Length);

                // Comparación en tiempo constante: evita ataques por medición de tiempo
                return CryptographicOperations.FixedTimeEquals(hashCalculado, hashEsperado);
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
