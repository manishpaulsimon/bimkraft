@echo off
REM BIMKraft License Key Generator - Batch Mode
REM Generates test license keys for development/testing

echo ====================================
echo BIMKraft Test License Generator
echo ====================================
echo.
echo Generating test license keys...
echo.
echo This will create 4 test keys:
echo - Monthly license (valid 1 month)
echo - Yearly license (valid 12 months)
echo - Expired license (for testing)
echo - Long-term license (valid 3 years)
echo.

dotnet run -c Release -- --batch

echo.
echo ====================================
echo Test keys saved to:
echo test_licenses_[timestamp].txt
echo ====================================
echo.
pause
