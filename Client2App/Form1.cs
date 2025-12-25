using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Client2App.Security;
using SharedSecurityLib.Protocol;

namespace Client2App
{
    public partial class Form1 : Form
    {
        private readonly ClientKeyManager _keys = new ClientKeyManager();

        private TcpListener? _listener;
        private bool _listening = false;

        // CA'den aldığımız CERT satırı (GET_CERT ile döneceğiz)
        private string? _myCertRaw;

        // Peer (Client1) public key (PEER_CERT doğrulandıktan sonra set)
        private string? _client1PubKeyBase64FromCert;

        // SESSION KEY (Ks)
        private byte[]? _ks;

        // CA protokolünde MessageTypes içinde tanımlı değil (bu projede sabit)
        private const string CaGetPublicKey = "GET_CA_PUBLIC_KEY";
        private const string CaPublicKeyPrefix = "CA_PUBLIC_KEY:";

        public Form1()
        {
            InitializeComponent();
            HookLiveValidation();
            UpdateButtonStates();
        }

        private void Log(string msg)
        {
            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke(new Action(() => Log(msg)));
                return;
            }

            txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {msg}{Environment.NewLine}");
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

        // -------- Buttons --------

        private void btnTestDecryptKs_Click(object sender, EventArgs e)
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
                    "Client2 Decrypt Test: " + (same ? "BAŞARILI ✅" : "BAŞARISIZ ❌") +
                    "\n\nKs (Base64): " + Convert.ToBase64String(ks) +
                    "\n\nEncrypted Ks (Base64): " + Convert.ToBase64String(enc),
                    "Client2 Ks Decrypt Demo"
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message, "Client2App");
            }
        }

        // Connect to CA (buton)
        private async void btnConnectToCA_Click(object sender, EventArgs e)
        {
            try
            {
                // ZORUNLU: CA endpoint
                var ca = GetRequiredEndpoint(txtCaIp, txtCaPort, "CA");

                if (!_keys.HasClientKeys)
                    _keys.GenerateClientKeys();

                Log("Client2 Public Key (Base64): " + _keys.GetClientPublicKeyBase64());

                string caPubBase64 = await RequestCaPublicKeyAsync(ca.host, ca.port);
                _keys.SetCaPublicKey(caPubBase64);
                Log($"CA Public Key alındı ✅ ({ca.host}:{ca.port})");

                string req = _keys.BuildReqCertMessage("Client2");
                Log("CA'ye REQ_CERT gönderiliyor...");

                string certMsg = await RequestCertFromCaAsync(ca.host, ca.port, req);

                _myCertRaw = certMsg;
                _keys.SetMyCertificateFromCertMessage(certMsg);
                Log("CERT alındı ✅");

                bool ok = _keys.VerifyMyCertificate();
                Log(ok ? "CERT doğrulandı ✅" : "CERT doğrulama başarısız ❌");

                MessageBox.Show(ok ? "Client2: Sertifika doğrulandı ✅" : "Client2: Sertifika doğrulanamadı ❌", "Client2");
            }
            catch (Exception ex)
            {
                Log("CA bağlantı / CERT hata: " + ex.Message);
                MessageBox.Show("Hata: " + ex.Message, "Client2App");
            }
        }

        // Listener (UI Listen Port)
        private async void btnStartListener_Click(object sender, EventArgs e)
        {
            try
            {
                if (_listening)
                {
                    Log("Listener zaten çalışıyor.");
                    return;
                }

                // ZORUNLU: Listen Port
                int listenPort = GetRequiredPort(txtListenPort, "Listen");

                if (!_keys.HasClientKeys)
                    _keys.GenerateClientKeys();

                Log("Client2 Public Key (Base64): " + _keys.GetClientPublicKeyBase64());

                // IPAddress.Any -> başka makineden de bağlantı alabilsin
                _listener = new TcpListener(IPAddress.Any, listenPort);
                _listener.Start();
                _listening = true;

                Log($"Client2 Listener başladı: 0.0.0.0:{listenPort}");
                Log("Beklenen formatlar:");
                Log($"  - {MessageTypes.GET_CERT}");
                Log($"  - {MessageTypes.PEER_CERT}:<CERT:.>");
                Log($"  - {MessageTypes.SESSION_KEY}:<base64EncryptedKs>");

                while (_listening)
                {
                    TcpClient client = await _listener.AcceptTcpClientAsync();
                    _ = HandleClientAsync(client);
                }
            }
            catch (Exception ex)
            {
                Log("Listener hata: " + ex.Message);
            }
        }

        private async Task HandleClientAsync(TcpClient client)
        {
            using (client)
            {
                try
                {
                    Log("Bağlantı alındı.");

                    using var ns = client.GetStream();
                    using var reader = new StreamReader(ns, Encoding.UTF8);
                    using var writer = new StreamWriter(ns, Encoding.UTF8) { AutoFlush = true };

                    string? raw = await reader.ReadLineAsync();
                    if (string.IsNullOrWhiteSpace(raw))
                    {
                        Log("Boş veri geldi (ReadLine null/empty).");
                        return;
                    }

                    raw = raw.Trim();
                    Log("Gelen: " + (raw.Length > 140 ? raw.Substring(0, 140) + "..." : raw));

                    // 1) CERT isteği
                    if (raw.Equals(MessageTypes.GET_CERT, StringComparison.OrdinalIgnoreCase))
                    {
                        if (string.IsNullOrWhiteSpace(_myCertRaw))
                        {
                            await writer.WriteLineAsync("ERR:NO_CERT");
                            Log("GET_CERT geldi ama sertifika yok ❌ (Önce Connect to CA)");
                            return;
                        }

                        await writer.WriteLineAsync(_myCertRaw);
                        Log("Client2 CERT gönderildi ✅");
                        return;
                    }

                    // 2) Peer cert (Client1 kendi CERT'ini yolluyor)
                    if (raw.StartsWith(MessageTypes.PEER_CERT + ":", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!_keys.HasCaPublicKey)
                        {
                            await writer.WriteLineAsync("ERR:NO_CA_KEY");
                            Log("PEER_CERT geldi ama CA public key yok ❌ (Önce Connect to CA)");
                            return;
                        }

                        string cert = raw.Substring((MessageTypes.PEER_CERT + ":").Length).Trim();
                        var p = ProtocolMessage.Parse(cert);

                        // CERT:<clientId>:<pub>:<sig>
                        if (p.Length < 4 || !string.Equals(p[0], MessageTypes.CERT, StringComparison.Ordinal))
                        {
                            await writer.WriteLineAsync("ERR:BAD_PEER_CERT");
                            Log("PEER_CERT formatı hatalı ❌");
                            return;
                        }

                        string clientId = p[1];
                        string pubB64 = p[2];
                        string sigB64 = p[3];

                        bool ok = _keys.VerifyOtherClientCertificate(clientId, pubB64, sigB64);
                        if (!ok)
                        {
                            await writer.WriteLineAsync("ERR:PEER_CERT_INVALID");
                            Log("PEER_CERT doğrulanamadı ❌");
                            return;
                        }

                        _client1PubKeyBase64FromCert = pubB64;
                        await writer.WriteLineAsync("OK:PEER_CERT_ACCEPTED");
                        Log("PEER_CERT doğrulandı ve kabul edildi ✅");
                        return;
                    }

                    // 3) SESSION_KEY (ana akış)
                    if (raw.StartsWith(MessageTypes.SESSION_KEY + ":", StringComparison.OrdinalIgnoreCase))
                    {
                        string encB64 = raw.Substring((MessageTypes.SESSION_KEY + ":").Length).Trim();
                        if (string.IsNullOrWhiteSpace(encB64))
                        {
                            await writer.WriteLineAsync("ERR:EMPTY_SESSION_KEY");
                            Log("SESSION_KEY boş geldi ❌");
                            return;
                        }

                        byte[] enc;
                        try
                        {
                            enc = Convert.FromBase64String(encB64);
                        }
                        catch
                        {
                            await writer.WriteLineAsync("ERR:BAD_BASE64");
                            Log("SESSION_KEY Base64 hatalı ❌");
                            return;
                        }

                        // decrypt with Client2 private key
                        byte[] ks = _keys.DecryptBytesWithMyPrivateKey(enc);
                        _ks = ks;

                        await writer.WriteLineAsync("OK:SESSION_KEY_RECEIVED");
                        Log("SESSION_KEY alındı ve çözüldü ✅");
                        Log("Ks(Base64): " + Convert.ToBase64String(ks));
                        return;
                    }

                    await writer.WriteLineAsync("ERR:UNKNOWN_MESSAGE");
                    Log("Bilinmeyen mesaj türü ❌");
                }
                catch (Exception ex)
                {
                    Log("HandleClient hata: " + ex.Message);
                }
            }
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

        private static int GetRequiredPort(TextBox txtPort, string label)
        {
            string portRaw = (txtPort.Text ?? "").Trim();

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

            return port;
        }

        private void HookLiveValidation()
        {
            txtCaIp.TextChanged += (_, __) => UpdateButtonStates();
            txtCaPort.TextChanged += (_, __) => UpdateButtonStates();
            txtListenPort.TextChanged += (_, __) => UpdateButtonStates();
        }

        private void UpdateButtonStates()
        {
            bool caOk = IsValidIPv4(txtCaIp.Text) && IsValidPort(txtCaPort.Text);
            bool listenOk = IsValidPort(txtListenPort.Text);

            btnConnectToCA.Enabled = caOk;
            btnStartListener.Enabled = listenOk;
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
