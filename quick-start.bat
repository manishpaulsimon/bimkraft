@echo off
REM BIMKraft Quick Start - Main Menu

:menu
cls
echo ====================================
echo BIMKraft Development Quick Start
echo ====================================
echo.
echo What would you like to do?
echo.
echo 1. Build BIMKraft Plugin (Revit 2026)
echo 2. Build All Revit Versions
echo 3. Generate Customer License Key
echo 4. Generate Test License Keys
echo 5. Build License Key Generator
echo 6. Open License Generator Folder
echo 7. Open Build Output Folder
echo 8. Clean All Builds
echo 9. Exit
echo.

set /p choice="Enter choice (1-9): "

if "%choice%"=="1" goto build2026
if "%choice%"=="2" goto buildall
if "%choice%"=="3" goto genlicense
if "%choice%"=="4" goto gentestkeys
if "%choice%"=="5" goto buildgen
if "%choice%"=="6" goto opengenfolder
if "%choice%"=="7" goto openbuildfolder
if "%choice%"=="8" goto clean
if "%choice%"=="9" goto end
echo Invalid choice, please try again.
timeout /t 2 >nul
goto menu

:build2026
cls
echo Building BIMKraft for Revit 2026...
echo.
cd bimkraft-src
dotnet build /p:RevitVersion=2026 /p:Configuration=Debug
cd ..
echo.
echo Build complete! DLL copied to:
echo %%AppData%%\Autodesk\Revit\Addins\2026\BIMKraft.dll
echo.
pause
goto menu

:buildall
cls
echo Building BIMKraft for all Revit versions...
echo.
cd bimkraft-src
echo [1/4] Revit 2023...
dotnet build /p:RevitVersion=2023 /p:Configuration=Debug
echo [2/4] Revit 2024...
dotnet build /p:RevitVersion=2024 /p:Configuration=Debug
echo [3/4] Revit 2025...
dotnet build /p:RevitVersion=2025 /p:Configuration=Debug
echo [4/4] Revit 2026...
dotnet build /p:RevitVersion=2026 /p:Configuration=Debug
cd ..
echo.
echo All builds complete!
echo.
pause
goto menu

:genlicense
cls
echo Starting License Key Generator (Interactive Mode)...
echo.
cd tools\LicenseKeyGenerator
dotnet run -c Release
cd ..\..
echo.
pause
goto menu

:gentestkeys
cls
echo Generating Test License Keys...
echo.
cd tools\LicenseKeyGenerator
dotnet run -c Release -- --batch
cd ..\..
echo.
pause
goto menu

:buildgen
cls
echo Building License Key Generator...
echo.
cd tools\LicenseKeyGenerator
dotnet build -c Release
cd ..\..
echo.
echo Generator built successfully!
echo Location: tools\LicenseKeyGenerator\bin\Release\net8.0\
echo.
pause
goto menu

:opengenfolder
start "" "tools\LicenseKeyGenerator"
goto menu

:openbuildfolder
start "" "%AppData%\Autodesk\Revit\Addins\2026"
goto menu

:clean
cls
echo Cleaning all builds...
echo.
cd bimkraft-src
dotnet clean
cd ..\tools\LicenseKeyGenerator
dotnet clean
cd ..\..
echo.
echo Clean complete!
echo.
pause
goto menu

:end
cls
echo.
echo Thank you for using BIMKraft Quick Start!
echo.
timeout /t 2 >nul
