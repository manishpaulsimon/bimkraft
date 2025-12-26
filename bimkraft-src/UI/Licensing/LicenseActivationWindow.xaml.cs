using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using BIMKraft.Models;
using BIMKraft.Services;

namespace BIMKraft.UI.Licensing
{
    /// <summary>
    /// License activation window for BIMKraft
    /// </summary>
    public partial class LicenseActivationWindow : Window
    {
        private LicenseInfo _currentLicense;

        public LicenseActivationWindow()
        {
            InitializeComponent();
            Loaded += LicenseActivationWindow_Loaded;
        }

        /// <summary>
        /// Initialize window on load
        /// </summary>
        private void LicenseActivationWindow_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshLicenseStatus();
        }

        /// <summary>
        /// Refresh and display current license status
        /// </summary>
        private void RefreshLicenseStatus()
        {
            _currentLicense = LicenseManager.ValidateLicense();

            // Update status indicator and text
            switch (_currentLicense.Status)
            {
                case LicenseStatus.Active:
                    StatusIndicator.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981")); // Green
                    StatusText.Text = "Active";
                    StatusText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981"));
                    DeactivateButton.Visibility = Visibility.Visible;
                    StartTrialButton.IsEnabled = false;
                    break;

                case LicenseStatus.Trial:
                    StatusIndicator.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3B82F6")); // Blue
                    StatusText.Text = "Trial Active";
                    StatusText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3B82F6"));
                    DeactivateButton.Visibility = Visibility.Collapsed;
                    StartTrialButton.IsEnabled = false;
                    break;

                case LicenseStatus.GracePeriod:
                    StatusIndicator.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F59E0B")); // Orange
                    StatusText.Text = "Grace Period";
                    StatusText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F59E0B"));
                    DeactivateButton.Visibility = Visibility.Visible;
                    StartTrialButton.IsEnabled = false;
                    break;

                case LicenseStatus.Expired:
                    StatusIndicator.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444")); // Red
                    StatusText.Text = "Expired";
                    StatusText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444"));
                    DeactivateButton.Visibility = Visibility.Visible;
                    StartTrialButton.IsEnabled = false;
                    break;

                case LicenseStatus.Invalid:
                    StatusIndicator.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6B7280")); // Gray
                    StatusText.Text = "No Active License";
                    StatusText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6B7280"));
                    DeactivateButton.Visibility = Visibility.Collapsed;
                    StartTrialButton.IsEnabled = true;
                    break;
            }

            // Update license type
            LicenseTypeText.Text = _currentLicense.Type switch
            {
                LicenseType.Trial => "Free Trial (30 days)",
                LicenseType.Monthly => "Monthly Subscription (€12/month)",
                LicenseType.Yearly => "Annual Subscription (€99/year)",
                _ => "-"
            };

            // Update remaining days
            if (_currentLicense.Status == LicenseStatus.Invalid)
            {
                RemainingDaysText.Text = "-";
            }
            else if (_currentLicense.Status == LicenseStatus.Expired)
            {
                RemainingDaysText.Text = "Expired";
                RemainingDaysText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444"));
            }
            else
            {
                RemainingDaysText.Text = $"{_currentLicense.RemainingDays} days";

                // Color code based on remaining days
                if (_currentLicense.RemainingDays <= 7)
                {
                    RemainingDaysText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444")); // Red
                }
                else if (_currentLicense.RemainingDays <= 14)
                {
                    RemainingDaysText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F59E0B")); // Orange
                }
                else
                {
                    RemainingDaysText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981")); // Green
                }
            }
        }

        /// <summary>
        /// Handle license key text changes (basic validation)
        /// </summary>
        private void LicenseKeyTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            string key = LicenseKeyTextBox.Text.Trim();

            // Clear validation message
            ValidationMessage.Visibility = Visibility.Collapsed;

            // Enable activate button if key format looks valid
            // Expected format: BIMK-XXXX-XXXX-XXXX-XXXX-XXXX (29 chars with hyphens)
            if (key.StartsWith("BIMK-") && key.Length >= 24)
            {
                ActivateButton.IsEnabled = true;
            }
            else
            {
                ActivateButton.IsEnabled = false;
            }
        }

        /// <summary>
        /// Activate license button click
        /// </summary>
        private void ActivateButton_Click(object sender, RoutedEventArgs e)
        {
            string licenseKey = LicenseKeyTextBox.Text.Trim();

            try
            {
                // Show loading state
                ActivateButton.IsEnabled = false;
                ActivateButton.Content = "Activating...";

                // Attempt to activate license
                LicenseInfo license = LicenseManager.ActivateLicense(licenseKey);

                // Success
                MessageBox.Show(
                    $"License activated successfully!\n\n{license.GetStatusMessage()}",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                // Refresh status display
                RefreshLicenseStatus();

                // Clear license key field
                LicenseKeyTextBox.Text = "BIMK-XXXX-XXXX-XXXX-XXXX-XXXX";
            }
            catch (Exception ex)
            {
                // Show error
                ValidationMessage.Text = ex.Message;
                ValidationMessage.Visibility = Visibility.Visible;

                MessageBox.Show(
                    $"License activation failed:\n\n{ex.Message}",
                    "Activation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
            finally
            {
                // Reset button
                ActivateButton.Content = "Activate License";
                ActivateButton.IsEnabled = true;
            }
        }

        /// <summary>
        /// Start trial button click
        /// </summary>
        private void StartTrialButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Confirm with user
                MessageBoxResult result = MessageBox.Show(
                    "Start a free 30-day trial with full access to all BIMKraft features?\n\n" +
                    "The trial period begins immediately and cannot be reset.",
                    "Start Free Trial",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );

                if (result != MessageBoxResult.Yes)
                {
                    return;
                }

                // Show loading state
                StartTrialButton.IsEnabled = false;
                StartTrialButton.Content = "Starting Trial...";

                // Start trial
                TrialInfo trial = LicenseManager.StartTrial();

                // Success
                MessageBox.Show(
                    $"Trial started successfully!\n\n" +
                    $"Your 30-day free trial has begun.\n" +
                    $"Trial ends on: {trial.TrialEndDate:yyyy-MM-dd}\n\n" +
                    $"Enjoy full access to all BIMKraft features!",
                    "Trial Started",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                // Refresh status display
                RefreshLicenseStatus();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to start trial:\n\n{ex.Message}",
                    "Trial Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
            finally
            {
                // Reset button
                StartTrialButton.Content = "Start 30-Day Free Trial";
                StartTrialButton.IsEnabled = true;
            }
        }

        /// <summary>
        /// Purchase button click - open pricing page
        /// </summary>
        private void PurchaseButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Open BIMKraft pricing page in default browser
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://bimkraft.com/pricing",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Could not open pricing page:\n\n{ex.Message}\n\n" +
                    "Please visit: https://bimkraft.com/pricing",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }
        }

        /// <summary>
        /// Deactivate license button click
        /// </summary>
        private void DeactivateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Confirm with user
                MessageBoxResult result = MessageBox.Show(
                    "Are you sure you want to deactivate your license?\n\n" +
                    "You will need to re-enter your license key to reactivate.",
                    "Deactivate License",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning
                );

                if (result != MessageBoxResult.Yes)
                {
                    return;
                }

                // Deactivate license
                LicenseManager.DeactivateLicense();

                MessageBox.Show(
                    "License deactivated successfully.",
                    "Deactivated",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                // Refresh status display
                RefreshLicenseStatus();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to deactivate license:\n\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        /// <summary>
        /// Close button click
        /// </summary>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
