using System;
using System.Collections.Generic;

namespace BIMKraft.Models
{
    /// <summary>
    /// Trial period tracking information
    /// </summary>
    public class TrialInfo
    {
        /// <summary>
        /// Date when BIMKraft was first launched (trial start)
        /// </summary>
        public DateTime FirstLaunchDate { get; set; }

        /// <summary>
        /// Date when trial period started
        /// </summary>
        public DateTime TrialStartDate { get; set; }

        /// <summary>
        /// Date when trial period ends (30 days from start)
        /// </summary>
        public DateTime TrialEndDate { get; set; }

        /// <summary>
        /// List of notification milestones that have been sent
        /// (e.g., "30d", "14d", "7d", "3d", "1d")
        /// </summary>
        public List<string> NotificationsSent { get; set; }

        /// <summary>
        /// SHA-256 hash for integrity verification (prevents tampering)
        /// </summary>
        public string Hash { get; set; }

        public TrialInfo()
        {
            NotificationsSent = new List<string>();
        }

        /// <summary>
        /// Check if trial is still active
        /// </summary>
        public bool IsActive()
        {
            return DateTime.UtcNow < TrialEndDate;
        }

        /// <summary>
        /// Get number of days remaining in trial
        /// </summary>
        public int GetRemainingDays()
        {
            var remaining = (TrialEndDate - DateTime.UtcNow).TotalDays;
            return Math.Max(0, (int)Math.Ceiling(remaining));
        }

        /// <summary>
        /// Check if notification should be shown for this milestone
        /// </summary>
        public bool ShouldShowNotification(int daysRemaining)
        {
            string milestone = $"{daysRemaining}d";

            // Only show notifications at these milestones
            if (daysRemaining != 30 && daysRemaining != 14 &&
                daysRemaining != 7 && daysRemaining != 3 && daysRemaining != 1)
            {
                return false;
            }

            // Check if we've already shown this notification
            return !NotificationsSent.Contains(milestone);
        }

        /// <summary>
        /// Mark notification as sent for this milestone
        /// </summary>
        public void MarkNotificationSent(int daysRemaining)
        {
            string milestone = $"{daysRemaining}d";
            if (!NotificationsSent.Contains(milestone))
            {
                NotificationsSent.Add(milestone);
            }
        }
    }
}
