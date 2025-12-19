using System;
using System.Security.Cryptography;
using System.Text;

namespace SharedSecurityLib.Crypto
{
    public static class RsaSignature
    {
        // data: imzalanacak string (UTF8)
        // privateKeyXml: CA private key (XML) veya senin kullandığın format
        public static string SignToBase64(string data, string privateKeyXml)
        {
            using var rsa = RSA.Create();
            rsa.FromXmlString(privateKeyXml);

            byte[] bytes = Encoding.UTF8.GetBytes(data);
            byte[] sig = rsa.SignData(bytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            return Convert.ToBase64String(sig);
        }

        // publicKeyXml: CA public key (XML)
        public static bool VerifyBase64(string data, string signatureBase64, string publicKeyXml)
        {
            using var rsa = RSA.Create();
            rsa.FromXmlString(publicKeyXml);

            byte[] bytes = Encoding.UTF8.GetBytes(data);
            byte[] sig = Convert.FromBase64String(signatureBase64);

            return rsa.VerifyData(bytes, sig, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }
    }
}
