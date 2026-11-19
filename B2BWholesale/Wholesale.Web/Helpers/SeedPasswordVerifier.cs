
using System.Security.Cryptography;

namespace Wholesale.Web.Helpers
{
    public static class SeedPasswordVerifier
    {
        public static bool Verify(string password, string storedHash)
        {
            if (string.IsNullOrEmpty(password) ||
                string.IsNullOrEmpty(storedHash))
            {
                return false;
            }

            string[] parts = storedHash.Split('$');

            if (parts.Length != 4 || parts[0] != "PBKDF2")
                return false;

            if (!int.TryParse(parts[1], out int iterations))
                return false;

            if (iterations < 10000 || iterations > 1000000)
                return false;

            try
            {
                byte[] salt = Convert.FromBase64String(parts[2]);
                byte[] expected = Convert.FromBase64String(parts[3]);

                if (salt.Length != 16 || expected.Length != 32)
                    return false;

                byte[] actual = Rfc2898DeriveBytes.Pbkdf2(
                    password,
                    salt,
                    iterations,
                    HashAlgorithmName.SHA256,
                    expected.Length
                );

                return CryptographicOperations.FixedTimeEquals(
                    actual,
                    expected
                );
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
