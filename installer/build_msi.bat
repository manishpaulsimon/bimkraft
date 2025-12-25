@echo off
echo ================================================================
echo BIMKraft MSI Installer Builder
echo ================================================================
echo.

REM Check if bundle exists
if not exist BIMKraft.bundle (
    echo ERROR: Bundle not found. Run build_bundle.bat first.
    pause
    exit /b 1
)

REM Check for WiX Toolset v6
set "WIX_BIN=C:\Program Files\WiX Toolset v6.0\bin"
if not exist "%WIX_BIN%\wix.exe" (
    echo ERROR: WiX Toolset v6.0 not found at: %WIX_BIN%
    echo.
    echo Please install WiX Toolset from:
    echo https://github.com/wixtoolset/wix/releases
    echo.
    echo Or update WIX_BIN path in this script if installed elsewhere.
    pause
    exit /b 1
)

echo Using WiX Toolset v6.0 from: %WIX_BIN%
echo.

REM Clean previous build artifacts
if exist *.wixobj del /q *.wixobj
if exist *.wixpdb del /q *.wixpdb
if exist *.log del /q *.log
if exist BIMKraft_0.1.0.msi del /q BIMKraft_0.1.0.msi

echo Building MSI installer with WiX v6 (Revit 2026 only)...
"%WIX_BIN%\wix.exe" build -arch x64 -ext WixToolset.UI.wixext BIMKraft_2026_only.wxs -out BIMKraft_0.1.0_2026.msi
if %errorlevel% neq 0 (
    echo ERROR: WiX build failed
    pause
    exit /b %errorlevel%
)

echo.
echo ================================================================
echo MSI installer created successfully!
echo ================================================================
echo Output: BIMKraft_0.1.0_2026.msi
echo.
echo File information:
dir BIMKraft_0.1.0_2026.msi | find "BIMKraft_0.1.0_2026.msi"
echo.
echo Next steps:
echo 1. Test installer on clean VM with Revit 2023-2026
echo 2. Verify all tools work correctly
echo 3. Test uninstaller
echo 4. Optional: Sign MSI with code signing certificate
echo 5. Submit to Autodesk App Store
echo.
pause
