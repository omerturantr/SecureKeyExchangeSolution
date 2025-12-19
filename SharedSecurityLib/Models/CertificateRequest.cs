namespace SharedSecurityLib.Models
{
    // Client -> CA: Sertifika talebi modeli
    public class CertificateRequest
    {
        // İstemcinin kimliği (Client1, Client2)
        public string SubjectId { get; set; } = string.Empty;

        // İstemcinin public key'i (Base64 formatında)
        public string PublicKey { get; set; } = string.Empty;

        // Kullanılan algoritma (ör: RSA)
        public string AlgorithmId { get; set; } = "RSA";
    }
}
