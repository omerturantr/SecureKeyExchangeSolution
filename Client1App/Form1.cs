using Client1App.Security;
using SharedSecurityLib.Protocol;
using System;
using System.IO;
using System.Net;
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

            HookLiveValidation();
            UpdateButtonStates(); // ilk açılışta doğru enabled/disabled
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Katı sistem: otomatik default yazma yok.
            // Kullanıcı girmek zorunda.
        }

        private async void btnConnectToCA_Click(object sender, EventArgs e)
        {
            try
            {
                // 0) ZORUNLU: CA endpoint doğrula
                var ca = GetRequiredEndpoint(txtCaIp, txtCaPort, "CA");

                // 1) Client RSA key pair üret
                _keys.GenerateClientKeys();

                // 2) CA Public Key al
                string caPubBase64 = await RequestCaPublicKeyAsync(ca.host, ca.port);
                _keys.SetCaPublicKey(caPubBase64);

                // 3) CA'den CERT iste
                string req = _keys.BuildReqCertMessage("Client1");
                string certMsg = await RequestCertFromCaAsync(ca.host, ca.port, req);

                // 4) CERT set + doğrula
                _keys.SetMyCertificateFromCertMessage(certMsg);
                bool isValid = _keys.VerifyMyCertificate();

                // PEER_CERT için raw sertifika satırını sakla
                _myCertRaw = _keys.BuildMyCertMessageRaw();

                string caPreview = caPubBase64.Length > 60 ? caPubBase64.Substring(0, 60) + "..." : caPubBase64;
                string certPreview = certMsg.Length > 80 ? certMsg.Substring(0, 80) + "..." : certMsg;

                MessageBox.Show(
                    (isValid ? "Sertifika doğrulandı ✅" : "Sertifika doğrulanamadı ❌") +
                    $"\n\nCA Endpoint:\n{ca.host}:{ca.port}" +
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

        // -------- CA Helpers (UI endpoint ile) --------

        private static async Task<string> RequestCaPublicKeyAsync(string host, int port)
        {
            using TcpClient client = new TcpClient();
            await client.ConnectAsync(host, port);

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

        private static async Task<string> RequestCertFromCaAsync(string host, int port, string reqCertMessage)
        {
            using TcpClient client = new TcpClient();
            await client.ConnectAsync(host, port);

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

        // -------- Client2 iletişimi (UI endpoint ile) --------

        private async void btnSendKsToClient2_Click(object sender, EventArgs e)
        {
            try
            {
                // ZORUNLU: Client2 endpoint
                var c2 = GetRequiredEndpoint(txtClient2Ip, txtClient2Port, "Client2");

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
                string client2CertMsg = await RequestClient2CertAsync(c2.host, c2.port);

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

                // 4) (Opsiyonel ama güvenli) Client2'ye kendi CERT'imi gönder (mutual auth)
                string peerCertResp = await SendAndReadLineAsync(
                    MessageTypes.PEER_CERT + ":" + _myCertRaw,
                    c2.host,
                    c2.port
                );

                if (!peerCertResp.StartsWith("OK:", StringComparison.OrdinalIgnoreCase))
                    throw new Exception("Client2, PEER_CERT kabul etmedi: " + peerCertResp);

                // 5) Ks üret
                byte[] ks = _keys.GenerateSessionKey(32);

                // 6) Ks'i Client2 public key ile şifrele
                byte[] encKs = _keys.EncryptBytesWithOtherPublicKeyBase64(client2PubB64, ks);
                string encKsB64 = Convert.ToBase64String(encKs);

                // 7) SESSION_KEY gönder
                string resp = await SendAndReadLineAsync(
                    MessageTypes.SESSION_KEY + ":" + encKsB64,
                    c2.host,
                    c2.port
                );

                if (!resp.StartsWith("OK:", StringComparison.OrdinalIgnoreCase))
                    throw new Exception("SESSION_KEY başarısız / reddedildi: " + resp);

                MessageBox.Show(
                    "SESSION_KEY gönderildi ✅" +
                    "\nClient2 kabul etti ✅" +
                    "\n\nKs(Base64): " + Convert.ToBase64String(ks),
                    "Client1"
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message, "Client1");
            }
        }

        private static async Task<string> RequestClient2CertAsync(string host, int port)
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

        private static async Task<string> SendAndReadLineAsync(string message, string host, int port)
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

        // -------- Strict Validation Helpers --------

        private static (string host, int port) GetRequiredEndpoint(TextBox txtIp, TextBox txtPort, string label)
        {
            string ipRaw = (txtIp.Text ?? "").Trim();
            string portRaw = (txtPort.Text ?? "").Trim();

            if (string.IsNullOrWhiteSpace(ipRaw))
            {
                txtIp.Focus();
                throw new Exception($"{label} IP zorunlu. Lütfen IP adresi gir.");
            }

            if (!IPAddress.TryParse(ipRaw, out _))
            {
                txtIp.Focus();
                throw new Exception($"{label} IP formatı hatalı: '{ipRaw}'");
            }

            if (string.IsNullOrWhiteSpace(portRaw))
            {
                txtPort.Focus();
                throw new Exception($"{label} Port zorunlu. Lütfen port gir.");
            }

            if (!int.TryParse(portRaw, out int port) || port < 1 || port > 65535)
            {
                txtPort.Focus();
                throw new Exception($"{label} Port geçersiz: '{portRaw}' (1-65535 arası olmalı)");
            }

            return (ipRaw, port);
        }

        private void HookLiveValidation()
        {
            txtCaIp.TextChanged += (_, __) => UpdateButtonStates();
            txtCaPort.TextChanged += (_, __) => UpdateButtonStates();
            txtClient2Ip.TextChanged += (_, __) => UpdateButtonStates();
            txtClient2Port.TextChanged += (_, __) => UpdateButtonStates();
        }

        private void UpdateButtonStates()
        {
            bool caOk = IsValidIPv4(txtCaIp.Text) && IsValidPort(txtCaPort.Text);
            bool c2Ok = IsValidIPv4(txtClient2Ip.Text) && IsValidPort(txtClient2Port.Text);

            btnConnectToCA.Enabled = caOk;

            // "Send Ks" için hem CA hem Client2 endpoint gerekli
            btnSendKsToClient2.Enabled = caOk && c2Ok;
        }

        private static bool IsValidIPv4(string? ip)
        {
            ip = (ip ?? "").Trim();
            return IPAddress.TryParse(ip, out var addr) && addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork;
        }

        private static bool IsValidPort(string? portText)
        {
            portText = (portText ?? "").Trim();
            return int.TryParse(portText, out int p) && p >= 1 && p <= 65535;
        }
    }
}
