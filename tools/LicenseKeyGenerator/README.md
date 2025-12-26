# BIMKraft License Key Generator

A command-line tool to generate encrypted license keys for BIMKraft customers.

## Features

- **Interactive Mode** - Step-by-step key generation
- **Batch Mode** - Generate multiple test keys at once
- **AES-256 Encryption** - Secure license keys with HMAC-SHA256 signatures
- **Clipboard Support** - Automatically copy keys to clipboard (Windows)
- **File Export** - Save license keys with customer details

## Installation

### Prerequisites

- .NET 8.0 SDK installed

### Build the Generator

```bash
cd tools/LicenseKeyGenerator
dotnet build -c Release
```

The compiled executable will be in: `bin/Release/net8.0/LicenseKeyGenerator.exe`

## Usage

### Interactive Mode

Run the generator without arguments:

```bash
dotnet run
# or
LicenseKeyGenerator.exe
```

You'll be prompted for:
1. **Customer Email** - User's email address
2. **License Type** - Monthly (€12/month) or Yearly (€99/year)
3. **Duration** - Optional custom duration in months
4. **Start Date** - Optional custom start date (default: today)

The generator will:
- Create an encrypted license key
- Display license details
- Copy key to clipboard (Windows)
- Optionally save to a text file

### Batch Mode (Generate Test Keys)

Generate multiple test keys for testing:

```bash
dotnet run -- --batch
# or
LicenseKeyGenerator.exe --batch
```

This creates 4 test license keys:
- **Monthly License** - Valid for 1 month
- **Yearly License** - Valid for 12 months
- **Expired License** - Already expired (for testing)
- **Long-term License** - Valid for 3 years

All keys are saved to `test_licenses_YYYYMMDD_HHMMSS.txt`

## Examples

### Example 1: Generate Monthly License

```
Customer Email: customer@example.com
License Type: 1 (Monthly)
Duration: 1 month
Start Date: [today]

Generated Key: BIMK-A3F7-9K2P-X8M4-Q5N6-J1R8
```

### Example 2: Generate Yearly License Starting in Future

```
Customer Email: customer@example.com
License Type: 2 (Yearly)
Duration: 12 months
Start Date: 2025-01-01

Generated Key: BIMK-B8K3-L4M2-N7P9-R5T6-W2X1
```

### Example 3: Generate Custom 6-Month License

```
Customer Email: customer@example.com
License Type: 2 (Yearly)
Duration: 6 months
Start Date: [today]

Generated Key: BIMK-C9M5-N2Q8-P4R7-S1V3-X6Y2
```

## License Key Format

License keys follow this format:

```
BIMK-XXXX-XXXX-XXXX-XXXX-XXXX
```

- **Prefix**: `BIMK` (BIMKraft identifier)
- **Length**: 29 characters (24 data + 5 hyphens)
- **Encoding**: Base32 (no ambiguous characters like O/0, I/1)
- **Encryption**: AES-256 with HMAC-SHA256 signature

### What's Encrypted in the Key

Each license key contains:
- License ID (unique GUID)
- Customer email
- License type (Monthly/Yearly)
- Issue date (UTC)
- Expiry date (UTC)
- Feature list (currently "all")

## Delivering Keys to Customers

### Option 1: Manual Delivery (Current)

1. Customer purchases via PayPal/Stripe/bank transfer
2. You receive notification
3. Run this generator with customer details
4. Email the license key to customer

**Email Template:**

```
Subject: Your BIMKraft License Key

Hi [Name],

Thank you for purchasing BIMKraft!

Your License Details:
- Type: [Monthly/Yearly]
- Expires: [YYYY-MM-DD]

License Key:
BIMK-XXXX-XXXX-XXXX-XXXX-XXXX

Activation Instructions:
1. Open Autodesk Revit
2. Click the "Manage License" button in the BIMKraft ribbon tab
3. Enter your license key in the activation window
4. Click "Activate License"

Need help? Visit https://bimkraft.com/support

Best regards,
BIMKraft Team
```

### Option 2: Automated Delivery (Future)

See the Stripe integration guide: `docs/stripe-setup-guide.md`

## Security Notes

### Key Security

- License keys are encrypted with AES-256
- HMAC-SHA256 signature prevents tampering
- Encryption key is obfuscated in source code
- Trial data uses SHA-256 hash integrity checks

### Known Limitations (Phase 1 - Offline Licensing)

- **Shareable Keys**: Offline keys can be shared between users
- **No Activation Limits**: Single key can activate unlimited machines
- **Reversible**: Encryption key can be extracted with reverse engineering

These are acceptable risks for Phase 1 MVP. Phase 2 (online validation) adds:
- Machine activation limits (max 3 devices per license)
- Remote license revocation
- Online validation with hardware fingerprinting

### Security Recommendations

1. **Don't commit encryption keys to Git** - Already obfuscated but avoid changes
2. **Code obfuscation** - Use ConfuserEx before release builds
3. **Monitor for piracy** - Google search for leaked keys periodically
4. **Fast response** - Issue new keys quickly if leaked
5. **Focus on value** - Most users will pay if they find value

## Troubleshooting

### Error: "Failed to encrypt license"

- Check that `LicenseCrypto.cs`, `LicenseInfo.cs`, and `LicenseEnums.cs` are accessible
- Ensure Newtonsoft.Json package is installed

### Error: "Invalid date format"

- Use `yyyy-MM-dd` format for dates (e.g., `2025-12-26`)

### License key doesn't work in BIMKraft

1. Verify key was copied correctly (check for spaces/line breaks)
2. Check expiry date hasn't passed
3. Ensure BIMKraft plugin has licensing system integrated
4. Test with a freshly generated key

## Batch Processing

For processing multiple licenses from a CSV/Excel:

```bash
# Example: Read from CSV and generate keys
# You can extend Program.cs to support CSV input
```

## Changelog

### v1.0 (2025-12-26)
- Initial release
- Interactive mode
- Batch test key generation
- Clipboard support
- File export
- AES-256 + HMAC-SHA256 encryption

## Support

For issues or questions:
- Email: support@bimkraft.com
- Docs: https://bimkraft.com/docs
