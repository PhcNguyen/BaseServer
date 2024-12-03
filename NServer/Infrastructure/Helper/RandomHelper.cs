using System.Security.Cryptography;

namespace NServer.Infrastructure.Helper
{
    public static class RandomHelper
    {
        public static byte[] GenerateKey128() => GenerateKey(128);

        public static byte[] GenerateKey192() => GenerateKey(192);

        public static byte[] GenerateKey256() => GenerateKey(256);
        

        // General method to generate keys of any size
        private static byte[] GenerateKey(int bitSize)
        {
            using var rng = RandomNumberGenerator.Create();
            byte[] key = new byte[bitSize / 8];
            rng.GetBytes(key);
            return key;
        }
    }
}