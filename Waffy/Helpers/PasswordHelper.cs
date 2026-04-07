using System.Security.Cryptography;
using System.Text;

namespace Waffy.Helpers
{
    public static class PasswordHelper
    {
        // Converts plain text password into a SHA-256 hash
        // This hash is what gets stored and compared in the DB
        public static string Hash(string password)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}