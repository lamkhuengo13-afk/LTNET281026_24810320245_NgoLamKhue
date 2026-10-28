using System.Security.Cryptography;
namespace BaiTap41_Login;
internal static class PasswordSecurity
{
    internal static bool Verify(string password, byte[] hash, byte[] salt)
    {
        byte[] actual=Rfc2898DeriveBytes.Pbkdf2(password,salt,120000,HashAlgorithmName.SHA256,32);
        return CryptographicOperations.FixedTimeEquals(actual,hash);
    }
}
