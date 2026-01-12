using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace VideoStore.Services
{
    public class PasswordHasher<T> : IPasswordHasher<T>
    {
        // PBKDF2 settings
        private const int SaltSize = 16;
        private const int KeySize = 32;
        private const int Iterations = 100_000;

        public string HashPassword(T user, string password)
        {
            using var rng = RandomNumberGenerator.Create();
            var salt = new byte[SaltSize];
            rng.GetBytes(salt);

            var key = KeyDerivation.Pbkdf2(password, salt, KeyDerivationPrf.HMACSHA256, Iterations, KeySize);

            var result = new byte[1 + SaltSize + KeySize];
            result[0] = 0x01; // version
            Buffer.BlockCopy(salt, 0, result, 1, SaltSize);
            Buffer.BlockCopy(key, 0, result, 1 + SaltSize, KeySize);
            return Convert.ToBase64String(result);
        }

        public bool VerifyHashedPassword(T user, string hashedPassword, string providedPassword)
        {
            try
            {
                var bytes = Convert.FromBase64String(hashedPassword);
                if (bytes.Length != 1 + SaltSize + KeySize) return false;
                var salt = new byte[SaltSize];
                Buffer.BlockCopy(bytes, 1, salt, 0, SaltSize);
                var key = KeyDerivation.Pbkdf2(providedPassword, salt, KeyDerivationPrf.HMACSHA256, Iterations, KeySize);
                for (int i = 0; i < KeySize; i++)
                    if (bytes[1 + SaltSize + i] != key[i]) return false;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
