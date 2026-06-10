using System;
using System.Security.Cryptography;
using System.Text;

namespace AgroRegionApp.Data
{
    internal static class PasswordHasher
    {
        public static string Hash(string password)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                var builder = new StringBuilder(bytes.Length * 2);
                foreach (var b in bytes)
                    builder.Append(b.ToString("x2"));
                return builder.ToString();
            }
        }

        public static bool Verify(string password, string storedHash)
        {
            if (string.IsNullOrEmpty(storedHash))
                return false;

            if (storedHash == password)
                return true;

            return string.Equals(Hash(password), storedHash, StringComparison.OrdinalIgnoreCase);
        }
    }
}
