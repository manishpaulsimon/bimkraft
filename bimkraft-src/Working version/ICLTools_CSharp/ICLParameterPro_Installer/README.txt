========================================
ICL Parameter Pro - Installation Package
========================================
Version: 1.0.0
For: Autodesk Revit 2023

QUICK INSTALLATION
==================
1. Double-click: INSTALL.bat
2. Start Revit 2023
3. Look for "ICL" tab in ribbon
4. Click "Parameter Pro" button

FIRST TIME USE
==============
Before using the tool:
1. In Revit: Manage > Shared Parameters
2. Browse to your company's .txt parameter file
3. Click OK
4. Now use the tool!

WHAT IT DOES
============
ICL Parameter Pro helps you manage project parameters:
+ Select from shared parameters
+ Configure binding types (Instance/Type)
+ Apply to multiple categories
+ Save and load presets
+ Search and filter parameters

WHERE TO FIND IT IN REVIT
==========================
After starting Revit 2023:

Step 1: Look at the top ribbon tabs
[Architecture] [Structure] ... [Modify] [Add-Ins] [ICL] <-- HERE!

Step 2: Click the "ICL" tab

Step 3: You'll see "Parameter Tools" panel

Step 4: Click "Parameter Pro" button

MANUAL INSTALLATION
===================
If INSTALL.bat doesn't work, copy these 3 files:
  - ICLParameterPro.dll
  - ICLParameterPro.addin
  - Newtonsoft.Json.dll

Paste them here (copy this path to Windows Explorer):
  %AppData%\Autodesk\Revit\Addins\2023\

Then restart Revit.

UNINSTALLATION
==============
Double-click: UNINSTALL.bat

SYSTEM REQUIREMENTS
===================
- Autodesk Revit 2023
- Windows 10 or Windows 11
- .NET Framework 4.8 (included with Windows)

FEATURES
========
+ Tree view of all shared parameters
+ Search and filter parameters by name
+ Configure for each parameter:
  - Binding type (Instance or Type)
  - Categories (Views, Sheets, Walls, etc.)
  - Parameter group (where it appears in properties)
+ Save configurations as JSON presets
+ Load presets for quick parameter setup
+ Add multiple parameters at once
+ Professional WPF interface

USING PRESETS
=============
Presets are saved in:
  %AppData%\Autodesk\Revit\Addins\2023\ParameterPresets\

You can:
- Save your parameter configurations
- Share preset files with colleagues
- Load presets in any project

TROUBLESHOOTING
===============
Problem: ICL tab doesn't appear
Solution:
  - Restart Revit
  - Verify files are in %AppData%\Autodesk\Revit\Addins\2023\
  - Check Windows Event Viewer for errors

Problem: "No shared parameter file loaded"
Solution:
  - Go to: Manage > Shared Parameters
  - Browse to your .txt parameter file
  - Click OK

Problem: Parameters not being added
Solution:
  - Select at least one category
  - Verify parameter exists in shared parameter file
  - Check if parameter already exists in project

SUPPORT
=======
For questions or issues:
- Contact ICL Team
- Check company SharePoint for guides
- Ask in Teams channel

FILE LIST
=========
This package contains:
  ICLParameterPro.dll       - Main add-in (126 KB)
  ICLParameterPro.addin     - Revit manifest (1 KB)
  Newtonsoft.Json.dll       - JSON library (700 KB)
  INSTALL.bat               - Installation script
  UNINSTALL.bat             - Removal script
  README.txt                - This file
  VERSION.txt               - Version information

TECHNICAL DETAILS
=================
- Built with: C# + WPF
- Framework: .NET Framework 4.8
- Revit API: 2023
- Platform: Any CPU
- Dependencies: Newtonsoft.Json 13.0.3

LICENSE
=======
(c) 2025 ICL Team. All rights reserved.
This software is for use by authorized ICL team members and clients.

CHANGELOG
=========
Version 1.0.0 (2025)
- Initial release
- Custom ICL ribbon tab
- Parameter selection from shared parameters
- Binding type configuration
- Multi-category support
- Preset system (save/load)
- Search and filter
- Professional UI

========================================
Thank you for using ICL Parameter Pro!
========================================
