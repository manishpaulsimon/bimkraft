using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using BIMKraft.Models;
using Newtonsoft.Json;

namespace BIMKraft.Services
{
    /// <summary>
    /// Cryptographic utilities for license encryption, decryption, and validation
    /// </summary>
    public static class LicenseCrypto
    {
        // Secret key for encryption (obfuscated across multiple parts)
        // In production, consider additional obfuscation techniques
        private static readonly byte[] KeyPart1 = new byte[] { 0x42, 0x49, 0x4D, 0x4B, 0x52, 0x41, 0x46, 0x54 };
        private static readonly byte[] KeyPart2 = new byte[] { 0x2D, 0x53, 0x45, 0x43, 0x52, 0x45, 0x54, 0x2D };
        private static readonly byte[] KeyPart3 = new byte[] { 0x4B, 0x45, 0x59, 0x2D, 0x32, 0x30, 0x32, 0x35 };

        /// <summary>
        /// Get the encryption key (derived from multiple parts)
        /// </summary>
        private static byte[] GetEncryptionKey()
        {
            byte[] combined = new byte[KeyPart1.Length + KeyPart2.Length + KeyPart3.Length];
            Buffer.BlockCopy(KeyPart1, 0, combined, 0, KeyPart1.Length);
            Buffer.BlockCopy(KeyPart2, 0, combined, KeyPart1.Length, KeyPart2.Length);
            Buffer.BlockCopy(KeyPart3, 0, combined, KeyPart1.Length + KeyPart2.Length, KeyPart3.Length);

            // Derive 256-bit key using SHA256
            using (var sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(combined);
            }
        }

        /// <summary>
        /// Encrypt license data and generate license key
        /// </summary>
        public static string EncryptLicense(LicenseInfo licenseInfo)
        {
            try
            {
                // Serialize license info to JSON
                string json = JsonConvert.SerializeObject(licenseInfo);
                byte[] plainText = Encoding.UTF8.GetBytes(json);

                using (Aes aes = Aes.Create())
                {
                    aes.Key = GetEncryptionKey();
                    aes.GenerateIV();

                    using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                    using (var msEncrypt = new MemoryStream())
                    {
                        // Write IV first (needed for decryption)
                        msEncrypt.Write(aes.IV, 0, aes.IV.Length);

                        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(json);
                        }

                        byte[] encrypted = msEncrypt.ToArray();

                        // Generate HMAC signature for integrity
                        string signature = GenerateSignature(Convert.ToBase64String(encrypted));

                        // Combine encrypted data and signature
                        string combined = Convert.ToBase64String(encrypted) + "|" + signature;

                        // Convert to Base32 and format as license key
                        return FormatLicenseKey(Base32Encode(Encoding.UTF8.GetBytes(combined)));
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"License encryption failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Decrypt and validate license key
        /// </summary>
        public static LicenseInfo DecryptLicense(string licenseKey)
        {
            try
            {
                // Remove formatting (hyphens and prefix)
                string cleanKey = licenseKey.Replace("-", "").Replace("BIMK", "");

                // Decode from Base32
                byte[] combined = Base32Decode(cleanKey);
                string combinedString = Encoding.UTF8.GetString(combined);

                // Split encrypted data and signature
                string[] parts = combinedString.Split('|');
                if (parts.Length != 2)
                {
                    throw new Exception("Invalid license key format");
                }

                string encryptedData = parts[0];
                string signature = parts[1];

                // Verify signature
                if (!ValidateSignature(encryptedData, signature))
                {
                    throw new Exception("License key signature validation failed");
                }

                // Decrypt
                byte[] encrypted = Convert.FromBase64String(encryptedData);

                using (Aes aes = Aes.Create())
                {
                    aes.Key = GetEncryptionKey();

                    // Extract IV from the beginning
                    byte[] iv = new byte[16];
                    Buffer.BlockCopy(encrypted, 0, iv, 0, 16);
                    aes.IV = iv;

                    // Extract cipher text
                    byte[] cipherText = new byte[encrypted.Length - 16];
                    Buffer.BlockCopy(encrypted, 16, cipherText, 0, cipherText.Length);

                    using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                    using (var msDecrypt = new MemoryStream(cipherText))
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    using (var srDecrypt = new StreamReader(csDecrypt))
                    {
                        string json = srDecrypt.ReadToEnd();
                        return JsonConvert.DeserializeObject<LicenseInfo>(json);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"License decryption failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Generate HMAC-SHA256 signature for data integrity
        /// </summary>
        public static string GenerateSignature(string data)
        {
            byte[] key = GetEncryptionKey();
            using (var hmac = new HMACSHA256(key))
            {
                byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
                return Convert.ToBase64String(hash);
            }
        }

        /// <summary>
        /// Validate HMAC signature
        /// </summary>
        public static bool ValidateSignature(string data, string signature)
        {
            string expected = GenerateSignature(data);
            return expected == signature;
        }

        /// <summary>
        /// Format license key as BIMK-XXXX-XXXX-XXXX-XXXX-XXXX
        /// </summary>
        private static string FormatLicenseKey(string rawKey)
        {
            // Add prefix
            string key = "BIMK" + rawKey;

            // Insert hyphens every 4 characters
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

        /// <summary>
        /// Base32 encoding (no ambiguous characters like O/0, I/1)
        /// </summary>
        private static string Base32Encode(byte[] data)
        {
            const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // No I, O, 0, 1
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

        /// <summary>
        /// Base32 decoding
        /// </summary>
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

        /// <summary>
        /// Generate a unique machine fingerprint (for Phase 2 online validation)
        /// </summary>
        public static string GetMachineFingerprint()
        {
            try
            {
                // Combine multiple hardware identifiers
                string machineId = Environment.MachineName;
                string cpuId = GetCpuId();
                string motherboardId = GetMotherboardId();

                string combined = $"{machineId}|{cpuId}|{motherboardId}";

                // Hash to create fingerprint
                using (var sha256 = SHA256.Create())
                {
                    byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(combined));
                    return Convert.ToBase64String(hash).Substring(0, 22); // Shortened for readability
                }
            }
            catch
            {
                // Fallback to machine name only
                return Convert.ToBase64String(
                    SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(Environment.MachineName))
                ).Substring(0, 22);
            }
        }

        /// <summary>
        /// Get CPU ID (simplified, for basic fingerprinting)
        /// </summary>
        private static string GetCpuId()
        {
            try
            {
                using (var searcher = new System.Management.ManagementObjectSearcher("SELECT ProcessorId FROM Win32_Processor"))
                {
                    foreach (var item in searcher.Get())
                    {
                        return item["ProcessorId"]?.ToString() ?? "UNKNOWN";
                    }
                }
            }
            catch
            {
                return "UNKNOWN";
            }

            return "UNKNOWN";
        }

        /// <summary>
        /// Get Motherboard ID (simplified, for basic fingerprinting)
        /// </summary>
        private static string GetMotherboardId()
        {
            try
            {
                using (var searcher = new System.Management.ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BaseBoard"))
                {
                    foreach (var item in searcher.Get())
                    {
                        return item["SerialNumber"]?.ToString() ?? "UNKNOWN";
                    }
                }
            }
            catch
            {
                return "UNKNOWN";
            }

            return "UNKNOWN";
        }
    }
}
