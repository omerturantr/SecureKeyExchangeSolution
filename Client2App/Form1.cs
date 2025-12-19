using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Client2App.Security;
using SharedSecurityLib.Crypto;
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

        // Km handshake state
        private string? _pendingN1B64;
        private string? _pendingN2B64;
        private byte[]? _km;
        private byte[]? _ks;

        // CA protokolünde MessageTypes içinde tanımlı değil (bu projede sabit)
        private const string CaGetPublicKey = "GET_CA_PUBLIC_KEY";
        private const string CaPublicKeyPrefix = "CA_PUBLIC_KEY:";

        public Form1()
        {
            InitializeComponent();
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
                if (!_keys.HasClientKeys)
                    _keys.GenerateClientKeys();

                Log("Client2 Public Key (Base64): " + _keys.GetClientPublicKeyBase64());

                string caPubBase64 = await RequestCaPublicKeyAsync();
                _keys.SetCaPublicKey(caPubBase64);
                Log("CA Public Key alındı ✅");

                string req = _keys.BuildReqCertMessage("Client2");
                Log("CA'ye REQ_CERT gönderiliyor...");

                string certMsg = await RequestCertFromCaAsync(req);

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

        // Listener
        private async void btnStartListener_Click(object sender, EventArgs e)
        {
            try
            {
                if (_listening)
                {
                    Log("Listener zaten çalışıyor.");
                    return;
                }

                if (!_keys.HasClientKeys)
                    _keys.GenerateClientKeys();

                Log("Client2 Public Key (Base64): " + _keys.GetClientPublicKeyBase64());

                _listener = new TcpListener(IPAddress.Parse(ProtocolConstants.Localhost), ProtocolConstants.Client2Port);
                _listener.Start();
                _listening = true;

                Log($"Client2 Listener başladı: {ProtocolConstants.Localhost}:{ProtocolConstants.Client2Port}");
                Log("Beklenen formatlar:");
                Log($"  - {MessageTypes.GET_PUBLIC_KEY}");
                Log($"  - {MessageTypes.GET_CERT}");
                Log($"  - {MessageTypes.PEER_CERT}:<CERT:...>");
                Log($"  - {MessageTypes.KM1}:<base64EncPayload>");
                Log($"  - {MessageTypes.KM3}:<base64EncPayload>");
                Log($"  - {MessageTypes.SESSION_CONFIRM}:<base64Mac>");
                Log($"  - {MessageTypes.SESSION_KEY}:<base64EncryptedKs> (legacy demo)");

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

                    // 1) Public key isteği
                    if (raw.Equals(MessageTypes.GET_PUBLIC_KEY, StringComparison.OrdinalIgnoreCase))
                    {
                        string pub = _keys.GetClientPublicKeyBase64();
                        await writer.WriteLineAsync("CLIENT2_PUBLIC_KEY:" + pub);
                        Log("Client2 public key gönderildi ✅");
                        return;
                    }

                    // 2) CERT isteği
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

                    // 3) Peer cert (Client1 kendi CERT'ini yolluyor)
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
                            Log("PEER_CERT format hatalı ❌");
                            return;
                        }

                        string peerId = p[1];
                        string peerPub = p[2];
                        string peerSig = p[3];

                        bool ok = _keys.VerifyOtherClientCertificate(peerId, peerPub, peerSig);
                        if (!ok)
                        {
                            await writer.WriteLineAsync("ERR:PEER_CERT_INVALID");
                            Log("PEER_CERT doğrulanamadı ❌");
                            return;
                        }

                        _client1PubKeyBase64FromCert = peerPub;
                        await writer.WriteLineAsync("OK:PEER_CERT");
                        Log("Client1 CERT alındı & doğrulandı ✅");
                        return;
                    }

                    // 4) KM1
                    if (raw.StartsWith(MessageTypes.KM1 + ":", StringComparison.OrdinalIgnoreCase))
                    {
                        if (string.IsNullOrWhiteSpace(_client1PubKeyBase64FromCert))
                        {
                            await writer.WriteLineAsync("ERR:NO_CLIENT1_CERT");
                            Log("KM1 geldi ama Client1 CERT yok ❌ (Önce PEER_CERT)");
                            return;
                        }

                        string encB64 = raw.Substring((MessageTypes.KM1 + ":").Length).Trim();
                        byte[] encBytes = Convert.FromBase64String(encB64);

                        // decrypt with Client2 private key
                        byte[] plain = _keys.DecryptBytesWithMyPrivateKey(encBytes);
                        string payload = Encoding.UTF8.GetString(plain); // "N1B64:Client1"

                        var parts = payload.Split(':');
                        if (parts.Length < 2)
                        {
                            await writer.WriteLineAsync("ERR:BAD_KM1");
                            return;
                        }

                        _pendingN1B64 = parts[0];
                        string initiatorId = parts[1];

                        // generate N2
                        byte[] n2 = new byte[16];
                        RandomNumberGenerator.Fill(n2);
                        _pendingN2B64 = Convert.ToBase64String(n2);

                        byte[] n1 = Convert.FromBase64String(_pendingN1B64);

                        _km = CryptoHelper.DeriveMasterKey(n1, n2, initiatorId, "Client2");
                        _ks = null; // yeni handshake başladığı için temizle

                        // KM2 payload: "N1B64:N2B64:Client2"
                        string km2Payload = $"{_pendingN1B64}:{_pendingN2B64}:Client2";
                        byte[] km2Plain = Encoding.UTF8.GetBytes(km2Payload);

                        byte[] km2Enc = _keys.EncryptBytesWithOtherPublicKeyBase64(_client1PubKeyBase64FromCert, km2Plain);
                        string km2EncB64 = Convert.ToBase64String(km2Enc);

                        await writer.WriteLineAsync(MessageTypes.KM2 + ":" + km2EncB64);
                        Log("KM2 gönderildi ✅ (Km hesaplandı)");
                        return;
                    }

                    // 5) KM3
                    if (raw.StartsWith(MessageTypes.KM3 + ":", StringComparison.OrdinalIgnoreCase))
                    {
                        if (_pendingN2B64 == null || _km == null)
                        {
                            await writer.WriteLineAsync("ERR:NO_KM_STATE");
                            Log("KM3 geldi ama state yok ❌");
                            return;
                        }

                        string encB64 = raw.Substring((MessageTypes.KM3 + ":").Length).Trim();
                        byte[] encBytes = Convert.FromBase64String(encB64);

                        byte[] plain = _keys.DecryptBytesWithMyPrivateKey(encBytes);
                        string n2B64 = Encoding.UTF8.GetString(plain).Trim();

                        if (!string.Equals(n2B64, _pendingN2B64, StringComparison.Ordinal))
                        {
                            await writer.WriteLineAsync("ERR:KM3_MISMATCH");
                            Log("KM3 mismatch ❌");
                            return;
                        }

                        _ks = CryptoHelper.DeriveSessionKey(_km);

                        await writer.WriteLineAsync("OK:KM_DONE");
                        Log("Km tamam ✅  Ks türetildi ✅");
                        return;
                    }

                    // 6) SESSION_CONFIRM
                    if (raw.StartsWith(MessageTypes.SESSION_CONFIRM + ":", StringComparison.OrdinalIgnoreCase))
                    {
                        if (_ks == null)
                        {
                            await writer.WriteLineAsync("ERR:NO_KS");
                            Log("SESSION_CONFIRM geldi ama Ks yok ❌");
                            return;
                        }

                        string macB64 = raw.Substring((MessageTypes.SESSION_CONFIRM + ":").Length).Trim();
                        byte[] recvMac = Convert.FromBase64String(macB64);

                        byte[] expected = CryptoHelper.BuildSessionConfirmMac(_ks);
                        bool ok = CryptographicOperations.FixedTimeEquals(recvMac, expected);

                        await writer.WriteLineAsync(ok ? "OK:SESSION" : "ERR:BAD_MAC");
                        Log(ok ? "SESSION_CONFIRM doğrulandı ✅ (Ks ortak)" : "SESSION_CONFIRM doğrulanamadı ❌");
                        return;
                    }

                    // Legacy: SESSION_KEY (eski demo akışı kalsın)
                    string legacyPrefix = MessageTypes.SESSION_KEY + ":";
                    if (raw.StartsWith(legacyPrefix, StringComparison.OrdinalIgnoreCase))
                    {
                        string encB64 = raw.Substring(legacyPrefix.Length).Trim();

                        byte[] encKs;
                        try { encKs = Convert.FromBase64String(encB64); }
                        catch { Log("Base64 parse hatası (legacy SESSION_KEY)."); return; }

                        byte[] ks = _keys.DecryptSessionKeyWithClientPrivateKey(encKs);

                        Log("Legacy Ks çözüldü ✅ (Base64): " + Convert.ToBase64String(ks));
                        MessageBox.Show("Client2: (Legacy) Ks alındı ve çözüldü ✅", "Client2");
                        return;
                    }

                    Log("Beklenmeyen mesaj tipi.");
                    await writer.WriteLineAsync("ERR:UNKNOWN");
                }
                catch (Exception ex)
                {
                    Log($"HandleClientAsync hata: {ex.Message} (HResult=0x{ex.HResult:X8})");
                }
            }
        }
    }
}
