@echo off
REM Build License Key Generator

echo ====================================
echo Building License Key Generator...
echo ====================================
echo.

dotnet build -c Release

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ====================================
    echo Build Successful!
    echo ====================================
    echo.
    echo Executable location:
    echo bin\Release\net8.0\LicenseKeyGenerator.exe
    echo.
    echo You can now run:
    echo - generate-license.bat (interactive mode)
    echo - generate-test-keys.bat (batch test keys)
    echo.
) else (
    echo.
    echo ====================================
    echo Build Failed!
    echo ====================================
    echo.
    echo Check the error messages above.
    echo.
)

pause
