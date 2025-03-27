namespace Quanlytaikhoan.Services
{
    public interface IPasswordHasher
    {
        string HassPassword(string password, out byte[] salt);

        bool VerifyPassword(string password, string hash, byte[] salt);
    }
}
