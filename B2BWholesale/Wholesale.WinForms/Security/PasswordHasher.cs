using System;
using System.Security.Cryptography;

namespace Wholesale.WinForms.Security
{
    /// <summary>
    /// PBKDF2-HMAC-SHA256 password hashing. Stored format:  PBKDF2$iterations$saltBase64$hashBase64
    /// The same class can be copied into the ASP.NET Core project so both applications accept the same Users table.
    /// </summary>
    public static class PasswordHasher
    {
        private const int SaltSize = 16;
        private const int KeySize = 32;
        public const int DefaultIterations = 100000;

        public static string Hash(string password)
        {
            return Hash(password, DefaultIterations);
        }

        public static string Hash(string password, int iterations)
        {
            var salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create()) rng.GetBytes(salt);
            byte[] key = Derive(password, salt, iterations);
            return string.Format("PBKDF2${0}${1}${2}", iterations, Convert.ToBase64String(salt), Convert.ToBase64String(key));
        }

        public static bool Verify(string password, string stored)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(stored)) return false;
            string[] parts = stored.Split('$');
            if (parts.Length != 4 || parts[0] != "PBKDF2") return false;
            int iterations;
            if (!int.TryParse(parts[1], out iterations) || iterations < 1) return false;
            byte[] salt, expected;
            try
            {
                salt = Convert.FromBase64String(parts[2]);
                expected = Convert.FromBase64String(parts[3]);
            }
            catch (FormatException) { return false; }
            byte[] actual = Derive(password, salt, iterations);
            return ConstantTimeEquals(actual, expected);
        }

        private static byte[] Derive(string password, byte[] salt, int iterations)
        {
            using (var kdf = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
                return kdf.GetBytes(KeySize);
        }

        private static bool ConstantTimeEquals(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;
            int diff = 0;
            for (int i = 0; i < a.Length; i++) diff |= a[i] ^ b[i];
            return diff == 0;
        }
    }
}
