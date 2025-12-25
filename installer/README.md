# BIMKraft Installer - Build Instructions

This directory contains all files needed to package BIMKraft as an MSI installer for the Autodesk App Store.

## Overview

- **Version**: 0.1.0 (MVP Release)
- **Supported Revit Versions**: 2023, 2024, 2025, 2026
- **Installer Type**: MSI (Windows Installer)
- **License**: Proprietary/Commercial

## Prerequisites

### Required Software

1. **.NET SDK 8.0** (for building .NET 8.0 versions)
   - Download from: https://dotnet.microsoft.com/download

2. **WiX Toolset v3.11** (for creating MSI installer)
   - Download from: https://wixtoolset.org/releases/
   - Install to default location: `C:\Program Files (x86)\WiX Toolset v3.11\`

3. **Visual Studio 2022** or **MSBuild Tools**
   - Required for building .NET Framework 4.8 and .NET 8.0 projects

### Optional Software

- **Code Signing Certificate** (recommended for production release)
  - Prevents "Unknown Publisher" warnings
  - Required by some enterprise IT policies

## Build Process

### Step 1: Build Bundle

Run the bundle builder script to compile all Revit versions and create the bundle structure:

```batch
cd installer
build_bundle.bat
```

This script will:
- Build BIMKraft for Revit 2023, 2024, 2025, 2026 in Release mode
- Create `BIMKraft.bundle/` directory structure
- Copy all DLLs, icons, config files, and .addin files
- Verify bundle contents

**Expected Output:**
```
BIMKraft.bundle/
├── PackageContents.xml
├── LICENSE.txt
└── Contents/
    ├── 2023/ (BIMKraft.dll, Newtonsoft.Json.dll, icons, etc.)
    ├── 2024/
    ├── 2025/
    └── 2026/
