using System;
using System.Security.Cryptography;
using System.Text;
using SharedSecurityLib.Protocol;

namespace Client1App.Security
{
    internal class ClientKeyManager
    {
        private RSA? _clientRsa;

        private string? _caPublicKeyBase64;

        // Kendi sertifikamız (CA'den dönen)
        private string? _myCertClientId;
        private string? _myCertClientPublicKeyBase64;
        private string? _myCertSignatureBase64;

        // =====================================================
        // CLIENT KEY MANAGEMENT
        // =====================================================

        public bool HasClientKeys => _clientRsa != null;
        public bool HasCaPublicKey => !string.IsNullOrWhiteSpace(_caPublicKeyBase64);

        public bool HasMyCertificate =>
            !string.IsNullOrWhiteSpace(_myCertClientId) &&
            !string.IsNullOrWhiteSpace(_myCertClientPublicKeyBase64) &&
            !string.IsNullOrWhiteSpace(_myCertSignatureBase64);

        public void GenerateClientKeys()
        {
            _clientRsa?.Dispose();
            _clientRsa = RSA.Create(2048);
        }

        public string GetClientPublicKeyBase64()
        {
            if (_clientRsa == null)
                throw new InvalidOperationException("Client keys not generated yet.");

            byte[] pub = _clientRsa.ExportSubjectPublicKeyInfo();
            return Convert.ToBase64String(pub);
        }

        // =====================================================
        // CA KEY MANAGEMENT
        // =====================================================

        public void SetCaPublicKey(string base64)
        {
            if (string.IsNullOrWhiteSpace(base64))
                throw new ArgumentException("CA public key is empty.");

            _ = Convert.FromBase64String(base64.Trim()); // Base64 doğrulama
            _caPublicKeyBase64 = base64.Trim();
        }

        public string GetCaPublicKeyBase64()
        {
            if (_caPublicKeyBase64 == null)
                throw new InvalidOperationException("CA public key not set.");

            return _caPublicKeyBase64;
        }

        // =====================================================
        // CERTIFICATE REQUEST/RESPONSE
        // =====================================================

        /// <summary>
        /// REQ_CERT:<clientId>:<clientPublicKeyBase64>
        /// </summary>
        public string BuildReqCertMessage(string clientId)
        {
            if (string.IsNullOrWhiteSpace(clientId))
                throw new ArgumentException("clientId is empty.");

            string pub = GetClientPublicKeyBase64();
            return ProtocolMessage.Build(MessageTypes.REQ_CERT, clientId.Trim(), pub);
        }

        /// <summary>
        /// CERT:<clientId>:<clientPublicKeyBase64>:<caSignatureBase64>
        /// </summary>
        public void SetMyCertificateFromCertMessage(string certMessage)
        {
            if (string.IsNullOrWhiteSpace(certMessage))
                throw new ArgumentException("certMessage is empty.");

            var parts = ProtocolMessage.Parse(certMessage.Trim());
            if (parts.Length < 4 || !string.Equals(parts[0], MessageTypes.CERT, StringComparison.Ordinal))
                throw new ArgumentException("Invalid CERT message format.");

            _myCertClientId = parts[1];
            _myCertClientPublicKeyBase64 = parts[2];

            _ = Convert.FromBase64String(parts[3].Trim());
            _myCertSignatureBase64 = parts[3].Trim();
        }

        public string BuildMyCertMessageRaw()
        {
            if (!HasMyCertificate)
                throw new InvalidOperationException("My certificate not set.");

            return ProtocolMessage.Build(
                MessageTypes.CERT,
                _myCertClientId!,
                _myCertClientPublicKeyBase64!,
                _myCertSignatureBase64!
            );
        }

