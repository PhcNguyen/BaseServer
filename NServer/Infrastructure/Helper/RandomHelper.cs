using System.Security.Cryptography;

namespace NServer.Infrastructure.Helper
{
    internal class RandomHelper
    {
        public static byte[] Generate128Bit()
        {
            using var rng = RandomNumberGenerator.Create();
            byte[] key = new byte[128 / 8];
            rng.GetBytes(key);
            return key;
        }

        public static byte[] Generate192Bit()
        {
            using var rng = RandomNumberGenerator.Create();
            byte[] key = new byte[192 / 8];
            rng.GetBytes(key);
            return key;
        }

        public static byte[] Generate256Bit()
        {
            using var rng = RandomNumberGenerator.Create();
            byte[] key = new byte[256 / 8];
            rng.GetBytes(key);
            return key;
        }
    }
}
