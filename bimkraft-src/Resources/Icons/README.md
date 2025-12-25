# BIMKraft Icon Resources

This folder contains the power and speed-themed icons for BIMKraft tools.

## Icon Set

The following icons are used in the BIMKraft ribbon:

### Parameter Tools
- `parameter_pro.png` - Electric blue with gold 'P' and lightning bolt
- `parameter_transfer_pro.png` - Electric blue with transfer arrows and lightning

### Workset Tools
- `workset_manager.png` - Steel gray with layered worksets and lightning

### Quality Tools
- `warnings_browser_pro.png` - Power orange with warning triangle and exclamation mark

### Measurement Tools
- `line_length_calculator.png` - Power orange with ruler and connected lines

### Family Tools
- `family_renamer.png` - Electric blue with lightning bolt and speed lines

## Generating Icons

### Prerequisites
1. Install Python 3.x
2. Install Pillow library:
   ```bash
   pip install pillow
   ```

### Generate All Icons
From the repository root:
```bash
python generate_icons.py
```

This will create all icons in the `icons/` folder with the BIMKraft power & speed theme.

### Copy Icons to Project
After generation, copy the icons from `icons/` to this folder:
```bash
# Windows
xcopy /Y icons\*.png bimkraft-src\Resources\Icons\

# Linux/Mac
cp icons/*.png bimkraft-src/Resources/Icons/
```

## Icon Specifications

- **Size**: 32x32 pixels (Revit ribbon standard)
- **Format**: PNG with RGBA transparency
- **Theme**: Power & Speed (BIM Power = BIM Kraft)
- **Brand Colors**:
  - Electric Blue: `RGB(0, 150, 255)` - Primary brand color
  - Lightning Yellow: `RGB(255, 255, 0)` - Speed and energy
  - Power Orange: `RGB(255, 100, 0)` - Warnings and power
  - Steel Gray: `RGB(100, 120, 140)` - Industrial strength
  - Gold: `RGB(255, 215, 0)` - Premium features

## Design Elements

Each icon features:
1. **Radial Gradient Background** - Energy radiating from center
2. **Tool-Specific Symbol** - Clear representation of functionality
3. **Lightning Bolts** - Speed and power symbolism
4. **Speed Lines** - Motion and efficiency
5. **Glossy Border** - Modern, polished appearance

## Integration

Icons are automatically loaded by `BIMKraftRibbonApplication.cs` using the `LoadIcon()` method.

The icons are loaded from the deployment folder:
```
%AppData%\Autodesk\Revit\Addins\2026\Resources\Icons\
```

## Build Process

Icons are copied during the build process via post-build events in `BIMKraft.csproj`.

If icons don't appear:
1. Verify icons exist in this folder
2. Rebuild the project to copy icons to output
3. Check Revit installation folder for icon files
4. Verify file permissions

## Design Documentation

For complete brand guidelines and design principles, see:
- [DESIGN.md](../../../DESIGN.md) - BIMKraft Design Documentation

## Customization

To create custom icons:
1. Edit `generate_icons.py` in the repository root
2. Follow the existing icon creation pattern
3. Use BIMKraft brand colors
4. Maintain 32x32 pixel size
5. Include power/speed themed elements

## Troubleshooting

### Icons Not Showing
- Ensure icon files exist in the deployment folder
- Check file names match exactly (case-sensitive)
- Verify PNG format and 32x32 size
- Rebuild project to copy icons

### Missing Icons During Development
If you don't have Python installed:
1. Download pre-generated icons from releases
2. Or manually create 32x32 PNG icons using any image editor
3. Follow the color scheme in DESIGN.md

---

**BIMKraft - BIM Power**
*Fast. Powerful. Professional.*
