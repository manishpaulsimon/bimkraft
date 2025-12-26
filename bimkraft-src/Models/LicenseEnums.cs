using System;

namespace BIMKraft.Models
{
    /// <summary>
    /// License type enumeration
    /// </summary>
    public enum LicenseType
    {
        /// <summary>
        /// 30-day trial license
        /// </summary>
        Trial,

        /// <summary>
        /// Monthly subscription (€12/month)
        /// </summary>
        Monthly,

        /// <summary>
        /// Annual subscription (€99/year)
        /// </summary>
        Yearly
    }

    /// <summary>
    /// License status enumeration
    /// </summary>
    public enum LicenseStatus
    {
        /// <summary>
        /// License is valid and active
        /// </summary>
        Active,

        /// <summary>
        /// Trial period is active
        /// </summary>
        Trial,

        /// <summary>
        /// License has expired but within grace period
        /// </summary>
        GracePeriod,

        /// <summary>
        /// License has expired beyond grace period
        /// </summary>
        Expired,

        /// <summary>
        /// License key is invalid or corrupted
        /// </summary>
        Invalid
    }
}
