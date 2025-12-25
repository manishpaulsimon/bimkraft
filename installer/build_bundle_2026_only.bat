@echo off
setlocal enabledelayedexpansion

echo ================================================================
echo BIMKraft Bundle Builder - Revit 2026 Only
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

echo Creating bundle directory structure for Revit 2026...
mkdir "%BUNDLE_ROOT%\Contents\2026\Resources\Icons"
echo.

echo Building BIMKraft for Revit 2026 (Release)...
echo ================================================================

REM Restore NuGet packages
echo Restoring NuGet packages for Revit 2026...
%MSBUILD% %PROJECT% /t:Restore /p:Configuration=Release /p:Platform=x64 /p:RevitVersion=2026
if !errorlevel! neq 0 (
    echo ERROR: NuGet restore failed for Revit 2026
    pause
    exit /b !errorlevel!
)
echo.

REM Build
echo Building for Revit 2026...
%MSBUILD% %PROJECT% /p:Configuration=Release /p:Platform=x64 /p:RevitVersion=2026
if !errorlevel! neq 0 (
    echo ERROR: Build failed for Revit 2026
    pause
    exit /b !errorlevel!
)

echo.
echo Copying files to bundle for Revit 2026...

set OUTDIR=..\bimkraft-src\bin\x64\Release\net8.0-windows

REM Copy BIMKraft.dll
copy /Y "%OUTDIR%\BIMKraft.dll" "%BUNDLE_ROOT%\Contents\2026\"
if !errorlevel! neq 0 (
    echo ERROR: Failed to copy BIMKraft.dll
    echo Expected path: %OUTDIR%\BIMKraft.dll
    pause
    exit /b !errorlevel!
)

REM Copy Newtonsoft.Json.dll from packages folder
REM Note: For net8.0-windows, NuGet dependencies may not be copied to bin, so we copy from packages
copy /Y "..\bimkraft-src\packages\Newtonsoft.Json.13.0.4\lib\net6.0\Newtonsoft.Json.dll" "%BUNDLE_ROOT%\Contents\2026\"
if !errorlevel! neq 0 (
    echo ERROR: Failed to copy Newtonsoft.Json.dll
    pause
    exit /b !errorlevel!
)

REM Copy App.config
copy /Y "..\bimkraft-src\App.config" "%BUNDLE_ROOT%\Contents\2026\"

REM Copy version-specific .addin file
copy /Y "addin_templates\BIMKraft_2026.addin" "%BUNDLE_ROOT%\Contents\2026\BIMKraft.addin"
if !errorlevel! neq 0 (
    echo ERROR: Failed to copy .addin file
    pause
    exit /b !errorlevel!
)

REM Copy icon files
xcopy /Y "..\bimkraft-src\Resources\Icons\*.png" "%BUNDLE_ROOT%\Contents\2026\Resources\Icons\"
if !errorlevel! neq 0 (
    echo ERROR: Failed to copy icons
    pause
    exit /b !errorlevel!
)

echo Done copying files for Revit 2026
echo.

REM Copy PackageContents.xml (modified for 2026 only)
echo Creating simplified PackageContents.xml for Revit 2026...
echo ^<?xml version="1.0" encoding="utf-8"?^> > "%BUNDLE_ROOT%\PackageContents.xml"
echo ^<ApplicationPackage >> "%BUNDLE_ROOT%\PackageContents.xml"
echo   SchemaVersion="1.0" >> "%BUNDLE_ROOT%\PackageContents.xml"
echo   ProductType="Application" >> "%BUNDLE_ROOT%\PackageContents.xml"
echo   Name="BIM Kraft Tools" >> "%BUNDLE_ROOT%\PackageContents.xml"
echo   Description="Advanced BIM tools for Revit" >> "%BUNDLE_ROOT%\PackageContents.xml"
echo   Author="Maria Simon" >> "%BUNDLE_ROOT%\PackageContents.xml"
echo   AppVersion="0.1.0" >> "%BUNDLE_ROOT%\PackageContents.xml"
echo   ProductCode="{B9E8F2C1-6D5A-4E3B-8F7C-1A2B3C4D5E6F}" >> "%BUNDLE_ROOT%\PackageContents.xml"
echo   FriendlyVersion="0.1.0"^> >> "%BUNDLE_ROOT%\PackageContents.xml"
echo   ^<CompanyDetails Name="BIMKraft.de" Email="contact@bimkraft.de" /^> >> "%BUNDLE_ROOT%\PackageContents.xml"
echo   ^<RuntimeRequirements OS="Win64" Platform="Revit" SeriesMin="R2026" SeriesMax="R2026" /^> >> "%BUNDLE_ROOT%\PackageContents.xml"
echo   ^<Components^> >> "%BUNDLE_ROOT%\PackageContents.xml"
echo     ^<ComponentEntry AppName="BIM Kraft Tools" Version="0.1.0" ModuleName="./Contents/2026/BIMKraft.addin" LoadOnRevitStartup="True" /^> >> "%BUNDLE_ROOT%\PackageContents.xml"
echo   ^</Components^> >> "%BUNDLE_ROOT%\PackageContents.xml"
echo ^</ApplicationPackage^> >> "%BUNDLE_ROOT%\PackageContents.xml"

REM Copy LICENSE
copy /Y "LICENSE.txt" "%BUNDLE_ROOT%\"

echo.
echo ================================================================
echo Bundle creation complete for Revit 2026!
echo ================================================================
echo Bundle location: %cd%\%BUNDLE_ROOT%
echo.
echo Verifying bundle contents...
dir /b "%BUNDLE_ROOT%\Contents\2026\" | find /c "BIMKraft.dll"
dir /b "%BUNDLE_ROOT%\Contents\2026\" | find /c "Newtonsoft.Json.dll"
dir /b "%BUNDLE_ROOT%\Contents\2026\Resources\Icons\" | find /c ".png"
echo.
echo Contents of bundle:
dir /s /b "%BUNDLE_ROOT%"
echo.
pause
