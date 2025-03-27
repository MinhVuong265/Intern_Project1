using System.Security.Cryptography;
using System.Text;

namespace Quanlytaikhoan.Services
{
    public class PasswordHandler : IPasswordHasher
    {
        private const int keysize = 16;
        private const int interations = 100;
        private readonly HashAlgorithmName hashAlgorithm = HashAlgorithmName.SHA512;
        string IPasswordHasher.HassPassword(string password, out byte[] salt)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentNullException(nameof(password), "Mật khẩu không được để trống");
            }
            salt = RandomNumberGenerator.GetBytes(keysize);

            var hash = Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(password),salt,interations,hashAlgorithm,keysize);

            return Convert.ToHexString(hash);
        }

        bool IPasswordHasher.VerifyPassword(string password, string hash, byte[] salt)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentNullException(nameof(password), "Mật khẩu không được để trống");
            }
                var hashToCompare = Rfc2898DeriveBytes.Pbkdf2(password, salt, interations, hashAlgorithm, keysize);

            return CryptographicOperations.FixedTimeEquals(hashToCompare, Convert.FromHexString(hash));
        }
    }
}
