@echo off
echo Building BIM Kraft Tools for Revit 2026...
echo.

set MSBUILD="C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
set PROJECT=..\BIMKraft.csproj

echo Restoring NuGet packages for Revit 2026...
%MSBUILD% %PROJECT% /t:Restore /p:Configuration=Debug /p:Platform=x64 /p:RevitVersion=2026
if %errorlevel% neq 0 (
    echo ERROR: NuGet restore failed for Revit 2026
    pause
    exit /b %errorlevel%
)
echo.

echo Building for Revit 2026...
%MSBUILD% %PROJECT% /p:Configuration=Debug /p:Platform=x64 /p:RevitVersion=2026
if %errorlevel% neq 0 (
    echo ERROR: Build failed for Revit 2026
    pause
    exit /b %errorlevel%
)
echo.

echo Build completed successfully!
echo.
echo DLL deployed to: %%AppData%%\Autodesk\Revit\Addins\2026\BIMKraft.dll
echo.
echo ========================================
echo BIMKraft Tools Available:
echo ========================================
echo.
echo Parameter Tools Panel:
echo   - Parameter Pro
echo   - Parameter Transfer Pro
echo.
echo Workset Tools Panel:
echo   - Raster ^& Ebenen
echo   - Referenzen
echo   - Architektur
echo   - Bewehrung
echo   - Rohbau
echo   - Stahl
echo.
echo Quality Tools Panel:
echo   - Warnings Browser Pro
echo.
echo Measurement Tools Panel:
echo   - Line Length Calculator
echo.
echo Family Tools Panel:
echo   - Family Renamer
echo.
echo ========================================
pause
