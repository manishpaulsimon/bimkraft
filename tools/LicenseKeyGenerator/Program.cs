using System;
using System.Collections.Generic;
using BIMKraft.Models;
using BIMKraft.Services;

namespace LicenseKeyGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("╔════════════════════════════════════════════════════════╗");
            Console.WriteLine("║       BIMKraft License Key Generator v1.0             ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════╝");
            Console.WriteLine();

            if (args.Length > 0 && args[0] == "--batch")
            {
                // Batch mode - generate multiple test keys
                GenerateBatchKeys();
                return;
            }

            // Interactive mode
            while (true)
            {
                Console.WriteLine("\n┌─────────────────────────────────────────┐");
                Console.WriteLine("│  Generate New License Key               │");
                Console.WriteLine("└─────────────────────────────────────────┘\n");

                try
                {
                    // Get customer email
                    Console.Write("Customer Email: ");
                    string email = Console.ReadLine()?.Trim() ?? "";
                    if (string.IsNullOrEmpty(email))
                    {
                        Console.WriteLine("❌ Email is required!");
                        continue;
                    }

                    // Get license type
                    Console.WriteLine("\nLicense Type:");
                    Console.WriteLine("  1. Monthly (€12/month)");
                    Console.WriteLine("  2. Yearly (€99/year)");
                    Console.Write("\nChoice (1 or 2): ");
                    string typeChoice = Console.ReadLine()?.Trim() ?? "";

                    LicenseType licenseType;
                    int durationMonths;

                    switch (typeChoice)
                    {
                        case "1":
                            licenseType = LicenseType.Monthly;
                            durationMonths = 1;
                            break;
                        case "2":
                            licenseType = LicenseType.Yearly;
                            durationMonths = 12;
                            break;
                        default:
                            Console.WriteLine("❌ Invalid choice!");
                            continue;
                    }

                    // Get custom duration (optional)
                    Console.Write($"\nDuration in months (default: {durationMonths}): ");
                    string durationInput = Console.ReadLine()?.Trim() ?? "";
                    if (!string.IsNullOrEmpty(durationInput) && int.TryParse(durationInput, out int customDuration))
                    {
                        durationMonths = customDuration;
                    }

                    // Get start date (optional)
                    Console.Write("\nStart date (yyyy-MM-dd) or press Enter for today: ");
                    string startDateInput = Console.ReadLine()?.Trim() ?? "";
                    DateTime issueDate = DateTime.UtcNow;
                    if (!string.IsNullOrEmpty(startDateInput) && DateTime.TryParse(startDateInput, out DateTime customStart))
                    {
                        issueDate = customStart.ToUniversalTime();
                    }

                    // Calculate expiry
                    DateTime expiryDate = issueDate.AddMonths(durationMonths);

                    // Create license info
                    LicenseInfo licenseInfo = new LicenseInfo
                    {
                        LicenseId = Guid.NewGuid().ToString(),
                        Email = email,
                        Type = licenseType,
                        IssuedDate = issueDate,
                        ExpiryDate = expiryDate,
                        Status = LicenseStatus.Active,
                        Features = new List<string> { "all" }
                    };

                    // Generate license key
                    string licenseKey = LicenseCrypto.EncryptLicense(licenseInfo);

                    // Display result
                    Console.WriteLine("\n╔════════════════════════════════════════════════════════╗");
                    Console.WriteLine("║           LICENSE KEY GENERATED SUCCESSFULLY           ║");
                    Console.WriteLine("╚════════════════════════════════════════════════════════╝");
                    Console.WriteLine();
                    Console.WriteLine($"Customer:      {email}");
                    Console.WriteLine($"License Type:  {licenseType}");
                    Console.WriteLine($"Issue Date:    {issueDate:yyyy-MM-dd HH:mm:ss} UTC");
                    Console.WriteLine($"Expiry Date:   {expiryDate:yyyy-MM-dd HH:mm:ss} UTC");
                    Console.WriteLine($"Duration:      {durationMonths} month{(durationMonths != 1 ? "s" : "")}");
                    Console.WriteLine();
                    Console.WriteLine("┌─────────────────────────────────────────┐");
                    Console.WriteLine("│           LICENSE KEY                   │");
                    Console.WriteLine("└─────────────────────────────────────────┘");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"\n  {licenseKey}\n");
                    Console.ResetColor();
                    Console.WriteLine("────────────────────────────────────────────");
                    Console.WriteLine();

                    // Note: License key can be manually copied from above

                    // Save to file
                    Console.Write("\nSave to file? (y/n): ");
                    if (Console.ReadLine()?.Trim().ToLower() == "y")
                    {
                        string filename = $"license_{email.Replace("@", "_")}_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                        System.IO.File.WriteAllText(filename,
                            $"BIMKraft License Key\n" +
                            $"====================\n\n" +
                            $"Customer: {email}\n" +
                            $"Type: {licenseType}\n" +
                            $"Issued: {issueDate:yyyy-MM-dd}\n" +
                            $"Expires: {expiryDate:yyyy-MM-dd}\n\n" +
                            $"License Key:\n{licenseKey}\n\n" +
                            $"Instructions:\n" +
                            $"1. Open Revit\n" +
                            $"2. Click the 'Manage License' button in the BIMKraft ribbon\n" +
                            $"3. Enter the license key above\n" +
                            $"4. Click 'Activate License'\n"
                        );
                        Console.WriteLine($"✓ Saved to: {filename}");
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\n❌ Error: {ex.Message}");
                    Console.ResetColor();
                }

                // Continue or exit
                Console.Write("\nGenerate another key? (y/n): ");
                if (Console.ReadLine()?.Trim().ToLower() != "y")
                {
                    break;
                }
            }

            Console.WriteLine("\nThank you for using BIMKraft License Key Generator!");
        }

        static void GenerateBatchKeys()
        {
            Console.WriteLine("Generating test license keys...\n");

            var testKeys = new[]
            {
                new { Email = "test.monthly@example.com", Type = LicenseType.Monthly, Months = 1 },
                new { Email = "test.yearly@example.com", Type = LicenseType.Yearly, Months = 12 },
                new { Email = "test.expired@example.com", Type = LicenseType.Monthly, Months = -1 }, // Expired
                new { Email = "test.longterm@example.com", Type = LicenseType.Yearly, Months = 36 }, // 3 years
            };

            foreach (var testKey in testKeys)
            {
                DateTime issueDate = DateTime.UtcNow;
                DateTime expiryDate = issueDate.AddMonths(testKey.Months);

                LicenseInfo licenseInfo = new LicenseInfo
                {
                    LicenseId = Guid.NewGuid().ToString(),
                    Email = testKey.Email,
                    Type = testKey.Type,
                    IssuedDate = issueDate,
                    ExpiryDate = expiryDate,
                    Status = LicenseStatus.Active,
                    Features = new List<string> { "all" }
                };

                string licenseKey = LicenseCrypto.EncryptLicense(licenseInfo);

                Console.WriteLine($"┌─────────────────────────────────────────┐");
                Console.WriteLine($"│ {testKey.Email,-39} │");
                Console.WriteLine($"└─────────────────────────────────────────┘");
                Console.WriteLine($"Type:    {testKey.Type}");
                Console.WriteLine($"Expires: {expiryDate:yyyy-MM-dd}");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"Key:     {licenseKey}");
                Console.ResetColor();
                Console.WriteLine();
            }

            // Save all to file
            string batchFile = $"test_licenses_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            using (var writer = new System.IO.StreamWriter(batchFile))
            {
                writer.WriteLine("BIMKraft Test License Keys");
                writer.WriteLine("=========================");
                writer.WriteLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                writer.WriteLine();

                foreach (var testKey in testKeys)
                {
                    DateTime issueDate = DateTime.UtcNow;
                    DateTime expiryDate = issueDate.AddMonths(testKey.Months);

                    LicenseInfo licenseInfo = new LicenseInfo
                    {
                        LicenseId = Guid.NewGuid().ToString(),
                        Email = testKey.Email,
                        Type = testKey.Type,
                        IssuedDate = issueDate,
                        ExpiryDate = expiryDate,
                        Status = LicenseStatus.Active,
                        Features = new List<string> { "all" }
                    };

                    string licenseKey = LicenseCrypto.EncryptLicense(licenseInfo);

                    writer.WriteLine($"Email:   {testKey.Email}");
                    writer.WriteLine($"Type:    {testKey.Type}");
                    writer.WriteLine($"Expires: {expiryDate:yyyy-MM-dd}");
                    writer.WriteLine($"Key:     {licenseKey}");
                    writer.WriteLine();
                }
            }

            Console.WriteLine($"✓ All test keys saved to: {batchFile}");
        }
    }
}
