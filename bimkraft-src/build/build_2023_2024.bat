@echo off
echo Building BIM Kraft Tools for Revit 2023 and 2024...
echo.

set MSBUILD="C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
set PROJECT=..\BIMKraft.csproj

echo Restoring NuGet packages for Revit 2023...
%MSBUILD% %PROJECT% /t:Restore /p:Configuration=Debug /p:Platform=x64 /p:RevitVersion=2023
if %errorlevel% neq 0 (
    echo ERROR: NuGet restore failed for Revit 2023
    pause
    exit /b %errorlevel%
)
echo.

echo Building for Revit 2023...
%MSBUILD% %PROJECT% /p:Configuration=Debug /p:Platform=x64 /p:RevitVersion=2023
if %errorlevel% neq 0 (
    echo ERROR: Build failed for Revit 2023
    pause
    exit /b %errorlevel%
)
echo.

echo Restoring NuGet packages for Revit 2024...
%MSBUILD% %PROJECT% /t:Restore /p:Configuration=Debug /p:Platform=x64 /p:RevitVersion=2024
if %errorlevel% neq 0 (
    echo ERROR: NuGet restore failed for Revit 2024
    pause
    exit /b %errorlevel%
)
echo.

echo Building for Revit 2024...
%MSBUILD% %PROJECT% /p:Configuration=Debug /p:Platform=x64 /p:RevitVersion=2024
if %errorlevel% neq 0 (
    echo ERROR: Build failed for Revit 2024
    pause
    exit /b %errorlevel%
)
echo.

echo All builds completed successfully!
echo.
echo DLLs deployed to:
echo   - %%AppData%%\Autodesk\Revit\Addins\2023\BIMKraft.dll
echo   - %%AppData%%\Autodesk\Revit\Addins\2024\BIMKraft.dll
echo.
echo Tools available in BIMKraft tab:
echo   - Parameter Pro (Parameter Tools panel)
echo   - Parameter Transfer Pro (Parameter Tools panel)
echo   - Workset Tools (Workset Tools panel)
pause
