using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using BIMKraft.Models;
using Newtonsoft.Json;

namespace BIMKraft.Services
{
    /// <summary>
    /// Standalone version of LicenseCrypto without System.Management dependency
    /// </summary>
    public static class LicenseCrypto
    {
        // Secret key for encryption (obfuscated across multiple parts)
        private static readonly byte[] KeyPart1 = new byte[] { 0x42, 0x49, 0x4D, 0x4B, 0x52, 0x41, 0x46, 0x54 };
        private static readonly byte[] KeyPart2 = new byte[] { 0x2D, 0x53, 0x45, 0x43, 0x52, 0x45, 0x54, 0x2D };
        private static readonly byte[] KeyPart3 = new byte[] { 0x4B, 0x45, 0x59, 0x2D, 0x32, 0x30, 0x32, 0x35 };

        private static byte[] GetEncryptionKey()
        {
            byte[] combined = new byte[KeyPart1.Length + KeyPart2.Length + KeyPart3.Length];
            Buffer.BlockCopy(KeyPart1, 0, combined, 0, KeyPart1.Length);
            Buffer.BlockCopy(KeyPart2, 0, combined, KeyPart1.Length, KeyPart2.Length);
            Buffer.BlockCopy(KeyPart3, 0, combined, KeyPart1.Length + KeyPart2.Length, KeyPart3.Length);

            using (var sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(combined);
            }
        }

        public static string EncryptLicense(LicenseInfo licenseInfo)
        {
            try
            {
                string json = JsonConvert.SerializeObject(licenseInfo);
                byte[] plainText = Encoding.UTF8.GetBytes(json);

                using (Aes aes = Aes.Create())
                {
                    aes.Key = GetEncryptionKey();
                    aes.GenerateIV();

                    using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                    using (var msEncrypt = new MemoryStream())
                    {
                        msEncrypt.Write(aes.IV, 0, aes.IV.Length);

                        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(json);
                        }

                        byte[] encrypted = msEncrypt.ToArray();
                        string signature = GenerateSignature(Convert.ToBase64String(encrypted));
                        string combined = Convert.ToBase64String(encrypted) + "|" + signature;

                        return FormatLicenseKey(Base32Encode(Encoding.UTF8.GetBytes(combined)));
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"License encryption failed: {ex.Message}");
            }
        }

        public static LicenseInfo DecryptLicense(string licenseKey)
        {
            try
            {
                string cleanKey = licenseKey.Replace("-", "").Replace("BIMK", "");
                byte[] combined = Base32Decode(cleanKey);
                string combinedString = Encoding.UTF8.GetString(combined);

                string[] parts = combinedString.Split('|');
                if (parts.Length != 2)
                {
                    throw new Exception("Invalid license key format");
                }

                string encryptedData = parts[0];
                string signature = parts[1];

                if (!ValidateSignature(encryptedData, signature))
                {
                    throw new Exception("License key signature validation failed");
                }

                byte[] encrypted = Convert.FromBase64String(encryptedData);

                using (Aes aes = Aes.Create())
                {
                    aes.Key = GetEncryptionKey();

                    byte[] iv = new byte[16];
                    Buffer.BlockCopy(encrypted, 0, iv, 0, 16);
                    aes.IV = iv;

                    byte[] cipherText = new byte[encrypted.Length - 16];
                    Buffer.BlockCopy(encrypted, 16, cipherText, 0, cipherText.Length);

                    using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                    using (var msDecrypt = new MemoryStream(cipherText))
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    using (var srDecrypt = new StreamReader(csDecrypt))
                    {
                        string json = srDecrypt.ReadToEnd();
                        return JsonConvert.DeserializeObject<LicenseInfo>(json) ?? throw new Exception("Failed to deserialize license");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"License decryption failed: {ex.Message}");
            }
        }

        public static string GenerateSignature(string data)
        {
            byte[] key = GetEncryptionKey();
            using (var hmac = new HMACSHA256(key))
            {
                byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
                return Convert.ToBase64String(hash);
            }
        }

        public static bool ValidateSignature(string data, string signature)
        {
            string expected = GenerateSignature(data);
            return expected == signature;
        }

        private static string FormatLicenseKey(string rawKey)
        {
            string key = "BIMK" + rawKey;
            StringBuilder formatted = new StringBuilder();
            for (int i = 0; i < key.Length; i++)
            {
                if (i > 0 && i % 4 == 0)
                {
                    formatted.Append('-');
                }
                formatted.Append(key[i]);
            }
            return formatted.ToString();
        }

        private static string Base32Encode(byte[] data)
        {
            const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            StringBuilder result = new StringBuilder();
            int buffer = 0;
            int bitsLeft = 0;

            foreach (byte b in data)
            {
                buffer = (buffer << 8) | b;
                bitsLeft += 8;

                while (bitsLeft >= 5)
                {
                    int index = (buffer >> (bitsLeft - 5)) & 0x1F;
                    result.Append(alphabet[index]);
                    bitsLeft -= 5;
                }
            }

            if (bitsLeft > 0)
            {
                int index = (buffer << (5 - bitsLeft)) & 0x1F;
                result.Append(alphabet[index]);
            }

            return result.ToString();
        }

        private static byte[] Base32Decode(string encoded)
        {
            const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var bytes = new System.Collections.Generic.List<byte>();
            int buffer = 0;
            int bitsLeft = 0;

            foreach (char c in encoded.ToUpper())
            {
                int value = alphabet.IndexOf(c);
                if (value < 0)
                {
                    throw new Exception($"Invalid character in license key: {c}");
                }

                buffer = (buffer << 5) | value;
                bitsLeft += 5;

                if (bitsLeft >= 8)
                {
                    bytes.Add((byte)((buffer >> (bitsLeft - 8)) & 0xFF));
                    bitsLeft -= 8;
                }
            }

            return bytes.ToArray();
        }
    }
}
