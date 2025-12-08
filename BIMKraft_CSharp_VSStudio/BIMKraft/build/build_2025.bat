@echo off
echo Building BIM Kraft Tools for Revit 2025...
echo.

set MSBUILD="C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
set PROJECT=..\BIMKraft.csproj

echo Restoring NuGet packages for Revit 2025...
%MSBUILD% %PROJECT% /t:Restore /p:Configuration=Debug /p:Platform=x64 /p:RevitVersion=2025
if %errorlevel% neq 0 (
    echo ERROR: NuGet restore failed for Revit 2025
    pause
    exit /b %errorlevel%
)
echo.

echo Building for Revit 2025...
%MSBUILD% %PROJECT% /p:Configuration=Debug /p:Platform=x64 /p:RevitVersion=2025
if %errorlevel% neq 0 (
    echo ERROR: Build failed for Revit 2025
    pause
    exit /b %errorlevel%
)
echo.

echo Build completed successfully!
echo.
echo DLL deployed to: %%AppData%%\Autodesk\Revit\Addins\2025\BIMKraft.dll
echo.
echo Tools available in BIMKraft tab:
echo   - Parameter Pro (Parameter Tools panel)
echo   - Parameter Transfer Pro (Parameter Tools panel)
echo   - Workset Tools (Workset Tools panel)
pause
