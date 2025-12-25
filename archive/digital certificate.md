● The publisher shows as "Unknown Publisher" because the BIMKraft.dll is not digitally signed with a code signing certificate.

  Why This Happens

  In the Revit security dialog, the Publisher field shows:
  - The certificate subject name if the DLL is digitally signed
  - "Unknown Publisher" if the DLL is not signed (which is the current case)

  The Name field ("BIM Kraft Tools") comes from the assembly metadata, which is correctly set. But the Publisher field specifically requires a digital signature from a trusted Certificate Authority (CA).

  This is Normal for Development

  For development and personal use, having "Unknown Publisher" is completely normal and expected. Many Revit plugins show this unless they're:
  - Commercially distributed
  - Code-signed with a certificate from a trusted CA

  If You Want to Fix It

  To show a verified publisher name, you would need to:

  1. Obtain a code signing certificate from a trusted CA like:
    - DigiCert
    - Sectigo
    - GlobalSign
    - (These typically cost $100-400/year)
  2. Sign the DLL after building using signtool.exe:
  signtool sign /f MyCertificate.pfx /p password /t http://timestamp.digicert.com BIMKraft.dll     
  3. Update the build script to automatically sign after compilation

  Recommendation

  For now, you can safely click "Always Load" since this is your own plugin. The security warning is just Windows/Revit being cautious about unsigned code, which is standard security practice.      

  If you plan to distribute BIMKraft to others in your organization or commercially, then investing in a code signing certificate would be worthwhile to avoid this warning for your users.