using System;

namespace SharedSecurityLib.Models
{
    public static class CertificatePayload
    {
        // (Mevcut) CA'nın imzalayacağı veri (clientId + pubKey)
        // Aynı string hem CA hem Client tarafında birebir üretilmeli
        public static string BuildToSign(string clientId, string clientPublicKeyBase64)
            => $"{clientId}:{clientPublicKeyBase64}";

        // (Yeni) oversimplified X.509 imza payload'ı
        public static string BuildToSign(SimpleCertificate cert)
        {
            // Mevcut CryptoHelper ile aynı alan sırası + delimiter standardı (|)
            return
                cert.SubjectId + "|" +
                cert.AlgorithmId + "|" +
                cert.PublicKey + "|" +
                cert.NotBefore.ToUniversalTime().ToString("o") + "|" +
                cert.NotAfter.ToUniversalTime().ToString("o") + "|" +
                cert.SerialNumber;
        }
    }
}
