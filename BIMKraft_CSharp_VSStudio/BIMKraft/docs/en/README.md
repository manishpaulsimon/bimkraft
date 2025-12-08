# BIMKraft Tools Documentation

## Overview

BIMKraft Tools is a collection of productivity tools for Autodesk Revit, designed to streamline BIM workflows and enhance parameter management capabilities.

## Available Tools

### 1. Parameter Pro
Advanced parameter management tool with preset support, batch operations, and intelligent category handling.

**Features:**
- Multi-select parameter management
- Category-based parameter binding
- Preset save/load functionality
- Merge categories for existing parameters
- Search and filter capabilities
- Support for Revit 2023-2026

**Documentation:** [Parameter Pro User Guide](ParameterPro_UserGuide.md)

### 2. Future Tools
Additional tools will be added to the BIMKraft tab as the suite expands.

---

## Installation

### Requirements
- Autodesk Revit 2023, 2024, 2025, or 2026
- Windows 10 or later
- .NET Framework 4.8 (Revit 2023-2024) or .NET 8.0 (Revit 2025-2026)

### Installation Steps

1. Close Revit if it's running
2. Copy `BIMKraft Tools.dll` to:
   - `%AppData%\Autodesk\Revit\Addins\[VERSION]\`
   - Replace [VERSION] with your Revit version (2023, 2024, 2025, or 2026)
3. Copy `BIMKraft.addin` to the same folder
4. Copy any required dependency DLLs (e.g., `Newtonsoft.Json.dll`) to the same folder
5. Launch Revit
6. Look for the **BIMKraft** tab in the ribbon

### Verification

After installation, you should see:
- A new **BIMKraft** tab in the Revit ribbon
- **Parameter Tools** panel with the **Parameter Pro** button

---

## Getting Started

### First Time Use

1. **Configure Shared Parameters**
   - Ensure your shared parameter file is loaded in Revit
   - Go to Manage → Project Parameters → Shared Parameters

2. **Launch a Tool**
   - Click on the BIMKraft tab
   - Select the tool you want to use
   - Follow the tool-specific workflow

3. **Explore Features**
   - Each tool has built-in tooltips
   - Refer to individual tool documentation for detailed instructions

---

## Documentation by Tool

- [Parameter Pro User Guide](ParameterPro_UserGuide.md) - Complete guide for parameter management

---

## Project Structure

For developers and those interested in extending BIMKraft Tools:

See [Project Structure Guide](../PROJECT_STRUCTURE.md) for information on:
- Adding new tools
- Project organization
- Best practices
- Development workflow

---

## Support & Troubleshooting

### Common Issues

**BIMKraft Tab Not Appearing**
- Verify DLL files are in the correct Addins folder
- Check that the .addin file matches your Revit version
- Restart Revit

**Tools Not Loading**
- Check Revit version compatibility
- Ensure all dependency DLLs are present
- Review Windows Event Viewer for error details

**Shared Parameters Not Found**
- Verify shared parameter file is loaded in Revit
- Check file path in Manage → Shared Parameters
- Ensure parameter names match exactly

### Getting Help

1. Check the tool-specific documentation
2. Review the troubleshooting section in each guide
3. Contact your BIM manager or IT support
4. Check the project repository for known issues

---

## Version Information

### Current Version: 1.2

**What's New:**
- Removed default categories for full user control
- Improved multi-select behavior
- Added category merge functionality
- Enhanced preset management
- Multi-version Revit support

**Supported Revit Versions:**
- Revit 2023 (NET Framework 4.8)
- Revit 2024 (NET Framework 4.8)
- Revit 2025 (.NET 8.0)
- Revit 2026 (.NET 8.0)

---

## Best Practices

### Shared Parameters
- Maintain a centralized shared parameter file for your team
- Use consistent naming conventions
- Document parameter purposes and usage
- Version control your shared parameter file

### Presets
- Create presets for common scenarios
- Share presets with team members
- Document preset purposes
- Regular backup of preset files

### Workflow Integration
- Train team members on tool usage
- Establish standard workflows
- Document company-specific processes
- Regular review and updates

---

## Feedback & Contributions

We welcome feedback and suggestions for new features:
- Report bugs or issues
- Request new features
- Share workflow improvements
- Contribute to documentation

---

## License

[Your license information here]

---

## Credits

Developed by Maria Simon (bimkraft.de)
For questions and support, contact your BIM team

---

**BIMKraft Tools** - Enhancing Revit Productivity

*Last Updated: 2024*
