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
    /// Core license management service for trial tracking and license validation
    /// </summary>
    public class LicenseManager
    {
        private static readonly string AppDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "BIMKraft"
        );

        private static readonly string TrialDataFile = Path.Combine(AppDataFolder, "trial.dat");
        private static readonly string LicenseDataFile = Path.Combine(AppDataFolder, "license.dat");

        // Grace period in days after license expires
        private const int GracePeriodDays = 7;

        // Trial period in days
        private const int TrialPeriodDays = 30;

        /// <summary>
        /// Validate current license status and return license information
        /// </summary>
        public static LicenseInfo ValidateLicense()
        {
            try
            {
                // Ensure AppData folder exists
                EnsureAppDataFolderExists();

                // First, check if a paid license exists
                if (File.Exists(LicenseDataFile))
                {
                    LicenseInfo license = LoadLicenseInfo();
                    if (license != null)
                    {
                        return ValidatePaidLicense(license);
                    }
                }

                // No paid license, check trial status
                return ValidateTrialLicense();
            }
            catch (Exception ex)
            {
                // If validation fails, return invalid status
                return new LicenseInfo
                {
                    Status = LicenseStatus.Invalid,
                    RemainingDays = 0,
                    Type = LicenseType.Trial
                };
            }
        }

        /// <summary>
        /// Activate a new license key
        /// </summary>
        public static LicenseInfo ActivateLicense(string licenseKey)
        {
            try
            {
                // Decrypt and validate the license key
                LicenseInfo license = LicenseCrypto.DecryptLicense(licenseKey);

                // Verify license is not expired
                if (license.ExpiryDate < DateTime.UtcNow)
                {
                    throw new Exception("License key has expired");
                }

                // Update status based on expiry
                license.Status = LicenseStatus.Active;
                license.RemainingDays = (int)(license.ExpiryDate - DateTime.UtcNow).TotalDays;

                // Save license to disk
                SaveLicenseInfo(license);

                return license;
            }
            catch (Exception ex)
            {
                throw new Exception($"License activation failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Start a new trial period
        /// </summary>
        public static TrialInfo StartTrial()
        {
            try
            {
                EnsureAppDataFolderExists();

                // Check if trial already exists
                if (File.Exists(TrialDataFile))
                {
                    TrialInfo existing = LoadTrialInfo();
                    if (existing != null && VerifyTrialHash(existing))
                    {
                        // Trial already started, return existing
                        return existing;
                    }
                }

                // Create new trial
                TrialInfo trial = new TrialInfo
                {
                    FirstLaunchDate = DateTime.UtcNow,
                    TrialStartDate = DateTime.UtcNow,
                    TrialEndDate = DateTime.UtcNow.AddDays(TrialPeriodDays),
                    NotificationsSent = new System.Collections.Generic.List<string>()
                };

                // Generate hash for integrity
                trial.Hash = GenerateTrialHash(trial);

                // Save to disk
                SaveTrialInfo(trial);

                return trial;
            }
            catch (Exception ex)
            {
                throw new Exception($"Trial initialization failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Check if a specific feature is available based on license status
        /// </summary>
        public static bool IsFeatureAvailable(string featureName)
        {
            LicenseInfo license = ValidateLicense();

            // During trial or active license, all features are available
            if (license.Status == LicenseStatus.Trial || license.Status == LicenseStatus.Active)
            {
                return true;
            }

            // During grace period, features are available with warnings
            if (license.Status == LicenseStatus.GracePeriod)
            {
                return true;
            }

            // Expired or invalid - no features available
            return false;
        }

        /// <summary>
        /// Get current license status
        /// </summary>
        public static LicenseStatus GetLicenseStatus()
        {
            return ValidateLicense().Status;
        }

        /// <summary>
        /// Get trial information (returns null if no trial exists)
        /// </summary>
        public static TrialInfo GetTrialInfo()
        {
            if (!File.Exists(TrialDataFile))
            {
                return null;
            }

            return LoadTrialInfo();
        }

        /// <summary>
        /// Check if trial reminder should be shown and mark as sent
        /// </summary>
        public static bool ShouldShowTrialReminder(out int daysRemaining)
        {
            daysRemaining = 0;

            TrialInfo trial = GetTrialInfo();
            if (trial == null || !trial.IsActive())
            {
                return false;
            }

            daysRemaining = trial.GetRemainingDays();

            if (trial.ShouldShowNotification(daysRemaining))
            {
                // Mark notification as sent
                trial.MarkNotificationSent(daysRemaining);
                SaveTrialInfo(trial);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Deactivate current license (remove license file)
        /// </summary>
        public static void DeactivateLicense()
        {
            try
            {
                if (File.Exists(LicenseDataFile))
                {
                    File.Delete(LicenseDataFile);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"License deactivation failed: {ex.Message}");
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// Validate a paid license and update its status
        /// </summary>
        private static LicenseInfo ValidatePaidLicense(LicenseInfo license)
        {
            DateTime now = DateTime.UtcNow;

            // Check if license is still active
            if (license.ExpiryDate > now)
            {
                license.Status = LicenseStatus.Active;
                license.RemainingDays = (int)(license.ExpiryDate - now).TotalDays;
                return license;
            }

            // License expired - check grace period
            DateTime gracePeriodEnd = license.ExpiryDate.AddDays(GracePeriodDays);
            if (now <= gracePeriodEnd)
            {
                license.Status = LicenseStatus.GracePeriod;
                license.RemainingDays = (int)(gracePeriodEnd - now).TotalDays;
                return license;
            }

            // License expired beyond grace period
            license.Status = LicenseStatus.Expired;
            license.RemainingDays = -1;
            return license;
        }

        /// <summary>
        /// Validate trial license status
        /// </summary>
        private static LicenseInfo ValidateTrialLicense()
        {
            // Check if trial exists
            if (!File.Exists(TrialDataFile))
            {
                // No trial started yet - return invalid status (will prompt to start trial)
                return new LicenseInfo
                {
                    Status = LicenseStatus.Invalid,
                    Type = LicenseType.Trial,
                    RemainingDays = 0
                };
            }

            // Load trial info
            TrialInfo trial = LoadTrialInfo();
            if (trial == null || !VerifyTrialHash(trial))
            {
                // Trial file corrupted - treat as invalid
                return new LicenseInfo
                {
                    Status = LicenseStatus.Invalid,
                    Type = LicenseType.Trial,
                    RemainingDays = 0
                };
            }

            // Check if trial is still active
            if (trial.IsActive())
            {
                return new LicenseInfo
                {
                    Status = LicenseStatus.Trial,
                    Type = LicenseType.Trial,
                    RemainingDays = trial.GetRemainingDays(),
                    IssuedDate = trial.TrialStartDate,
                    ExpiryDate = trial.TrialEndDate
                };
            }

            // Trial expired
            return new LicenseInfo
            {
                Status = LicenseStatus.Expired,
                Type = LicenseType.Trial,
                RemainingDays = 0,
                IssuedDate = trial.TrialStartDate,
                ExpiryDate = trial.TrialEndDate
            };
        }

        /// <summary>
        /// Save license information to disk
        /// </summary>
        private static void SaveLicenseInfo(LicenseInfo license)
        {
            try
            {
                EnsureAppDataFolderExists();
                string json = JsonConvert.SerializeObject(license, Formatting.Indented);

                // Encrypt the license data before saving
                byte[] encrypted = ProtectedData.Protect(
                    Encoding.UTF8.GetBytes(json),
                    null,
                    DataProtectionScope.CurrentUser
                );

                File.WriteAllBytes(LicenseDataFile, encrypted);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to save license data: {ex.Message}");
            }
        }

        /// <summary>
        /// Load license information from disk
        /// </summary>
        private static LicenseInfo LoadLicenseInfo()
        {
            try
            {
                if (!File.Exists(LicenseDataFile))
                {
                    return null;
                }

                byte[] encrypted = File.ReadAllBytes(LicenseDataFile);

                // Decrypt the license data
                byte[] decrypted = ProtectedData.Unprotect(
                    encrypted,
                    null,
                    DataProtectionScope.CurrentUser
                );

                string json = Encoding.UTF8.GetString(decrypted);
                return JsonConvert.DeserializeObject<LicenseInfo>(json);
            }
            catch (Exception)
            {
                // If decryption or deserialization fails, return null
                return null;
            }
        }

        /// <summary>
        /// Save trial information to disk
        /// </summary>
        private static void SaveTrialInfo(TrialInfo trial)
        {
            try
            {
                EnsureAppDataFolderExists();

                // Regenerate hash before saving
                trial.Hash = GenerateTrialHash(trial);

                string json = JsonConvert.SerializeObject(trial, Formatting.Indented);
                File.WriteAllText(TrialDataFile, json);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to save trial data: {ex.Message}");
            }
        }

        /// <summary>
        /// Load trial information from disk
        /// </summary>
        private static TrialInfo LoadTrialInfo()
        {
            try
            {
                if (!File.Exists(TrialDataFile))
                {
                    return null;
                }

                string json = File.ReadAllText(TrialDataFile);
                return JsonConvert.DeserializeObject<TrialInfo>(json);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Generate SHA-256 hash for trial data integrity
        /// </summary>
        private static string GenerateTrialHash(TrialInfo trial)
        {
            string data = $"{trial.FirstLaunchDate:O}|{trial.TrialStartDate:O}|{trial.TrialEndDate:O}";

            using (var sha256 = SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
                return Convert.ToBase64String(hash);
            }
        }

        /// <summary>
        /// Verify trial data hasn't been tampered with
        /// </summary>
        private static bool VerifyTrialHash(TrialInfo trial)
        {
            if (string.IsNullOrEmpty(trial.Hash))
            {
                return false;
            }

            string expectedHash = GenerateTrialHash(trial);
            return expectedHash == trial.Hash;
        }

        /// <summary>
        /// Ensure AppData folder exists
        /// </summary>
        private static void EnsureAppDataFolderExists()
        {
            if (!Directory.Exists(AppDataFolder))
            {
                Directory.CreateDirectory(AppDataFolder);
            }
        }

        #endregion
    }
}
