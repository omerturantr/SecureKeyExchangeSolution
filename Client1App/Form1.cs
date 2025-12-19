using Client1App.Security;
using SharedSecurityLib.Crypto;
using SharedSecurityLib.Protocol;
using System;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client1App
{
    public partial class Form1 : Form
    {
        private readonly ClientKeyManager _keys = new ClientKeyManager();

        // Client1'in kendi CERT raw satırı (PEER_CERT ile yollayacağız)
        private string? _myCertRaw;

        // CA protokolünde MessageTypes içinde tanımlı değil (bu projede sabit)
        private const string CaGetPublicKey = "GET_CA_PUBLIC_KEY";
        private const string CaPublicKeyPrefix = "CA_PUBLIC_KEY:";

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // boş
        }

        private async void btnConnectToCA_Click(object sender, EventArgs e)
        {
            try
            {
                // 1) Client RSA key pair üret
                _keys.GenerateClientKeys();

                // 2) CA Public Key al
                string caPubBase64 = await RequestCaPublicKeyAsync();
                _keys.SetCaPublicKey(caPubBase64);

                // 3) CA'den CERT iste
                string req = _keys.BuildReqCertMessage("Client1");
                string certMsg = await RequestCertFromCaAsync(req);

                // 4) CERT set + doğrula
                _keys.SetMyCertificateFromCertMessage(certMsg);
                bool isValid = _keys.VerifyMyCertificate();

                // PEER_CERT için raw sertifika satırını sakla
                _myCertRaw = _keys.BuildMyCertMessageRaw();

                string caPreview = caPubBase64.Length > 60 ? caPubBase64.Substring(0, 60) + "..." : caPubBase64;
                string certPreview = certMsg.Length > 80 ? certMsg.Substring(0, 80) + "..." : certMsg;

                MessageBox.Show(
                    (isValid ? "Sertifika doğrulandı ✅" : "Sertifika doğrulanamadı ❌") +
                    "\n\nCA Key (preview):\n" + caPreview +
                    "\n\nCERT (preview):\n" + certPreview,
                    "Certificate Verification"
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message, "Client1App");
            }
        }

        // -------- CA Helpers --------

        private static async Task<string> RequestCaPublicKeyAsync()
        {
            using TcpClient client = new TcpClient();
            await client.ConnectAsync(ProtocolConstants.Localhost, ProtocolConstants.CaPort);

            using NetworkStream ns = client.GetStream();

            byte[] req = Encoding.UTF8.GetBytes(CaGetPublicKey + "\n");
            await ns.WriteAsync(req, 0, req.Length);

            byte[] buffer = new byte[8192];
            int read = await ns.ReadAsync(buffer, 0, buffer.Length);

            if (read <= 0)
                throw new Exception("CA'dan cevap gelmedi (GET_CA_PUBLIC_KEY).");

            string raw = Encoding.UTF8.GetString(buffer, 0, read).Trim();

            if (raw.StartsWith(CaPublicKeyPrefix, StringComparison.OrdinalIgnoreCase))
                return raw.Substring(CaPublicKeyPrefix.Length).Trim();

            if (raw.StartsWith("ERR:", StringComparison.OrdinalIgnoreCase))
                throw new Exception("CA hata döndü: " + raw);

            throw new Exception("Beklenmeyen CA cevabı: " + raw);
        }

        private static async Task<string> RequestCertFromCaAsync(string reqCertMessage)
        {
            using TcpClient client = new TcpClient();
            await client.ConnectAsync(ProtocolConstants.Localhost, ProtocolConstants.CaPort);

            using NetworkStream ns = client.GetStream();

            byte[] req = Encoding.UTF8.GetBytes(reqCertMessage + "\n");
            await ns.WriteAsync(req, 0, req.Length);

            byte[] buffer = new byte[8192];
            int read = await ns.ReadAsync(buffer, 0, buffer.Length);

            if (read <= 0)
                throw new Exception("CA'dan cevap gelmedi (REQ_CERT).");

            string raw = Encoding.UTF8.GetString(buffer, 0, read).Trim();

            if (raw.StartsWith(MessageTypes.CERT + ":", StringComparison.Ordinal))
                return raw;

            if (raw.StartsWith("ERR:", StringComparison.OrdinalIgnoreCase))
                throw new Exception("CA hata döndü: " + raw);

            throw new Exception("Beklenmeyen CA cevabı: " + raw);
        }

        // -------- Demo: local RSA encrypt/decrypt --------

        private void btnTestSessionKey_Click(object sender, EventArgs e)
        {
            try
            {
                if (!_keys.HasClientKeys)
                    _keys.GenerateClientKeys();

                byte[] ks = _keys.GenerateSessionKey();
                byte[] enc = _keys.EncryptSessionKeyWithClientPublicKey(ks);
                byte[] dec = _keys.DecryptSessionKeyWithClientPrivateKey(enc);

                bool same = CryptographicOperations.FixedTimeEquals(ks, dec);

                MessageBox.Show(
                    "Ks Test Sonucu: " + (same ? "BAŞARILI ✅" : "BAŞARISIZ ❌") +
                    "\n\nKs (Base64): " + Convert.ToBase64String(ks) +
                    "\n\nEncrypted Ks (Base64): " + Convert.ToBase64String(enc),
                    "Session Key (Ks) Demo"
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message);
            }
        }

        // -------- Client2 CERT Request --------

        private static async Task<string> RequestClient2CertAsync(string host = "127.0.0.1", int port = 9100)
        {
            using var tcp = new TcpClient();
            await tcp.ConnectAsync(host, port);

            using var ns = tcp.GetStream();
            using var writer = new StreamWriter(ns, Encoding.UTF8) { AutoFlush = true };
            using var reader = new StreamReader(ns, Encoding.UTF8);

            await writer.WriteLineAsync(MessageTypes.GET_CERT);

            string? raw = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(raw))
                throw new Exception("Client2 CERT cevabı gelmedi.");

            raw = raw.Trim();

            if (raw.StartsWith("ERR:", StringComparison.OrdinalIgnoreCase))
                throw new Exception("Client2 hata döndü: " + raw);

            if (!raw.StartsWith(MessageTypes.CERT + ":", StringComparison.Ordinal))
                throw new Exception("Beklenmeyen CERT mesajı: " + raw);

            return raw;
        }

        // tek satır gönder/tek satır oku helper
        private static async Task<string> SendAndReadLineAsync(string message, string host = "127.0.0.1", int port = 9100)
        {
            using var tcp = new TcpClient();
            await tcp.ConnectAsync(host, port);

            using var ns = tcp.GetStream();
            using var writer = new StreamWriter(ns, Encoding.UTF8) { AutoFlush = true };
            using var reader = new StreamReader(ns, Encoding.UTF8);

            await writer.WriteLineAsync(message);

            string? resp = await reader.ReadLineAsync();
            return resp?.Trim() ?? "";
        }

        // -------- ADIM 9: Km handshake + Ks derivation --------

        private async void btnSendKsToClient2_Click(object sender, EventArgs e)
        {
            try
            {
                if (!_keys.HasCaPublicKey)
                    throw new Exception("CA public key yok. Önce 'Connect to CA' ile sertifikanı doğrula.");

                if (!_keys.HasClientKeys)
                    _keys.GenerateClientKeys();

                if (string.IsNullOrWhiteSpace(_myCertRaw))
                {
                    if (_keys.HasMyCertificate)
                        _myCertRaw = _keys.BuildMyCertMessageRaw();
                    else
                        throw new Exception("Kendi CERT’in yok. Önce Connect to CA yap.");
                }

                // 1) Client2 CERT iste
                string client2CertMsg = await RequestClient2CertAsync();

                // 2) CERT parse: CERT:<clientId>:<pubKeyB64>:<sigB64>
                var parts = ProtocolMessage.Parse(client2CertMsg);
                if (parts.Length < 4)
                    throw new Exception("Client2 CERT formatı hatalı.");

                string clientId = parts[1];
                string client2PubB64 = parts[2];
                string sigB64 = parts[3];

                // 3) CA ile doğrula
                bool ok = _keys.VerifyOtherClientCertificate(clientId, client2PubB64, sigB64);
                if (!ok)
                    throw new Exception("Client2 CERT doğrulanamadı! Devam edilmiyor.");

                // 4) Client2'ye kendi CERT'imi gönder (KM2'yi bana şifreleyebilmesi için)
                string peerCertResp = await SendAndReadLineAsync(MessageTypes.PEER_CERT + ":" + _myCertRaw);
                if (!peerCertResp.StartsWith("OK:", StringComparison.OrdinalIgnoreCase))
                    throw new Exception("Client2, PEER_CERT kabul etmedi: " + peerCertResp);

                // 5) KM1 başlat: N1 üret
                byte[] n1 = new byte[16];
                RandomNumberGenerator.Fill(n1);
                string n1B64 = Convert.ToBase64String(n1);

                // KM1 payload: "N1B64:Client1"
                string km1Payload = $"{n1B64}:Client1";
                byte[] km1Plain = Encoding.UTF8.GetBytes(km1Payload);

                // KM1 -> Client2 public key ile encrypt
                byte[] km1Enc = _keys.EncryptBytesWithOtherPublicKeyBase64(client2PubB64, km1Plain);
                string km1EncB64 = Convert.ToBase64String(km1Enc);

                string km2Resp = await SendAndReadLineAsync(MessageTypes.KM1 + ":" + km1EncB64);
                if (!km2Resp.StartsWith(MessageTypes.KM2 + ":", StringComparison.OrdinalIgnoreCase))
                    throw new Exception("KM2 gelmedi: " + km2Resp);

                // 6) KM2 decrypt (Client1 private key)
                string km2EncB64 = km2Resp.Substring((MessageTypes.KM2 + ":").Length).Trim();
                byte[] km2Enc = Convert.FromBase64String(km2EncB64);

                byte[] km2Plain = _keys.DecryptBytesWithMyPrivateKey(km2Enc);
                string km2Payload = Encoding.UTF8.GetString(km2Plain).Trim();

                // payload: "N1B64:N2B64:Client2"
                var parts2 = km2Payload.Split(':');
                if (parts2.Length < 3)
                    throw new Exception("KM2 payload hatalı.");

                if (!string.Equals(parts2[0], n1B64, StringComparison.Ordinal))
                    throw new Exception("KM2 N1 uyuşmuyor!");

                string n2B64 = parts2[1];
                byte[] n2 = Convert.FromBase64String(n2B64);

                // 7) Km hesapla
                byte[] km = CryptoHelper.DeriveMasterKey(n1, n2, "Client1", "Client2");

                // 8) KM3 gönder: "N2B64" (Client2 public key ile encrypt)
                byte[] km3Plain = Encoding.UTF8.GetBytes(n2B64);
                byte[] km3Enc = _keys.EncryptBytesWithOtherPublicKeyBase64(client2PubB64, km3Plain);
                string km3EncB64 = Convert.ToBase64String(km3Enc);

                string km3Resp = await SendAndReadLineAsync(MessageTypes.KM3 + ":" + km3EncB64);
                if (!km3Resp.StartsWith("OK:", StringComparison.OrdinalIgnoreCase))
                    throw new Exception("KM3 başarısız: " + km3Resp);

                // 9) Ks türet
                byte[] ks = CryptoHelper.DeriveSessionKey(km);

                // 10) SESSION_CONFIRM gönder (Client2 aynı Ks ile doğrulasın)
                byte[] mac = CryptoHelper.BuildSessionConfirmMac(ks);
                string macB64 = Convert.ToBase64String(mac);

                string confirmResp = await SendAndReadLineAsync(MessageTypes.SESSION_CONFIRM + ":" + macB64);
                if (!confirmResp.StartsWith("OK:", StringComparison.OrdinalIgnoreCase))
                    throw new Exception("SESSION_CONFIRM başarısız: " + confirmResp);

                MessageBox.Show(
                    "Adım 9 tamam ✅\nKm üretildi ✅\nKs türetildi ✅\nClient2 doğruladı ✅" +
                    "\n\nKs(Base64): " + Convert.ToBase64String(ks),
                    "Client1"
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message, "Client1");
            }
        }
    }
}
