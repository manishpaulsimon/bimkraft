# BIMKraft Quick Start Guide

Quick commands to build, test, and manage BIMKraft licensing.

## 🚀 One-Click Scripts

### Main Menu (Recommended)

```batch
quick-start.bat
```

**Interactive menu with all common tasks:**
- Build for specific Revit versions
- Generate license keys
- Open relevant folders
- Clean builds

---

## 🔨 Building BIMKraft

### Option 1: Interactive Menu

```batch
build.bat
```

Choose from:
1. Revit 2023 (.NET 4.8)
2. Revit 2024 (.NET 4.8)
3. Revit 2025 (.NET 8.0)
4. Revit 2026 (.NET 8.0)
5. Build All Versions
6. Clean Build (Release)

### Option 2: Direct Build

**Revit 2026 (Debug):**
```batch
cd bimkraft-src
dotnet build /p:RevitVersion=2026 /p:Configuration=Debug
```

**Revit 2026 (Release):**
```batch
cd bimkraft-src
dotnet build /p:RevitVersion=2026 /p:Configuration=Release
```

**Build Output:**
- Debug: `%AppData%\Autodesk\Revit\Addins\2026\BIMKraft.dll`
- Release: `bimkraft-src\bin\Release\net8.0-windows\BIMKraft.dll`

---

## 🔑 License Key Generation

### Generate Customer Keys (Interactive)

```batch
cd tools\LicenseKeyGenerator
generate-license.bat
```

**Prompts you for:**
- Customer email
- License type (Monthly/Yearly)
- Duration (optional)
- Start date (optional)

**Output:**
- Displays license key on screen
- Optional: Save to file

### Generate Test Keys (Batch)

```batch
cd tools\LicenseKeyGenerator
generate-test-keys.bat
```

**Creates 4 test keys:**
- Monthly (1 month)
- Yearly (12 months)
- Expired (for testing)
- Long-term (3 years)

**Output:**
- Saved to `test_licenses_[timestamp].txt`

### Build Generator First Time

```batch
cd tools\LicenseKeyGenerator
build-generator.bat
```

---

## 📁 Folder Structure

```
bimkraft/
├── quick-start.bat           ← Main menu (start here!)
├── build.bat                 ← Build plugin
├── bimkraft-src/            ← Plugin source code
│   └── bin/                 ← Build output
├── tools/
│   └── LicenseKeyGenerator/
│       ├── generate-license.bat      ← Create customer keys
│       ├── generate-test-keys.bat    ← Create test keys
│       ├── build-generator.bat       ← Build generator
│       └── test_licenses_*.txt       ← Generated test keys
└── docs/                    ← Documentation
```

---

## 🧪 Testing Workflow

### 1. Build Plugin

```batch
quick-start.bat
→ Choose option 1 (Build for Revit 2026)
```

### 2. Generate Test Keys

```batch
quick-start.bat
→ Choose option 4 (Generate Test Keys)
```

### 3. Test in Revit

1. Open **Revit 2026**
2. Look for **BIMKraft** ribbon tab
3. **First launch**: Prompt to start 30-day trial
4. Click **"Manage License"** button
5. Paste a test key from `test_licenses_*.txt`
6. Click **"Activate License"**

### 4. Verify Features Work

Test all 6 tools:
- Parameter Pro ✓
- Parameter Transfer Pro ✓
- Workset Manager ✓
- Warnings Browser Pro ✓
- Line Length Calculator ✓
- Family Renamer ✓

---

## 💼 Production Workflow

### Generate Real Customer Key

```batch
cd tools\LicenseKeyGenerator
generate-license.bat
```

Example:
```
Customer Email: customer@example.com
License Type: 2 (Yearly)
Duration: 12 months
Start Date: [press Enter for today]

Generated Key: BIMK-XXXX-XXXX-XXXX-XXXX-XXXX
```

### Email to Customer

```
Subject: Your BIMKraft License Key

Hi [Name],

Thank you for purchasing BIMKraft!

License Key:
BIMK-XXXX-XXXX-XXXX-XXXX-XXXX

Expiry Date: [date]

Activation Steps:
1. Open Autodesk Revit
2. Click "Manage License" in BIMKraft ribbon
3. Paste your license key
4. Click "Activate License"

Support: support@bimkraft.com

Best regards,
BIMKraft Team
```

---

## 🛠️ Troubleshooting

### Build Fails

**Check .NET SDK version:**
```batch
dotnet --version
```
Need: .NET 8.0 SDK for Revit 2025/2026

**Clean and rebuild:**
```batch
quick-start.bat
→ Option 8 (Clean All Builds)
→ Then rebuild
```

### License Generator Fails

**Build generator first:**
```batch
cd tools\LicenseKeyGenerator
build-generator.bat
```

**Check Newtonsoft.Json package:**
```batch
cd tools\LicenseKeyGenerator
dotnet restore
dotnet build
```

### Plugin Not Loading in Revit

**Check DLL location:**
```
%AppData%\Autodesk\Revit\Addins\2026\BIMKraft.dll
```

**Check .addin file:**
```
%AppData%\Autodesk\Revit\Addins\2026\BIMKraft.addin
```

**View Revit logs:**
```
C:\Users\[username]\AppData\Local\Autodesk\Revit\Autodesk Revit 2026\Journals
```

---

## 📊 Quick Reference

| Task | Command |
|------|---------|
| **Main menu** | `quick-start.bat` |
| **Build plugin** | `build.bat` |
| **Generate customer key** | `cd tools\LicenseKeyGenerator && generate-license.bat` |
| **Generate test keys** | `cd tools\LicenseKeyGenerator && generate-test-keys.bat` |
| **Clean builds** | `quick-start.bat` → Option 8 |
| **Open build output** | `quick-start.bat` → Option 7 |

---

## 🎯 First Time Setup

1. **Open command prompt** in project folder
2. **Run:** `quick-start.bat`
3. **Choose Option 5:** Build License Key Generator
4. **Choose Option 1:** Build BIMKraft Plugin
5. **Choose Option 4:** Generate Test Keys
6. **Open Revit 2026** and test

**That's it!** You're ready to develop and test BIMKraft.

---

## 📚 Documentation

- **Full Documentation**: `docs/`
- **User Guide**: `docs/user-guide.md`
- **Stripe Setup**: `docs/stripe-setup-guide.md`
- **License Generator**: `tools/LicenseKeyGenerator/README.md`

---

**Need Help?**
- Check `docs/` folder for detailed guides
- Open an issue on GitHub
- Email: support@bimkraft.com
