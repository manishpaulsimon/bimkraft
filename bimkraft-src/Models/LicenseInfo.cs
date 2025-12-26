using System;
using System.Collections.Generic;

namespace BIMKraft.Models
{
    /// <summary>
    /// License information data model
    /// </summary>
    public class LicenseInfo
    {
        /// <summary>
        /// Unique license identifier (GUID)
        /// </summary>
        public string LicenseId { get; set; }

        /// <summary>
        /// User's email address
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Type of license (Trial, Monthly, Yearly)
        /// </summary>
        public LicenseType Type { get; set; }

        /// <summary>
        /// Date when license was issued
        /// </summary>
        public DateTime IssuedDate { get; set; }

        /// <summary>
        /// Date when license expires
        /// </summary>
        public DateTime ExpiryDate { get; set; }

        /// <summary>
        /// Current license status
        /// </summary>
        public LicenseStatus Status { get; set; }

        /// <summary>
        /// Number of days remaining (can be negative if expired)
        /// </summary>
        public int RemainingDays { get; set; }

        /// <summary>
        /// List of features available with this license
        /// </summary>
        public List<string> Features { get; set; }

        /// <summary>
        /// Machine fingerprint (optional, for Phase 2 online validation)
        /// </summary>
        public string MachineId { get; set; }

        public LicenseInfo()
        {
            Features = new List<string>();
        }

        /// <summary>
        /// Get human-readable status message
        /// </summary>
        public string GetStatusMessage()
        {
            switch (Status)
            {
                case LicenseStatus.Active:
                    return $"License active. Expires on {ExpiryDate:yyyy-MM-dd} ({RemainingDays} days remaining)";

                case LicenseStatus.Trial:
                    return $"Trial active. {RemainingDays} days remaining";

                case LicenseStatus.GracePeriod:
                    return $"License expired. Grace period: {RemainingDays} days remaining";

                case LicenseStatus.Expired:
                    return "License expired. Please renew to continue using BIMKraft.";

                case LicenseStatus.Invalid:
                    return "No valid license found. Please activate or start trial.";

                default:
                    return "Unknown license status";
            }
        }

        /// <summary>
        /// Get color for status display (for UI)
        /// </summary>
        public string GetStatusColor()
        {
            switch (Status)
            {
                case LicenseStatus.Active:
                    return "#10B981"; // Green

                case LicenseStatus.Trial:
                    return "#3B82F6"; // Blue

                case LicenseStatus.GracePeriod:
                    return "#F59E0B"; // Orange

                case LicenseStatus.Expired:
                case LicenseStatus.Invalid:
                    return "#EF4444"; // Red

                default:
                    return "#6B7280"; // Gray
            }
        }
    }
}
