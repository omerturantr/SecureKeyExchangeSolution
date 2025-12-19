using System;
using System.Security.Cryptography;
using System.Text;
using SharedSecurityLib.Models;

namespace SharedSecurityLib.Crypto
{
    public static class CryptoHelper
    {
        // Yeni bir RSA anahtar çifti üretir
        public static RSA CreateRsaKey(int keySize = 2048)
        {
            var rsa = RSA.Create();
            rsa.KeySize = keySize;
            return rsa;
        }

        // RSA public key'i Base64 olarak döndürür
        public static string ExportPublicKeyBase64(RSA rsa)
        {
            byte[] publicKeyBytes = rsa.ExportSubjectPublicKeyInfo();
            return Convert.ToBase64String(publicKeyBytes);
        }

        // Sertifikanın imzalanacak özet verisini hazırlar (oversimplified X.509 payload)
        public static byte[] GetCertificateDataToSign(SimpleCertificate cert)
        {
            // CertificatePayload overload kullanıyoruz (tek standart)
            string data = CertificatePayload.BuildToSign(cert);
            return Encoding.UTF8.GetBytes(data);
        }

        // CA sertifikayı dijital olarak imzalar
        public static void SignCertificate(SimpleCertificate cert, RSA caPrivateKey)
        {
            byte[] dataToSign = GetCertificateDataToSign(cert);

            byte[] signature = caPrivateKey.SignData(
                dataToSign,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);

            cert.SignatureBase64 = Convert.ToBase64String(signature);
        }

        // İstemci sertifika imzasını doğrular
        public static bool VerifyCertificate(SimpleCertificate cert, RSA caPublicKey)
        {
            byte[] dataToSign = GetCertificateDataToSign(cert);
            byte[] signature = Convert.FromBase64String(cert.SignatureBase64);

            return caPublicKey.VerifyData(
                dataToSign,
                signature,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);
        }

        // =====================================================
        // ADIM 9: Km (Master Key) + Ks türetme yardımcıları
        // =====================================================

        public static byte[] Combine(params byte[][] arrays)
        {
            int len = 0;
            foreach (var a in arrays) len += a.Length;

            byte[] result = new byte[len];
            int offset = 0;

            foreach (var a in arrays)
            {
                Buffer.BlockCopy(a, 0, result, offset, a.Length);
                offset += a.Length;
            }

            return result;
        }

        public static byte[] ComputeSha256(byte[] data)
        {
            using var sha = SHA256.Create();
            return sha.ComputeHash(data);
        }

        public static byte[] ComputeHmacSha256(byte[] key, string text)
        {
            using var h = new HMACSHA256(key);
            return h.ComputeHash(Encoding.UTF8.GetBytes(text));
        }

        // Km = SHA256(N1 || N2 || IDA || IDB)
        public static byte[] DeriveMasterKey(byte[] n1, byte[] n2, string initiatorId, string responderId)
        {
            return ComputeSha256(Combine(
                n1,
                n2,
                Encoding.UTF8.GetBytes(initiatorId),
                Encoding.UTF8.GetBytes(responderId)
            ));
        }

        // Ks = HMACSHA256(Km, "Ks")  (32 byte)
        public static byte[] DeriveSessionKey(byte[] masterKeyKm)
        {
            return ComputeHmacSha256(masterKeyKm, "Ks");
        }

        // Confirm = HMACSHA256(Ks, "OK")
        public static byte[] BuildSessionConfirmMac(byte[] sessionKeyKs)
        {
            return ComputeHmacSha256(sessionKeyKs, "OK");
        }

        // RSA encrypt with receiver public key (SPKI Base64)
        public static byte[] EncryptWithPublicKeyBase64(string publicKeyBase64, byte[] plain)
        {
            byte[] pubBytes = Convert.FromBase64String(publicKeyBase64.Trim());

            using var rsa = RSA.Create();
            rsa.ImportSubjectPublicKeyInfo(pubBytes, out _);

            return rsa.Encrypt(plain, RSAEncryptionPadding.OaepSHA256);
        }

        // =====================================================
        // ADIM 10: Ks ile veri şifreleme (AES-GCM)
        // DATA:<nonceB64>:<cipherB64>:<tagB64>
        // =====================================================

        public static byte[] RandomBytes(int len)
        {
            var b = new byte[len];
            RandomNumberGenerator.Fill(b);
            return b;
        }

        /// <summary>
        /// keyBase64: Ks (Base64). Decode sonrası 16/24/32 bayt olmalı. (Bizim Ks = HMACSHA256 => 32 bayt)
        /// </summary>
        public static (string nonceB64, string cipherB64, string tagB64) EncryptAesGcm(string keyBase64, string plainText)
        {
            if (string.IsNullOrWhiteSpace(keyBase64))
                throw new ArgumentException("keyBase64 boş olamaz.", nameof(keyBase64));

            byte[] key = Convert.FromBase64String(keyBase64);
            byte[] nonce = RandomBytes(12); // GCM nonce: 12 bayt önerilir
            byte[] pt = Encoding.UTF8.GetBytes(plainText ?? string.Empty);

            byte[] ct = new byte[pt.Length];
            byte[] tag = new byte[16]; // 128-bit tag

            using var aes = new AesGcm(key);
            aes.Encrypt(nonce, pt, ct, tag);

            return (Convert.ToBase64String(nonce), Convert.ToBase64String(ct), Convert.ToBase64String(tag));
        }

        /// <summary>
        /// keyBase64: Ks (Base64)
        /// </summary>
        public static string DecryptAesGcm(string keyBase64, string nonceB64, string cipherB64, string tagB64)
        {
            if (string.IsNullOrWhiteSpace(keyBase64))
                throw new ArgumentException("keyBase64 boş olamaz.", nameof(keyBase64));

            byte[] key = Convert.FromBase64String(keyBase64);
            byte[] nonce = Convert.FromBase64String(nonceB64);
            byte[] ct = Convert.FromBase64String(cipherB64);
            byte[] tag = Convert.FromBase64String(tagB64);

            byte[] pt = new byte[ct.Length];

            using var aes = new AesGcm(key);
            aes.Decrypt(nonce, ct, tag, pt);

            return Encoding.UTF8.GetString(pt);
        }
    }
}
