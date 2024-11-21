using System.Buffers;
using System.Security.Cryptography;

namespace NServer.Core.Security
{
    internal class AesCipher
    {
        private readonly byte[] K;

        public AesCipher(byte[] k)
        {
            int ks = k.Length * 8;
            if (ks != 128 && ks != 192 && ks != 256)
            {
                throw new ArgumentException("The provided key length must be 128, 192, or 256 bits.");
            }
            K = k;
        }

        private static void I(byte[] c)
        {
            for (int i = c.Length - 1; i >= 0; i--)
            {
                if (++c[i] != 0) break;
            }
        }

        private static Aes C(byte[] k)
        {
            var aes = Aes.Create();
            aes.Key = k;
            aes.Mode = CipherMode.ECB;
            return aes;
        }

        public async ValueTask<byte[]> E(byte[] p)
        {
            using var aes = C(K);
            byte[] v = new byte[16];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(v);
            }

            using var ms = new MemoryStream();
            await ms.WriteAsync(v);

            byte[] c = new byte[16];
            Array.Copy(v, c, v.Length);
            using var encryptor = aes.CreateEncryptor();

            byte[] ec = ArrayPool<byte>.Shared.Rent(16);

            for (int i = 0; i < p.Length; i += aes.BlockSize / 8)
            {
                encryptor.TransformBlock(c, 0, c.Length, ec, 0);

                int bte = Math.Min(p.Length - i, aes.BlockSize / 8);
                byte[] blk = new byte[bte];
                Array.Copy(p, i, blk, 0, bte);

                for (int j = 0; j < bte; j++)
                    blk[j] ^= ec[j];

                await ms.WriteAsync(blk.AsMemory(0, bte));
                I(c);
            }

            ArrayPool<byte>.Shared.Return(ec);
            return ms.ToArray();
        }

        public async ValueTask<byte[]> D(byte[] c)
        {
            using var aes = C(K);
            byte[] v = new byte[16];
            Array.Copy(c, 0, v, 0, v.Length);

            using var ms = new MemoryStream(c, v.Length, c.Length - v.Length);
            using var encryptor = aes.CreateEncryptor();

            byte[] counter = new byte[16];
            Array.Copy(v, counter, v.Length);

            using var rs = new MemoryStream();
            byte[] buf = new byte[16];
            int br;
            byte[] ec = ArrayPool<byte>.Shared.Rent(16);

            while ((br = await ms.ReadAsync(buf)) > 0)
            {
                encryptor.TransformBlock(counter, 0, counter.Length, ec, 0);

                for (int j = 0; j < br; j++)
                    buf[j] ^= ec[j];

                await rs.WriteAsync(buf.AsMemory(0, br));
                I(counter);
            }

            ArrayPool<byte>.Shared.Return(ec);
            return rs.ToArray();
        }
    }
}