```

### Step 2: Build MSI Installer

After successfully building the bundle, create the MSI installer:

```batch
build_msi.bat
```

This script will:
- Verify bundle exists
- Compile WiX source file (`BIMKraft.wxs`)
- Link MSI installer
- Create `BIMKraft_0.1.0.msi`

**Expected Output:**
- `BIMKraft_0.1.0.msi` (~3-10 MB)

## File Descriptions

### Configuration Files

- **PackageContents.xml** - Autodesk App Store manifest defining version support
- **BIMKraft.wxs** - WiX installer configuration with component definitions
- **LICENSE.txt** - Commercial license agreement (plain text)
- **LICENSE.rtf** - License agreement for WiX UI (Rich Text Format)

### Build Scripts

- **build_bundle.bat** - Builds all Revit versions and creates bundle structure
- **build_msi.bat** - Compiles WiX source and creates MSI installer

### .addin Template Files

Located in `addin_templates/`:
- **BIMKraft_2023.addin** - Revit 2023 add-in manifest
- **BIMKraft_2024.addin** - Revit 2024 add-in manifest
- **BIMKraft_2025.addin** - Revit 2025 add-in manifest
- **BIMKraft_2026.addin** - Revit 2026 add-in manifest

Each .addin file points to the bundle installation path:
`C:\ProgramData\Autodesk\ApplicationPlugins\BIMKraft.bundle\Contents\{VERSION}\`

## Installation Paths

The MSI installer will deploy files to:

### Bundle Location (ProgramData)
```
C:\ProgramData\Autodesk\ApplicationPlugins\BIMKraft.bundle\
```

### .addin Files (AppData - per user)
```
%AppData%\Autodesk\Revit\Addins\2023\BIMKraft.addin
%AppData%\Autodesk\Revit\Addins\2024\BIMKraft.addin
%AppData%\Autodesk\Revit\Addins\2025\BIMKraft.addin
%AppData%\Autodesk\Revit\Addins\2026\BIMKraft.addin
```

## Testing

### Pre-Installation Testing

Before building the MSI, you can manually test the bundle:

1. Copy `BIMKraft.bundle/` to `C:\ProgramData\Autodesk\ApplicationPlugins\`
2. Copy `.addin` files to respective Revit Addins folders
3. Launch Revit and verify BIMKraft tab appears

### MSI Installation Testing

Test the installer on a clean Windows VM with Revit installed:

1. Run `BIMKraft_0.1.0.msi` as Administrator
2. Accept license agreement
3. Complete installation
4. Launch each Revit version (2023-2026)
5. Verify BIMKraft tab with all tools and icons
6. Test each tool:
   - Parameter Pro
   - Parameter Transfer Pro
   - Workset Manager
   - Warnings Browser Pro
   - Line Length Calculator
   - Family Renamer

### Uninstallation Testing

1. Uninstall via Windows Settings > Apps & Features
2. Verify all files removed from:
   - `C:\ProgramData\Autodesk\ApplicationPlugins\BIMKraft.bundle\`
   - `%AppData%\Autodesk\Revit\Addins\{version}\BIMKraft.addin`
3. Launch Revit, verify BIMKraft tab is gone

## Troubleshooting

### Build Errors

**Problem:** "dotnet: command not found"
- **Solution:** Install .NET SDK 8.0 and ensure it's in PATH

**Problem:** "WiX Toolset not found"
- **Solution:** Install WiX Toolset v3.11 or update `WIX_BIN` path in `build_msi.bat`

**Problem:** "Newtonsoft.Json.dll not found"
- **Solution:** Ensure NuGet restore completed successfully. Check `bin/x64/Release/` folders.

**Problem:** Build fails for Revit 2025/2026
- **Solution:** .NET 8.0 SDK required for these versions. Verify installation.

### Installation Errors

**Problem:** "Unknown Publisher" warning
- **Solution:** Sign MSI with code signing certificate (optional but recommended)

**Problem:** BIMKraft tab doesn't appear in Revit
- **Solution:**
  - Check Windows Event Viewer for errors
  - Verify .addin file installed correctly
  - Verify bundle DLL path is correct

**Problem:** Icons not showing
- **Solution:** Verify icon files exist in `Resources/Icons/` folder in bundle

## Code Signing (Optional)

For production release, sign the MSI installer:

```batch
signtool sign /f certificate.pfx /p password /t http://timestamp.digicert.com BIMKraft_0.1.0.msi
```

**Benefits:**
- No "Unknown Publisher" warnings
- Professional appearance
- Required by some enterprise IT policies

**Cost:** ~$200-400/year for code signing certificate from DigiCert, Sectigo, etc.

## Autodesk App Store Submission

### Preparation Checklist

- [ ] MSI installer tested on all Revit versions (2023-2026)
- [ ] All 6 tools functional with icons
- [ ] Uninstaller tested
- [ ] No errors in Windows Event Viewer
- [ ] License agreement reviewed
- [ ] Support email active: contact@bimkraft.de
- [ ] Support website active: https://bimkraft.de/support

### Required Materials

1. **MSI Installer**: `BIMKraft_0.1.0.msi`
2. **Product Screenshots**: 3-5 screenshots showing tools in action
3. **Short Description**: 1-2 sentences
4. **Long Description**: Features, benefits, use cases
5. **Pricing Model**: Free trial, subscription, perpetual license, etc.
6. **Support Contact**: contact@bimkraft.de
7. **Product Icon**: High-resolution icon for App Store listing

### Submission Process

1. Create Autodesk App Store developer account
2. Upload MSI installer
3. Provide product metadata
4. Submit for review
5. Monitor submission status

## Version Updates

To create a new version (e.g., 0.2.0):

1. Update version in:
   - `../bimkraft-src/Properties/AssemblyInfo.cs`
   - `PackageContents.xml` (AppVersion, FriendlyVersion)
   - `BIMKraft.wxs` (Product Version)
   - `build_msi.bat` (output filename)

2. Rebuild bundle and MSI
3. Test on all Revit versions
4. Submit update to App Store

## Support

For questions or issues:
- Email: contact@bimkraft.de
- Website: https://bimkraft.de
- Support: https://bimkraft.de/support

## License

Copyright (c) 2025 Maria Simon - BIMKraft.de
All rights reserved.

See LICENSE.txt for full terms.
