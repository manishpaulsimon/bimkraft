@echo off
REM BIMKraft Build Script
REM Builds BIMKraft for specified Revit versions

echo ====================================
echo BIMKraft Build Script
echo ====================================
echo.

:menu
echo Select Revit version to build:
echo.
echo 1. Revit 2023 (.NET 4.8)
echo 2. Revit 2024 (.NET 4.8)
echo 3. Revit 2025 (.NET 8.0)
echo 4. Revit 2026 (.NET 8.0)
echo 5. Build All Versions
echo 6. Clean Build (Release)
echo 7. Exit
echo.

set /p choice="Enter choice (1-7): "

if "%choice%"=="1" goto revit2023
if "%choice%"=="2" goto revit2024
if "%choice%"=="3" goto revit2025
if "%choice%"=="4" goto revit2026
if "%choice%"=="5" goto buildall
if "%choice%"=="6" goto release
if "%choice%"=="7" goto end
echo Invalid choice, please try again.
echo.
goto menu

:revit2023
echo.
echo Building for Revit 2023...
cd bimkraft-src
dotnet build /p:RevitVersion=2023 /p:Configuration=Debug
cd ..
goto done

:revit2024
echo.
echo Building for Revit 2024...
cd bimkraft-src
dotnet build /p:RevitVersion=2024 /p:Configuration=Debug
cd ..
goto done

:revit2025
echo.
echo Building for Revit 2025...
cd bimkraft-src
dotnet build /p:RevitVersion=2025 /p:Configuration=Debug
cd ..
goto done

:revit2026
echo.
echo Building for Revit 2026...
cd bimkraft-src
dotnet build /p:RevitVersion=2026 /p:Configuration=Debug
cd ..
goto done

:buildall
echo.
echo Building for all Revit versions...
echo.
echo [1/4] Building Revit 2023...
cd bimkraft-src
dotnet build /p:RevitVersion=2023 /p:Configuration=Debug
echo.
echo [2/4] Building Revit 2024...
dotnet build /p:RevitVersion=2024 /p:Configuration=Debug
echo.
echo [3/4] Building Revit 2025...
dotnet build /p:RevitVersion=2025 /p:Configuration=Debug
echo.
echo [4/4] Building Revit 2026...
dotnet build /p:RevitVersion=2026 /p:Configuration=Debug
cd ..
goto done

:release
echo.
echo Building for Revit 2026 (Release)...
cd bimkraft-src
dotnet clean
dotnet build /p:RevitVersion=2026 /p:Configuration=Release
cd ..
goto done

:done
echo.
echo ====================================
echo Build Complete!
echo ====================================
echo.
echo DLL Location:
echo Debug: bimkraft-src\bin\Debug\net8.0-windows\BIMKraft.dll
echo Or: %%AppData%%\Autodesk\Revit\Addins\2026\BIMKraft.dll
echo.
pause
goto menu

:end
echo.
echo Exiting...