        /// <summary>
        /// Kendi sertifikamızı doğrular:
        /// payload = "<clientId>:<clientPublicKeyBase64>"
        /// </summary>
        public bool VerifyMyCertificate()
        {
            if (!HasCaPublicKey)
                throw new InvalidOperationException("CA public key not set.");
            if (!HasMyCertificate)
                throw new InvalidOperationException("My certificate not set.");
            if (_clientRsa == null)
                throw new InvalidOperationException("Client keys not generated.");

            string currentPub = GetClientPublicKeyBase64();
            if (!string.Equals(currentPub, _myCertClientPublicKeyBase64, StringComparison.Ordinal))
                return false;

            return VerifyCertificateFor(_myCertClientId!, _myCertClientPublicKeyBase64!, _myCertSignatureBase64!);
        }

        public bool VerifyOtherClientCertificate(string clientId, string clientPublicKeyBase64, string signatureBase64)
        {
            if (string.IsNullOrWhiteSpace(clientId) ||
                string.IsNullOrWhiteSpace(clientPublicKeyBase64) ||
                string.IsNullOrWhiteSpace(signatureBase64))
                throw new ArgumentException("CERT fields are empty.");

            return VerifyCertificateFor(clientId.Trim(), clientPublicKeyBase64.Trim(), signatureBase64.Trim());
        }

        private bool VerifyCertificateFor(string clientId, string clientPublicKeyBase64, string signatureBase64)
        {
            if (_caPublicKeyBase64 == null)
                throw new InvalidOperationException("CA public key not set.");

            using RSA caRsa = RSA.Create();

            byte[] caPubBytes = Convert.FromBase64String(_caPublicKeyBase64);
            caRsa.ImportSubjectPublicKeyInfo(caPubBytes, out _);

            string payload = $"{clientId}:{clientPublicKeyBase64}";
            byte[] data = Encoding.UTF8.GetBytes(payload);

            byte[] sig = Convert.FromBase64String(signatureBase64);

            return caRsa.VerifyData(
                data,
                sig,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1
            );
        }

        // =====================================================
        // RSA ENCRYPT/DECRYPT helpers (KM messages)
        // =====================================================

        public byte[] EncryptBytesWithOtherPublicKeyBase64(string otherClientPublicKeyBase64, byte[] plain)
        {
            if (string.IsNullOrWhiteSpace(otherClientPublicKeyBase64))
                throw new ArgumentException("Other client public key is empty.");

            byte[] pubBytes = Convert.FromBase64String(otherClientPublicKeyBase64.Trim());

            using var rsa = RSA.Create();
            rsa.ImportSubjectPublicKeyInfo(pubBytes, out _);

            return rsa.Encrypt(plain, RSAEncryptionPadding.OaepSHA256);
        }

        public byte[] DecryptBytesWithMyPrivateKey(byte[] encrypted)
        {
            if (_clientRsa == null)
                throw new InvalidOperationException("Client keys not generated.");

            return _clientRsa.Decrypt(encrypted, RSAEncryptionPadding.OaepSHA256);
        }

        // =====================================================
        // SESSION KEY (Ks) – demo helper (artık Km’den türeteceğiz)
        // =====================================================

        public byte[] GenerateSessionKey(int sizeBytes = 32)
        {
            byte[] ks = new byte[sizeBytes];
            RandomNumberGenerator.Fill(ks);
            return ks;
        }

        public byte[] EncryptSessionKeyWithClientPublicKey(byte[] sessionKey)
        {
            if (_clientRsa == null)
                throw new InvalidOperationException("Client keys not generated.");

            return _clientRsa.Encrypt(sessionKey, RSAEncryptionPadding.OaepSHA256);
        }

        public byte[] DecryptSessionKeyWithClientPrivateKey(byte[] encryptedSessionKey)
        {
            if (_clientRsa == null)
                throw new InvalidOperationException("Client keys not generated.");

            return _clientRsa.Decrypt(encryptedSessionKey, RSAEncryptionPadding.OaepSHA256);
        }

        public byte[] EncryptSessionKeyWithOtherPublicKeyBase64(string otherClientPublicKeyBase64, byte[] sessionKey)
            => EncryptBytesWithOtherPublicKeyBase64(otherClientPublicKeyBase64, sessionKey);
    }
}
