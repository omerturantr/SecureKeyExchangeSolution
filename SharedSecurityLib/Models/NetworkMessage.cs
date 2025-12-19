namespace SharedSecurityLib.Models
{
    // Socket üzerinden gönderilecek tüm mesajların ortak yapısı
    public class NetworkMessage
    {
        // Mesaj türü (CERT_REQUEST, CERT_RESPONSE, KEY_EXCHANGE, DATA ...)
        public string Type { get; set; } = string.Empty;

        // Mesajı gönderen kim? (CA, Client1, Client2)
        public string From { get; set; } = string.Empty;

        // Mesaj gövdesi (JSON string)
        public string Body { get; set; } = string.Empty;
    }
}
