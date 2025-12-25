 Ready to code?

 Here is Claude's plan:
╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌
 BIMKraft MSI Installer Package - Implementation Plan

 Overview

 Create an MSI installer package for BIMKraft to distribute on the Autodesk App Store, supporting  
 Revit 2023-2026 with version 0.1.0 as the MVP release.

 Current State

 - ✅ Multi-version build system (2023-2026, .NET 4.8 for 2023-2024, .NET 8.0 for 2025-2026)       
 - ✅ 6 power-themed icons generated and working
 - ✅ Build scripts exist: build_all_versions.bat, build_2023_2024.bat, build_2025_2026.bat        
 - ❌ Missing: Bundle structure, PackageContents.xml, LICENSE, WiX config
 - ❌ Missing: Newtonsoft.Json.dll not copied during build
 - ⚠️ AssemblyInfo.cs version is 1.0.0.0 (needs update to 0.1.0.0)

 Target Structure

 BIMKraft.bundle/
 ├── PackageContents.xml          # Autodesk App Store manifest
 ├── LICENSE.txt                  # Commercial license
 └── Contents/
     ├── 2023/                    # .NET 4.8
     │   ├── BIMKraft.dll
     │   ├── BIMKraft.addin
     │   ├── App.config
     │   ├── Newtonsoft.Json.dll
     │   └── Resources/Icons/*.png (6 files)
     ├── 2024/                    # .NET 4.8
     ├── 2025/                    # .NET 8.0
     └── 2026/                    # .NET 8.0

 Installation Paths:
 - Bundle: C:\ProgramData\Autodesk\ApplicationPlugins\BIMKraft.bundle\
 - .addin files: %AppData%\Autodesk\Revit\Addins\{version}\BIMKraft.addin

 Implementation Steps

 Phase 1: Version & Configuration Updates

 1.1 Update AssemblyInfo.cs → bimkraft-src/Properties/AssemblyInfo.cs
 [assembly: AssemblyTitle("BIM Kraft Tools")]
 [assembly: AssemblyDescription("Advanced BIM tools for Revit: Parameter Management, Workset       
 Automation, Quality Control, Measurements, and Family Tools")]
 [assembly: AssemblyCopyright("Copyright © Maria Simon 2025 - bimkraft.de")]
 [assembly: AssemblyVersion("0.1.0.0")]
 [assembly: AssemblyFileVersion("0.1.0.0")]

 1.2 Update BIMKraft.csproj → bimkraft-src/BIMKraft.csproj

 Add Release configuration:
 <PropertyGroup Condition=" '$(Configuration)' == 'Release' ">
   <DebugType>none</DebugType>
   <Optimize>true</Optimize>
   <DefineConstants>TRACE</DefineConstants>
 </PropertyGroup>

 Update post-build to skip in Release mode:
 <Target Name="PostBuild" AfterTargets="PostBuildEvent" Condition=" '$(Configuration)' == 'Debug'  
 ">
   <!-- Existing post-build commands -->
 </Target>

 1.3 Update .gitignore

 Add:
 # Installer build artifacts
 installer/BIMKraft.bundle/
 installer/*.wixobj
 installer/*.wixpdb
 installer/*.msi
 installer/*.log

 Phase 2: Create Installer Directory Structure

 Create directory: installer/ with subdirectories:
 - installer/addin_templates/

 Phase 3: Create License & Manifest Files

 3.1 Create LICENSE.txt → installer/LICENSE.txt

 BIM Kraft Tools - End User License Agreement (EULA)

 Copyright (c) 2025 Maria Simon - BIMKraft.de
 All rights reserved.

 This software is licensed, not sold. By installing BIM Kraft Tools, you agree
 to the following terms:

 1. LICENSE GRANT
    Maria Simon grants you a non-exclusive, non-transferable license to use
    BIM Kraft Tools for Autodesk Revit in accordance with the terms of this
    agreement and your subscription purchased through the Autodesk App Store.

 2. RESTRICTIONS
    You may not:
    - Reverse engineer, decompile, or disassemble the software
    - Remove or modify any copyright notices
    - Distribute, sublicense, or transfer this software
    - Use the software for any illegal purpose

 3. OWNERSHIP
    BIM Kraft Tools is the intellectual property of Maria Simon. This license
    does not transfer ownership rights to you.

 4. WARRANTY DISCLAIMER
    This software is provided "AS IS" without warranty of any kind, either
    express or implied, including but not limited to warranties of
    merchantability or fitness for a particular purpose.

 5. LIMITATION OF LIABILITY
    In no event shall Maria Simon be liable for any damages arising from the
    use of this software.

 6. TERMINATION
    This license is effective until terminated. Your rights will terminate
    automatically if you fail to comply with these terms.

 7. SUPPORT
    Support is provided at: https://bimkraft.de/support
    Email: contact@bimkraft.de

 For full terms and conditions, visit: https://bimkraft.de/terms

 Maria Simon - BIMKraft.de
 https://bimkraft.de

 3.2 Create LICENSE.rtf → installer/LICENSE.rtf

 Convert LICENSE.txt to RTF format for WiX UI (use WordPad or programmatic conversion)

 3.3 Create PackageContents.xml → installer/PackageContents.xml

 <?xml version="1.0" encoding="utf-8"?>
 <ApplicationPackage
   SchemaVersion="1.0"
   ProductType="Application"
   Name="BIM Kraft Tools"
   Description="Advanced BIM tools for Revit: Parameter Management, Workset Automation, Quality    
 Control, Measurements, and Family Tools"
   Author="Maria Simon"
   AppVersion="0.1.0"
   ProductCode="{B9E8F2C1-6D5A-4E3B-8F7C-1A2B3C4D5E6F}"
   UpgradeCode="{C1D2E3F4-5A6B-7C8D-9E0F-1A2B3C4D5E6F}"
   FriendlyVersion="0.1.0"
   ServerProduct="False"
   Online="True"
   SupportPath="https://bimkraft.de/support"
   HelpFile="https://bimkraft.de/docs">

   <CompanyDetails
     Name="BIMKraft.de"
     Email="contact@bimkraft.de"
     Url="https://bimkraft.de"
     Phone="" />

   <RuntimeRequirements
     OS="Win64"
     Platform="Revit"
     SeriesMin="R2023"
     SeriesMax="R2026" />

   <Components Description="BIM Kraft Tools for Revit 2023-2026">

     <RuntimeRequirements OS="Win64" Platform="Revit" SeriesMin="R2023" SeriesMax="R2023" />       
     <ComponentEntry
       AppName="BIM Kraft Tools"
       Version="0.1.0"
       ModuleName="./Contents/2023/BIMKraft.addin"
       AppDescription="BIM Kraft Tools for Revit 2023"
       LoadOnRevitStartup="True" />

     <RuntimeRequirements OS="Win64" Platform="Revit" SeriesMin="R2024" SeriesMax="R2024" />       
     <ComponentEntry
       AppName="BIM Kraft Tools"
       Version="0.1.0"
       ModuleName="./Contents/2024/BIMKraft.addin"
       AppDescription="BIM Kraft Tools for Revit 2024"
       LoadOnRevitStartup="True" />

     <RuntimeRequirements OS="Win64" Platform="Revit" SeriesMin="R2025" SeriesMax="R2025" />       
     <ComponentEntry
       AppName="BIM Kraft Tools"
       Version="0.1.0"
       ModuleName="./Contents/2025/BIMKraft.addin"
       AppDescription="BIM Kraft Tools for Revit 2025"
       LoadOnRevitStartup="True" />

     <RuntimeRequirements OS="Win64" Platform="Revit" SeriesMin="R2026" SeriesMax="R2026" />       
     <ComponentEntry
       AppName="BIM Kraft Tools"
       Version="0.1.0"
       ModuleName="./Contents/2026/BIMKraft.addin"
       AppDescription="BIM Kraft Tools for Revit 2026"
       LoadOnRevitStartup="True" />

   </Components>
 </ApplicationPackage>

 Phase 4: Create .addin Template Files

 Create 4 .addin files with version-specific assembly paths:

 4.1 installer/addin_templates/BIMKraft_2023.addin
 4.2 installer/addin_templates/BIMKraft_2024.addin
 4.3 installer/addin_templates/BIMKraft_2025.addin
 4.4 installer/addin_templates/BIMKraft_2026.addin

 Template (replace {VERSION} with 2023, 2024, 2025, or 2026):
 <?xml version="1.0" encoding="utf-8"?>
 <RevitAddIns>
   <AddIn Type="Application">
     <Name>BIM Kraft Tools</Name>
     <Assembly>C:\ProgramData\Autodesk\ApplicationPlugins\BIMKraft.bundle\Contents\{VERSION}\BIMKr 
 aft.dll</Assembly>
     <FullClassName>BIMKraft.BIMKraftRibbonApplication</FullClassName>
     <ClientId>b9e8f2c1-6d5a-4e3b-8f7c-1a2b3c4d5e6f</ClientId>
     <VendorId>BIMKRAFT</VendorId>
     <VendorDescription>Maria Simon - BIM Kraft Tools (bimkraft.de)</VendorDescription>
   </AddIn>
 </RevitAddIns>

 Phase 5: Create Build Bundle Script

 5.1 Create build_bundle.bat → installer/build_bundle.bat

 This script:
 - Builds all 4 Revit versions in Release mode
 - Creates bundle directory structure
 - Copies BIMKraft.dll, Newtonsoft.Json.dll, App.config, .addin files, and icons
 - Copies PackageContents.xml and LICENSE.txt to bundle root

 Key operations:
 # Build each version
 msbuild /p:Configuration=Release /p:Platform=x64 /p:RevitVersion=2023
 msbuild /p:Configuration=Release /p:Platform=x64 /p:RevitVersion=2024
 msbuild /p:Configuration=Release /p:Platform=x64 /p:RevitVersion=2025
 msbuild /p:Configuration=Release /p:Platform=x64 /p:RevitVersion=2026

 # For each version:
 # - Copy BIMKraft.dll from bin/x64/Release/{framework}/
 # - Copy Newtonsoft.Json.dll (from same location)
 # - Copy App.config
 # - Copy version-specific .addin from addin_templates/
 # - Copy all 6 icons to Resources/Icons/

 Phase 6: Create WiX Installer Configuration

 6.1 Install WiX Toolset

 Download and install WiX Toolset v3.11 or v4.x from: https://wixtoolset.org/

 6.2 Create BIMKraft.ico → installer/BIMKraft.ico

 Convert one of the PNG icons (e.g., parameter_pro.png) to .ico format using online tool or        
 ImageMagick

 6.3 Create BIMKraft.wxs → installer/BIMKraft.wxs

 WiX source file defining:
 - Product metadata (Name, Version 0.1.0, Manufacturer)
 - Installation directories (ProgramData bundle + AppData .addin files)
 - Component definitions for all 4 Revit versions
 - File associations
 - Upgrade logic (MajorUpgrade)
 - UI configuration (WixUI_InstallDir)

 Critical GUIDs:
 - Product Id: B9E8F2C1-6D5A-4E3B-8F7C-1A2B3C4D5E6F
 - UpgradeCode: C1D2E3F4-5A6B-7C8D-9E0F-1A2B3C4D5E6F (persistent across versions)

 Component structure:
 - LICENSE component
 - PackageContents.xml component
 - Per-version components (2023, 2024, 2025, 2026):
   - BIMKraft.dll
   - BIMKraft.addin (bundle copy)
   - App.config
   - Newtonsoft.Json.dll
   - 6 icon files
 - Per-version .addin files in AppData

 6.4 Create build_msi.bat → installer/build_msi.bat

 # Compile WiX source
 candle.exe -arch x64 -ext WixUIExtension BIMKraft.wxs

 # Link MSI
 light.exe -ext WixUIExtension -cultures:en-us -out BIMKraft_0.1.0.msi BIMKraft.wixobj

 Phase 7: Testing Checklist

 7.1 Bundle Validation
 - Run build_bundle.bat
 - Verify 4 version folders created in BIMKraft.bundle/Contents/
 - Verify Newtonsoft.Json.dll present in each folder (different sizes for .NET 4.8 vs 8.0)
 - Verify all 6 icons in each Resources/Icons/ folder
 - Verify PackageContents.xml and LICENSE.txt in bundle root

 7.2 Manual Bundle Test (before MSI)
 - Copy bundle to C:\ProgramData\Autodesk\ApplicationPlugins\
 - Copy .addin files to %AppData%\Autodesk\Revit\Addins\{version}\
 - Launch each Revit version
 - Verify BIMKraft tab appears with all tools and icons
 - Test one tool per version

 7.3 MSI Build Test
 - Install WiX Toolset
 - Run build_msi.bat
 - Verify BIMKraft_0.1.0.msi created (~3-10 MB)

 7.4 Installation Test (per Revit version)
 - Run MSI installer as Administrator
 - Accept license, complete installation
 - Verify bundle installed to C:\ProgramData\Autodesk\ApplicationPlugins\BIMKraft.bundle\
 - Verify .addin files in %AppData%\Autodesk\Revit\Addins\{version}\
 - Launch Revit, verify tab and all tools functional
 - Test each tool: Parameter Pro, Parameter Transfer Pro, Workset Manager, Warnings Browser Pro,   
 Line Length Calculator, Family Renamer

 7.5 Uninstallation Test
 - Uninstall via Windows Settings > Apps
 - Verify all bundle files removed
 - Verify all .addin files removed
 - Launch Revit, verify BIMKraft tab gone

 Critical Files to Create/Modify

 Files to Modify

 1. bimkraft-src/Properties/AssemblyInfo.cs - Update version to 0.1.0
 2. bimkraft-src/BIMKraft.csproj - Add Release config, conditional post-build
 3. .gitignore - Add installer artifacts

 New Files to Create

 1. installer/LICENSE.txt - Commercial license text
 2. installer/LICENSE.rtf - RTF version for WiX UI
 3. installer/PackageContents.xml - Autodesk App Store manifest
 4. installer/BIMKraft.wxs - WiX installer configuration
 5. installer/BIMKraft.ico - Installer icon
 6. installer/build_bundle.bat - Bundle builder script
 7. installer/build_msi.bat - MSI builder script
 8. installer/addin_templates/BIMKraft_2023.addin - Revit 2023 .addin
 9. installer/addin_templates/BIMKraft_2024.addin - Revit 2024 .addin
 10. installer/addin_templates/BIMKraft_2025.addin - Revit 2025 .addin
 11. installer/addin_templates/BIMKraft_2026.addin - Revit 2026 .addin
 12. installer/README.md - Build instructions

 Key Considerations

 Newtonsoft.Json Dependency

 - Critical: Current build does NOT copy Newtonsoft.Json.dll to output
 - Solution: Build output directory contains it after MSBuild restore
 - Verify: Different DLL sizes for .NET 4.8 (net45) vs .NET 8.0 (net6.0)

 .addin File Assembly Path

 - Old path: Relative to %AppData%
 - New path: Absolute path to bundle:
 C:\ProgramData\Autodesk\ApplicationPlugins\BIMKraft.bundle\Contents\{VERSION}\BIMKraft.dll        
 - Important: Each version needs separate .addin file with version-specific path

 WiX Component GUIDs

 - Each component needs unique GUID
 - Use Guid="*" for auto-generation during development
 - Use fixed GUIDs in production for proper upgrade detection

 Code Signing (Optional but Recommended)

 - MSI installers should be digitally signed
 - Prevents "Unknown Publisher" warnings
 - Required by some enterprise IT policies
 - Cost: $200-400/year for certificate from DigiCert, Sectigo, etc.

 Build Sequence

 1. Update versions (AssemblyInfo.cs, .csproj)
 2. Create license files (LICENSE.txt, LICENSE.rtf)
 3. Create manifest (PackageContents.xml)
 4. Create .addin templates (4 files in addin_templates/)
 5. Create bundle builder (build_bundle.bat)
 6. Test bundle build manually
 7. Create WiX config (BIMKraft.wxs)
 8. Create MSI builder (build_msi.bat)
 9. Build and test MSI on clean VM
 10. Fix any issues, rebuild
 11. Final testing on all Revit versions
 12. Submit to Autodesk App Store

 Next Steps After Implementation

 1. Test installer on clean Windows VM with all Revit versions
 2. Prepare App Store submission materials:
   - Product screenshots (3-5 required)
   - Short description (1-2 sentences)
   - Long description (features, benefits)
   - Pricing model definition
 3. Optional: Obtain code signing certificate
 4. Submit to Autodesk App Store
 5. Monitor support channels (contact@bimkraft.de)
 6. Plan version 0.2.0 with bug fixes

 Success Criteria

 - MSI installer builds without errors
 - Installer runs on all supported Revit versions (2023-2026)
 - All 6 tools functional with icons in each Revit version
 - Bundle structure matches Autodesk App Store requirements
 - PackageContents.xml validates against schema
 - Uninstaller cleanly removes all files
 - No errors in Windows Event Viewer after installation
 - Ready for Autodesk App Store submission
╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