using System.Security.Cryptography;

namespace NServer.Core.Security
{
    internal class RsaCipher
    {
        private readonly RSA r = RSA.Create();

        public RSAParameters Pk { get; private set; }
        public RSAParameters Pr { get; private set; }

        public RsaCipher()
        {
            r.KeySize = 2048;
            Pk = r.ExportParameters(false);
            Pr = r.ExportParameters(true);
        }

        public static byte[] E(byte[] d, RSAParameters pk)
        {
            using var rsaE = RSA.Create();
            rsaE.ImportParameters(pk);
            return rsaE.Encrypt(d, RSAEncryptionPadding.Pkcs1);
        }

        public static byte[] D(byte[] d, RSAParameters pr)
        {
            using var rsaD = RSA.Create();
            rsaD.ImportParameters(pr);
            return rsaD.Decrypt(d, RSAEncryptionPadding.Pkcs1);
        }
    }
}
