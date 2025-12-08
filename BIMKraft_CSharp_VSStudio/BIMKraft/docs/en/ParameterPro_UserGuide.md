# BIM Kraft Parameter Pro - User Guide

## Overview

BIM Kraft Parameter Pro is an advanced parameter management tool for Autodesk Revit that streamlines the process of adding shared parameters to your projects with powerful features like category selection, preset management, and batch operations.

## Table of Contents

- [Getting Started](#getting-started)
- [Interface Overview](#interface-overview)
- [Basic Workflow](#basic-workflow)
- [Features](#features)
- [Advanced Features](#advanced-features)
- [Tips & Best Practices](#tips--best-practices)
- [Troubleshooting](#troubleshooting)

---

## Getting Started

### Launching the Tool

1. Open your Revit project
2. Navigate to the **ICL** tab in the Revit ribbon
3. Click on **Parameter Pro** button in the "Parameter Tools" panel

### Requirements

- Autodesk Revit 2023, 2024, 2025, or 2026
- A shared parameter file configured in Revit
- Appropriate permissions to modify project parameters

---

## Interface Overview

The Parameter Pro window is divided into three main sections:

### 1. Available Parameters (Left Panel)

- **Search Box**: Filter parameters by name in real-time
- **Parameter Tree**: Hierarchical view of all parameters from your shared parameter file, grouped by category
- **Selection Summary**: Shows count of selected parameters

### 2. Parameter Configuration (Right Panel)

- **Category Checkboxes**: Select which Revit categories the parameters should apply to
- **Parameter Group**: Choose the parameter group (e.g., Data, Identity Data, etc.)
- **Binding Type**: Select Instance or Type binding

### 3. Bottom Controls

- **Presets Section**: Save, load, and delete parameter configurations
- **Action Buttons**:
  - Update Existing Parameters checkbox
  - Merge Categories checkbox
  - Add Parameters to Project button
  - Cancel button

---

## Basic Workflow

### Adding Parameters to Your Project

1. **Select Parameters**
   - Click on checkboxes next to parameter names to select them
   - Use Shift+Click to select a range of parameters
   - Selected parameters remain checked for easy multi-selection

2. **Configure Categories**
   - In the right panel, check the categories where parameters should be available
   - You can select multiple categories (e.g., Doors, Windows, Walls)

3. **Choose Parameter Group**
   - Select the appropriate parameter group from the dropdown
   - Default is "Data"

4. **Select Binding Type**
   - **Instance**: Parameter values are unique to each element instance
   - **Type**: Parameter values are shared across all elements of the same type

5. **Add to Project**
   - Click "Add Parameters to Project" button
   - Review the results dialog showing success, failures, and skipped parameters

---

## Features

### 1. Multi-Select Functionality

**Normal Click**: Toggle individual parameter selection
- Click a parameter to select it
- Click again to deselect it
- Previously selected parameters remain selected

**Shift+Click**: Range selection
- Select one parameter
- Hold Shift and click another parameter
- All parameters between them are selected/deselected based on the first selection

### 2. Search and Filter

- Type in the search box to filter parameters by name
- Search is case-insensitive
- Results update in real-time
- Search works across all parameter groups

### 3. Preset Management

Presets allow you to save and reuse parameter configurations.

**Creating a Preset**:
1. Select parameters
2. Configure categories, parameter group, and binding type
3. Click "Save Preset"
4. Enter a preset name
5. Preset is saved as a JSON file in your user directory

**Loading a Preset**:
1. Select a preset from the dropdown
2. Click "Load Preset"
3. All parameters and configurations from the preset are loaded

**Deleting a Preset**:
1. Select a preset from the dropdown
2. Click "Delete Preset"
3. Confirm deletion

**Preset Storage Location**:
`%AppData%\ICLParameterPro\Presets\`

### 4. Update Existing Parameters

When the "Update Existing Parameters" checkbox is enabled:
- Parameters that already exist in the project will be updated
- Without this option, existing parameters are skipped

### 5. Merge Categories

When the "Merge Categories" checkbox is enabled:
- New categories are **added** to existing parameter bindings
- Existing categories are **preserved**
- Perfect for incrementally adding categories to parameters

**Example**:
- Parameter "ICL_Color" exists with category "Doors"
- You load a preset with "ICL_Color" configured for "Windows"
- With "Merge Categories" enabled: Parameter now applies to both "Doors" AND "Windows"
- Without merge: Parameter would only apply to "Windows" (replaces existing)

---

## Advanced Features

### Unified Configuration

When multiple parameters are selected:
- A single configuration panel controls all selected parameters
- All parameters receive the same category bindings, parameter group, and binding type
- Efficient for batch parameter setup

### Parameter Grouping

Parameters are displayed grouped by their group name from the shared parameter file, making it easy to find related parameters.

### Real-time Selection Summary

The bottom of the left panel shows how many parameters are currently selected, providing instant feedback during selection.

---

## Tips & Best Practices

### 1. Use Presets for Common Scenarios

Create presets for frequently used parameter sets:
- "Window Parameters" - All window-related parameters with window categories
- "Finish Parameters" - Finish-related parameters for multiple categories
- "Project Data" - Standard project information parameters

### 2. Start with No Default Categories

As of the latest version, no categories are selected by default when you pick a parameter. This gives you full control to choose only the categories you need.

### 3. Merge Categories for Existing Projects

When adding parameters to existing projects:
1. Enable "Update Existing Parameters"
2. Enable "Merge Categories"
3. Load your preset

This ensures you don't accidentally remove existing category bindings.

### 4. Organize with Parameter Groups

Use meaningful parameter groups to organize parameters in property palettes:
- **Identity Data**: Basic identification information
- **Data**: General data fields
- **Dimensions**: Size and measurement parameters
- **Materials and Finishes**: Appearance-related parameters

### 5. Use Search Efficiently

- Type partial names to find parameters quickly
- Use consistent naming conventions in your shared parameter file

### 6. Test with One Parameter First

Before bulk-adding parameters:
1. Select one test parameter
2. Configure and add it
3. Verify it appears correctly in Revit
4. Then proceed with bulk operations

---

## Troubleshooting

### Parameters Don't Appear in Project

**Check**:
- Is the shared parameter file still loaded in Revit?
- Are you looking in the correct category?
- Did the "Add Parameters" operation succeed? (Check the results dialog)

### "No Valid Categories" Error

**Solution**:
- Select at least one category before adding parameters
- Ensure the selected categories exist in your Revit template

### "Failed to Update" Message

**Possible Causes**:
- Parameter is locked by another user (workshared project)
- Insufficient permissions
- Parameter definition changed in shared parameter file

**Solution**:
- Ensure you have edit rights
- Synchronize with central (workshared projects)
- Verify shared parameter file integrity

### Preset Not Loading

**Check**:
- Does the preset file still exist at `%AppData%\ICLParameterPro\Presets\`?
- Is the JSON file valid? (Open in text editor to verify)
- Do the parameters in the preset exist in your current shared parameter file?

### Categories Not Merging

**Verify**:
- Both "Update Existing Parameters" and "Merge Categories" are checked
- Or just "Merge Categories" is checked (it works independently)
- Parameter name exactly matches existing parameter

---

## Keyboard Shortcuts

- **Shift + Click**: Range selection in parameter list
- **Ctrl + F**: Focus search box (when available)

---

## Parameter Results Dialog

After adding parameters, a results dialog shows:

### Successfully Added/Updated Parameters
Parameters that were added or had their categories merged successfully

### Skipped Existing Parameters
Parameters that already exist but weren't updated (when "Update Existing" is unchecked)

### Failed Parameters
Parameters that couldn't be added due to errors, with error messages for troubleshooting

---

## Supported Revit Versions

- **Revit 2023**: .NET Framework 4.8
- **Revit 2024**: .NET Framework 4.8
- **Revit 2025**: .NET 8.0
- **Revit 2026**: .NET 8.0

---

## About Presets

Presets are stored as JSON files containing:
- List of parameter names
- Category bindings for each parameter
- Parameter group assignment
- Binding type (Instance/Type)

You can share preset files with colleagues by copying the JSON files from your presets folder.

---

## Additional Resources

- **Project Repository**: Check for updates and report issues
- **Shared Parameter File**: Ensure your team uses a standardized shared parameter file
- **Training Materials**: Contact your BIM manager for company-specific workflows

---

## Version History

### Latest Version
- Removed default "Views" and "Sheets" categories - full user control
- Improved selection behavior - no need to hold Ctrl for multi-select
- Added "Merge Categories" feature for incremental category addition
- Enhanced preset management
- Multi-version Revit support (2023-2026)

---

## Support

For issues, feature requests, or questions:
- Contact your IT/BIM department
- Check the repository for known issues
- Review this documentation for common solutions

---

**BIM Kraft Parameter Pro** - Simplifying Parameter Management in Revit

*Documentation Version 1.2 - 2024*
