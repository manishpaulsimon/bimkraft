@echo off
echo ================================================================
echo BIMKraft MSI Installer Builder
echo ================================================================
echo./

REM Check if bundle exists
if not exist BIMKraft.bundle (
    echo ERROR: Bundle not found. Run build_bundle.bat first.
    pause
    exit /b 1
)

REM Check for WiX Toolset
set WIX_BIN=C:\Program Files (x86)\WiX Toolset v3.11\bin
if not exist "%WIX_BIN%\candle.exe" (
    echo ERROR: WiX Toolset v3.11 not found at: %WIX_BIN%
    echo.
    echo Please install WiX Toolset v3.11 from:
    echo https://wixtoolset.org/releases
    echo.
    echo Or update WIX_BIN path in this script if installed elsewhere.
    pause
    exit /b 1
)

echo Using WiX Toolset from: %WIX_BIN%
echo.

REM Clean previous build artifacts
if exist *.wixobj del /q *.wixobj
if exist *.wixpdb del /q *.wixpdb
if exist *.log del /q *.log

echo Compiling WiX source...
"%WIX_BIN%\candle.exe" -arch x64 -ext WixUIExtension BIMKraft.wxs
if %errorlevel% neq 0 (
    echo ERROR: WiX compilation failed
    pause
    exit /b %errorlevel%
)

echo.
echo Linking MSI installer...
"%WIX_BIN%\light.exe" -ext WixUIExtension -cultures:en-us -out BIMKraft_0.1.0.msi BIMKraft.wixobj
if %errorlevel% neq 0 (
    echo ERROR: WiX linking failed
    pause
    exit /b %errorlevel%
)

echo.
echo ================================================================
echo MSI installer created successfully!
echo ================================================================
echo Output: BIMKraft_0.1.0.msi
echo.
echo File information:
dir BIMKraft_0.1.0.msi | find "BIMKraft_0.1.0.msi"
echo.
echo Next steps:
echo 1. Test installer on clean VM with Revit 2023-2026
echo 2. Verify all tools work correctly
echo 3. Test uninstaller
echo 4. Optional: Sign MSI with code signing certificate
echo 5. Submit to Autodesk App Store
echo.
pause
