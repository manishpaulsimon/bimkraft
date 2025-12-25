@echo off
setlocal enabledelayedexpansion

echo ================================================================
echo BIMKraft Bundle Builder for Autodesk App Store
echo ================================================================
echo.

set MSBUILD="C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
set PROJECT=..\bimkraft-src\BIMKraft.csproj
set BUNDLE_ROOT=BIMKraft.bundle

REM Clean existing bundle
if exist "%BUNDLE_ROOT%" (
    echo Cleaning existing bundle directory...
    rmdir /s /q "%BUNDLE_ROOT%"
)

echo Creating bundle directory structure...
mkdir "%BUNDLE_ROOT%\Contents\2023\Resources\Icons"
mkdir "%BUNDLE_ROOT%\Contents\2024\Resources\Icons"
mkdir "%BUNDLE_ROOT%\Contents\2025\Resources\Icons"
mkdir "%BUNDLE_ROOT%\Contents\2026\Resources\Icons"
echo.

REM Build for each Revit version in RELEASE configuration
for %%V in (2023 2024 2025 2026) do (
    echo ================================================================
    echo Building BIMKraft for Revit %%V (Release)...
    echo ================================================================

    REM Restore NuGet packages
    echo Restoring NuGet packages for Revit %%V...
    %MSBUILD% %PROJECT% /t:Restore /p:Configuration=Release /p:Platform=x64 /p:RevitVersion=%%V
    if !errorlevel! neq 0 (
        echo ERROR: NuGet restore failed for Revit %%V
        exit /b !errorlevel!
    )
    echo.

    REM Build
    echo Building for Revit %%V...
    %MSBUILD% %PROJECT% /p:Configuration=Release /p:Platform=x64 /p:RevitVersion=%%V
    if !errorlevel! neq 0 (
        echo ERROR: Build failed for Revit %%V
        exit /b !errorlevel!
    )

    echo.
    echo Copying files to bundle for Revit %%V...

    REM Determine output path based on version (.NET 4.8 vs .NET 8.0)
    if "%%V"=="2023" set OUTDIR=..\bimkraft-src\bin\x64\Release\net48
    if "%%V"=="2024" set OUTDIR=..\bimkraft-src\bin\x64\Release\net48
    if "%%V"=="2025" set OUTDIR=..\bimkraft-src\bin\x64\Release\net8.0-windows
    if "%%V"=="2026" set OUTDIR=..\bimkraft-src\bin\x64\Release\net8.0-windows

    REM Copy BIMKraft.dll
    copy /Y "!OUTDIR!\BIMKraft.dll" "%BUNDLE_ROOT%\Contents\%%V\"
    if !errorlevel! neq 0 (
        echo ERROR: Failed to copy BIMKraft.dll for Revit %%V
        echo Expected path: !OUTDIR!\BIMKraft.dll
        exit /b !errorlevel!
    )

    REM Copy Newtonsoft.Json.dll from packages folder
    REM Use net45 for Revit 2023/2024 (.NET 4.8), net6.0 for Revit 2025/2026 (.NET 8.0)
    if "%%V"=="2023" set JSONLIB=..\bimkraft-src\packages\Newtonsoft.Json.13.0.4\lib\net45\Newtonsoft.Json.dll
    if "%%V"=="2024" set JSONLIB=..\bimkraft-src\packages\Newtonsoft.Json.13.0.4\lib\net45\Newtonsoft.Json.dll
    if "%%V"=="2025" set JSONLIB=..\bimkraft-src\packages\Newtonsoft.Json.13.0.4\lib\net6.0\Newtonsoft.Json.dll
    if "%%V"=="2026" set JSONLIB=..\bimkraft-src\packages\Newtonsoft.Json.13.0.4\lib\net6.0\Newtonsoft.Json.dll
    copy /Y "!JSONLIB!" "%BUNDLE_ROOT%\Contents\%%V\"
    if !errorlevel! neq 0 (
        echo ERROR: Failed to copy Newtonsoft.Json.dll for Revit %%V
        echo Expected path: !JSONLIB!
        exit /b !errorlevel!
    )

    REM Copy App.config
    copy /Y "..\bimkraft-src\App.config" "%BUNDLE_ROOT%\Contents\%%V\"

    REM Copy version-specific .addin file
    copy /Y "addin_templates\BIMKraft_%%V.addin" "%BUNDLE_ROOT%\Contents\%%V\BIMKraft.addin"
    if !errorlevel! neq 0 (
        echo ERROR: Failed to copy .addin file for Revit %%V
        exit /b !errorlevel!
    )

    REM Copy icon files
    xcopy /Y "..\bimkraft-src\Resources\Icons\*.png" "%BUNDLE_ROOT%\Contents\%%V\Resources\Icons\"
    if !errorlevel! neq 0 (
        echo ERROR: Failed to copy icons for Revit %%V
        exit /b !errorlevel!
    )

    echo Done copying files for Revit %%V
    echo.
)

REM Copy PackageContents.xml
echo Copying PackageContents.xml...
copy /Y "PackageContents.xml" "%BUNDLE_ROOT%\"

REM Copy LICENSE
echo Copying LICENSE.txt...
copy /Y "LICENSE.txt" "%BUNDLE_ROOT%\"

echo.
echo ================================================================
echo Bundle creation complete!
echo ================================================================
echo Bundle location: %cd%\%BUNDLE_ROOT%
echo.
echo Verifying bundle contents...
dir /s /b "%BUNDLE_ROOT%" | find /c "BIMKraft.dll" > nul
if !errorlevel! equ 0 (
    echo [OK] Found BIMKraft.dll files
)
dir /s /b "%BUNDLE_ROOT%" | find /c "Newtonsoft.Json.dll" > nul
if !errorlevel! equ 0 (
    echo [OK] Found Newtonsoft.Json.dll files
)
dir /s /b "%BUNDLE_ROOT%" | find /c ".png" > nul
if !errorlevel! equ 0 (
    echo [OK] Found icon files
)
echo.
echo Next steps:
echo 1. Review bundle contents in: %BUNDLE_ROOT%
echo 2. Build WiX installer: build_msi.bat
echo 3. Test installer on clean VM
echo 4. Submit to Autodesk App Store
echo.
pause
