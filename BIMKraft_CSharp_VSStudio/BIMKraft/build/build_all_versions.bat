@echo off
echo Building WIP Tools for all Revit versions...
echo.

set MSBUILD="C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
set PROJECT=..\ICLTools.csproj

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

echo All builds completed successfully!
pause
