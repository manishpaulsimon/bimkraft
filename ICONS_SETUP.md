# BIMKraft Icon System Setup

## Overview

This document describes the icon system setup for BIMKraft tools, implementing a consistent "Power & Speed" theme that reflects the BIM Kraft (BIM Power) brand identity.

## What Was Done

### 1. Design Documentation (`DESIGN.md`)
Created comprehensive brand guidelines including:
- **Brand Identity**: BIM Kraft = BIM Power (Speed + Energy)
- **Color Palette**:
  - Electric Blue (#0096FF) - Primary brand color
  - Lightning Yellow (#FFFF00) - Speed and energy
  - Power Orange (#FF6400) - Warnings and power
  - Steel Gray (#64788C) - Industrial strength
  - Energy Green (#32C832) - Quality and success
  - Gold (#FFD700) - Premium features
  - Fire Red (#FF3200) - Errors and critical
- **Design Principles**: Speed, power, professional industrial look
- **Tool-Specific Color Schemes**: Each tool category has defined colors

### 2. Icon Generator (`generate_icons.py`)
Updated Python script to generate 6 power-themed icons:

#### Parameter Tools
- **parameter_pro.png** - Electric blue + gold 'P' + lightning
- **parameter_transfer_pro.png** - Electric blue + transfer arrows + lightning

#### Workset Tools
- **workset_manager.png** - Steel gray + layered worksets + lightning

#### Quality Tools
- **warnings_browser_pro.png** - Power orange + warning triangle + exclamation

#### Measurement Tools
- **line_length_calculator.png** - Power orange + ruler + connected lines + lightning

#### Family Tools
- **family_renamer.png** - Electric blue + lightning bolt + speed lines

All icons feature:
- 32x32 pixels (Revit ribbon standard)
- PNG with RGBA transparency
- Radial gradient backgrounds
- Lightning bolts and speed lines
- Glossy borders for modern look

### 3. Ribbon Application Updates (`BIMKraftRibbonApplication.cs`)
- Added `LoadIcon()` method to load icons from Resources/Icons folder
- Updated all 6 button creation methods to set `LargeImage` property:
  - CreateParameterProButton()
  - CreateParameterTransferProButton()
  - CreateWorksetButtons() - for Workset Manager
  - CreateWarningsBrowserProButton()
  - CreateLineLengthCalculatorButton()
  - CreateFamilyRenamerButton()

### 4. Project Build System (`BIMKraft.csproj`)
Updated post-build target to:
- Create Resources/Icons directory in Revit Addins folder
- Copy all PNG icons from project to deployment location
- Icons are now automatically deployed with each build

### 5. Icon Resources Folder
Created structure:
```
bimkraft-src/
└── Resources/
    └── Icons/
        ├── README.md (comprehensive usage guide)
        └── *.png (icon files - to be generated)
```

## How to Generate Icons

### Prerequisites
1. Install Python 3.x (https://www.python.org/downloads/)
2. Install Pillow library:
   ```bash
   pip install pillow
   ```

### Generate Icons
From repository root:
```bash
python generate_icons.py
```

This creates all 6 icons in the `icons/` folder.

### Copy to Project
```bash
# Windows PowerShell
Copy-Item icons\*.png bimkraft-src\Resources\Icons\

# Windows CMD
xcopy /Y icons\*.png bimkraft-src\Resources\Icons\

# Linux/Mac
cp icons/*.png bimkraft-src/Resources/Icons/
```

### Build and Deploy
```bash
cd bimkraft-src/build
./build_2026.bat
```

Icons are automatically copied to:
```
%AppData%\Autodesk\Revit\Addins\2026\Resources\Icons\
```

## Icon Locations

### During Development
- Source icons: `bimkraft-src/Resources/Icons/*.png`
- Generated icons: `icons/*.png` (from generator script)

### After Build
Icons are deployed to:
```
%AppData%\Autodesk\Revit\Addins\{VERSION}\Resources\Icons\
```

For example:
```
C:\Users\{USER}\AppData\Roaming\Autodesk\Revit\Addins\2026\Resources\Icons\
```

## File Structure

```
bimkraft/
├── DESIGN.md                          # Brand design documentation
├── ICONS_SETUP.md                     # This file
├── generate_icons.py                  # Icon generator script
├── icons/                             # Generated icons (git ignored)
│   ├── parameter_pro.png
│   ├── parameter_transfer_pro.png
│   ├── workset_manager.png
│   ├── warnings_browser_pro.png
│   ├── line_length_calculator.png
│   └── family_renamer.png
└── bimkraft-src/
    ├── BIMKraftRibbonApplication.cs   # Loads and applies icons
    ├── BIMKraft.csproj                # Copies icons during build
    └── Resources/
        └── Icons/
            ├── README.md              # Icon usage guide
            └── *.png                  # Source icons
```

## Verification

After building and loading Revit, you should see:
1. BIMKraft tab in Revit ribbon
2. 5 panels: Parameter Tools, Workset Tools, Quality Tools, Measurement Tools, Family Tools
3. Each button has a colorful 32x32 icon with the power/speed theme
4. Lightning bolts visible on most icons
5. Consistent color scheme across all tools

## Troubleshooting

### Icons Not Appearing
1. **Check icon files exist**:
   ```
   dir "%AppData%\Autodesk\Revit\Addins\2026\Resources\Icons"
   ```

2. **Regenerate icons**:
   ```bash
   python generate_icons.py
   cp icons/*.png bimkraft-src/Resources/Icons/
   ```

3. **Rebuild project**:
   ```bash
   cd bimkraft-src/build
   ./build_2026.bat
   ```

4. **Restart Revit** completely

### Python Not Installed
If Python isn't available:
- Download from https://python.org/downloads/
- Or create placeholder 32x32 PNG files manually
- Or request pre-generated icons

### Build Errors
If post-build icon copy fails:
- Manually create the Icons folder in Revit Addins
- Manually copy PNG files
- Verify file permissions

## Brand Consistency

All icons follow the BIMKraft design system:
- **Speed**: Lightning bolts, speed lines, dynamic angles
- **Power**: Radial gradients, bold shapes, high contrast
- **Professional**: Clean design, glossy effects, consistent sizing

See `DESIGN.md` for complete brand guidelines.

## Future Enhancements

Potential improvements:
- 16x16 small icon variants
- SVG versions for documentation
- Dark mode variants
- Animated splash screen icons
- Accessibility-optimized versions

---

**BIMKraft - BIM Power**
*Fast. Powerful. Professional.*

Generated: 2025-12-25
