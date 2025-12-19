using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharedSecurityLib.Crypto;
using SharedSecurityLib.Protocol;   // <-- Adım 8.1'de ekledik
using SharedSecurityLib.Models;     // <-- birazdan client tarafında da kullanacağız

namespace CAApp
{
    public partial class Form1 : Form
    {
        private RSA? _caRsa;

        private TcpListener? _listener;
        private CancellationTokenSource? _cts;
        private bool _serverRunning = false;

        public Form1()
        {
            InitializeComponent();
        }

        private void Log(string message)
        {
            if (IsDisposed) return;

            void Add()
            {
                if (IsDisposed) return;

                string line = $"[{DateTime.Now:HH:mm:ss}] {message}";
                lstLog.BeginUpdate();
                lstLog.Items.Add(line);

                if (lstLog.Items.Count > 0)
                    lstLog.TopIndex = lstLog.Items.Count - 1;

                lstLog.EndUpdate();
            }

            if (lstLog.InvokeRequired)
                lstLog.BeginInvoke((Action)Add);
            else
                Add();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                _cts?.Cancel();
                _listener?.Stop();
            }
            catch { }
            base.OnFormClosing(e);
        }

        // Generate CA Keys
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                _caRsa = CryptoHelper.CreateRsaKey(2048);
                string publicKeyBase64 = CryptoHelper.ExportPublicKeyBase64(_caRsa);
                txtCAPublicKey.Text = publicKeyBase64;

                Log("CA RSA key pair generated successfully.");
            }
            catch (Exception ex)
            {
                Log("Error while generating CA keys: " + ex.Message);
            }
        }

        private void Form1_Load(object sender, EventArgs e) { }
        private void txtCAPublicKey_TextChanged(object sender, EventArgs e) { }

        // Start CA Server
        private void btnStartCAServer_Click(object sender, EventArgs e)
        {
            if (_serverRunning)
            {
                Log("Server zaten çalışıyor.");
                return;
            }

            try
            {
                _cts = new CancellationTokenSource();
                _listener = new TcpListener(IPAddress.Loopback, ProtocolConstants.CaPort);
                _listener.Start();

                _serverRunning = true;
                Log($"CA TCP Server başladı: {ProtocolConstants.Localhost}:{ProtocolConstants.CaPort} ✅");

                _ = AcceptLoopAsync(_cts.Token);
            }
            catch (Exception ex)
            {
                Log("Server başlatma hatası: " + ex.Message);
                _serverRunning = false;
            }
        }

        private async Task AcceptLoopAsync(CancellationToken token)
        {
            if (_listener == null) return;

            try
            {
                while (!token.IsCancellationRequested)
                {
                    TcpClient client;

                    try
                    {
                        client = await _listener.AcceptTcpClientAsync(token);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }

                    Log("Client bağlandı ✅");
                    _ = Task.Run(() => HandleClientAsync(client, token), token);
                }
            }
            catch (Exception ex)
            {
                Log("Accept loop hatası: " + ex.Message);
            }
            finally
            {
                Log("Accept loop durduruldu.");
                _serverRunning = false;
            }
        }

        private async Task HandleClientAsync(TcpClient client, CancellationToken token)
        {
            try
            {
                using (client)
                using (NetworkStream ns = client.GetStream())
                {
                    byte[] buffer = new byte[8192];
                    int read = await ns.ReadAsync(buffer, 0, buffer.Length, token);

                    if (read <= 0)
                    {
                        Log("Client veri göndermeden bağlantıyı kapattı.");
                        return;
                    }

                    string request = Encoding.UTF8.GetString(buffer, 0, read).Trim();
                    string response = await ProcessRequestAsync(request);

                    byte[] respBytes = Encoding.UTF8.GetBytes(response + "\n");
                    await ns.WriteAsync(respBytes, 0, respBytes.Length, token);
                    await ns.FlushAsync(token);

                    // Log response (kısa)
                    string logResp =
                        response.StartsWith(MessageTypes.CERT + ":") ? "CERT:<clientId>:<pubKey>:<sig>" :
                        response.StartsWith("CA_PUBLIC_KEY:") ? "CA_PUBLIC_KEY:<base64>" :
                        response.StartsWith("SIGNED_CERT:") ? "SIGNED_CERT:<base64>" :
                        response;

                    Log("Response sent: " + logResp);
                }
            }
            catch (Exception ex)
            {
                Log("Client handler hatası: " + ex.Message);
            }
        }

        private Task<string> ProcessRequestAsync(string request)
        {
            // CA key yoksa
            if (_caRsa == null)
                return Task.FromResult("ERR:NO_CA_KEYS");

            // Yeni protokol: REQ_CERT:<clientId>:<clientPublicKeyBase64>
            string type = ProtocolMessage.GetType(request);

            if (type == MessageTypes.REQ_CERT)
            {
                var parts = ProtocolMessage.Parse(request);
                if (parts.Length < 3)
                    return Task.FromResult("ERR:BAD_FORMAT");

                string clientId = parts[1];
                string clientPubKeyBase64 = parts[2];

                // İmzalanacak payload (standart)
                // Adım 8.2.1'de SharedSecurityLib/Models içine ekleyeceğimiz yardımcı metot:
                // $"{clientId}:{clientPubKeyBase64}"
                string toSign = $"{clientId}:{clientPubKeyBase64}";
                byte[] dataBytes = Encoding.UTF8.GetBytes(toSign);

                byte[] sig = _caRsa.SignData(dataBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                string sigBase64 = Convert.ToBase64String(sig);

                Log($"CERT üretildi ✅ (clientId={clientId})");

                string resp = ProtocolMessage.Build(MessageTypes.CERT, clientId, clientPubKeyBase64, sigBase64);
                return Task.FromResult(resp);
            }

            // (Opsiyonel) eski protokole geri uyumluluk – sende zaten vardı
            if (request == "GET_CA_PUBLIC_KEY")
            {
                string pub = CryptoHelper.ExportPublicKeyBase64(_caRsa);
                return Task.FromResult("CA_PUBLIC_KEY:" + pub);
            }

            if (request.StartsWith("SIGN_CLIENT_KEY:"))
            {
                string clientPubBase64 = request.Substring("SIGN_CLIENT_KEY:".Length).Trim();
                byte[] clientPubBytes = Convert.FromBase64String(clientPubBase64);

                byte[] signature = _caRsa.SignData(clientPubBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                string sigBase64 = Convert.ToBase64String(signature);

                Log("Client public key imzalandı (legacy).");
                return Task.FromResult("SIGNED_CERT:" + sigBase64);
            }

            return Task.FromResult("ERR:UNKNOWN_REQUEST");
        }
    }
}
