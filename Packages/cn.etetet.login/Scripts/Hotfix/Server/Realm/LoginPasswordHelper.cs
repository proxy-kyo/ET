using System.Security.Cryptography;
using System.Threading.Tasks;

namespace ET.Server
{
    public static class LoginPasswordHelper
    {
        public static async ETTask<byte[]> Hash(string password, byte[] salt)
        {
            return await Task.Run(() =>
            {
#pragma warning disable SYSLIB0060 // Unity does not provide the newer static Pbkdf2 API.
                using Rfc2898DeriveBytes derive = new(password, salt, 100000, HashAlgorithmName.SHA256);
#pragma warning restore SYSLIB0060
                return derive.GetBytes(32);
            });
        }

        public static async ETTask<bool> Verify(string password, byte[] salt, byte[] expected)
        {
            byte[] actual = await Hash(password, salt);
            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }
    }
}
