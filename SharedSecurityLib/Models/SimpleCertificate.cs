using System;

namespace SharedSecurityLib.Models
{
    // Basit bir sertifika modeli (oversimplified X.509 concept)
    public class SimpleCertificate
    {
        // Sertifikanın sahibi (Client1, Client2)
        public string SubjectId { get; set; } = string.Empty;

        // Kullanılan algoritma (RSA)
        public string AlgorithmId { get; set; } = "RSA";

        // İstemcinin public key'i (Base64)
        public string PublicKey { get; set; } = string.Empty;

        // Sertifika geçerlilik tarihleri
        public DateTime NotBefore { get; set; }
        public DateTime NotAfter { get; set; }

        // Sertifika seri numarası
        public string SerialNumber { get; set; } = string.Empty;

        // CA tarafından atılmış dijital imza (Base64)
        public string SignatureBase64 { get; set; } = string.Empty;
    }
}
