using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace MediaControlDistributionCenter.Utilities
{
    public static class PasswordGenerator
    {
        private const string Uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string Lowercase = "abcdefghijklmnopqrstuvwxyz";
        private const string Digits = "0123456789";
        private const string SpecialChars = "!@#$%^&*()_+[]{}|;:,.<>?";

        public static string GeneratePassword(int length = 12)
        {
            if (length < 8)
            {
                throw new ArgumentException("Password length should be at least 8 characters.");
            }

            var allChars = Uppercase + Lowercase + Digits + SpecialChars;
            var randomBytes = new byte[length];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(randomBytes);
            }

            var passwordChars = randomBytes.Select(b => allChars[b % allChars.Length]).ToArray();
            return new string(passwordChars);
        }
    }
}
