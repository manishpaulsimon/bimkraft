@echo off
REM BIMKraft License Key Generator - Interactive Mode
REM Use this to generate license keys for customers

echo ====================================
echo BIMKraft License Key Generator
echo ====================================
echo.
echo This tool generates encrypted license keys for customers.
echo.
echo You will be prompted for:
echo - Customer email address
echo - License type (Monthly or Yearly)
echo - Custom duration (optional)
echo - Start date (optional)
echo.
echo Press any key to start...
pause >nul

dotnet run -c Release

echo.
echo ====================================
echo.
pause
