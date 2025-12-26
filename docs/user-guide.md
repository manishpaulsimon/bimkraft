# BIMKraft User Guide

**Version 0.1.0**
**Last Updated: December 26, 2025**

Welcome to BIMKraft - your comprehensive toolkit for Autodesk Revit productivity!

## Table of Contents

1. [Getting Started](#getting-started)
2. [Parameter Pro](#parameter-pro)
3. [Parameter Transfer Pro](#parameter-transfer-pro)
4. [Workset Manager](#workset-manager)
5. [Warnings Browser Pro](#warnings-browser-pro)
6. [Line Length Calculator](#line-length-calculator)
7. [Family Renamer](#family-renamer)
8. [Tips & Best Practices](#tips--best-practices)
9. [Troubleshooting](#troubleshooting)
10. [Keyboard Shortcuts](#keyboard-shortcuts)

---

## Getting Started

### Installation

1. **Download** the BIMKraft MSI installer from [bimkraft.de/download](https://bimkraft.de/download)
2. **Run** the installer - it will automatically install to:
   - `C:\ProgramData\Autodesk\ApplicationPlugins\BIMKraft.bundle\`
   - `.addin` files to `%AppData%\Autodesk\Revit\Addins\[Version]\`
3. **Restart** Revit to load BIMKraft
4. **Find** the BIMKraft ribbon tab in Revit

### System Requirements

- **Revit**: 2023, 2024, 2025, or 2026
- **Operating System**: Windows 10 or Windows 11 (64-bit)
- **.NET**: Framework 4.7.2+ (for Revit 2023-2024) or .NET 8.0 (for Revit 2025-2026)
- **Disk Space**: 50 MB
- **RAM**: 8 GB minimum, 16 GB recommended

### The BIMKraft Ribbon

After installation, you'll see the **BIMKraft** tab in the Revit ribbon with 5 panels:

- **Parameter Tools**: Parameter Pro, Parameter Transfer Pro
- **Workset Tools**: Workset Manager
- **Quality Tools**: Warnings Browser Pro
- **Measurement Tools**: Line Length Calculator
- **Family Tools**: Family Renamer

---

## Parameter Pro

**Purpose**: Advanced parameter management with batch editing, filtering, and smart sorting.

### Key Features

- View all project parameters in one place
- Batch edit parameter values across multiple elements
- Advanced filtering and search
- Create and manage shared parameters
- Import/Export parameter configurations
- Save presets for common workflows

### How to Use

#### Opening Parameter Pro

1. Click **Parameter Pro** in the BIMKraft ribbon
2. The Parameter Manager window opens

#### Viewing Parameters

- **All Parameters** are listed in the main table
- **Columns** show:
  - Parameter Name
  - Type (Text, Number, Yes/No, etc.)
  - Group (Dimensions, Identity Data, etc.)
  - Shared (Yes/No)
  - GUID (for shared parameters)

#### Filtering Parameters

1. Use the **Search** box to filter by name
2. Type any part of the parameter name
3. Results update instantly

#### Adding Parameters

1. Click **Add Parameter**
2. Choose:
   - **Project Parameter**: Available in this project only
   - **Shared Parameter**: Can be shared across projects
3. Fill in:
   - Name
   - Type (Text, Integer, Number, etc.)
   - Group
   - Categories (which element types can use this parameter)
4. Click **OK**

#### Editing Parameters

1. Select a parameter from the list
2. Click **Edit**
3. Modify properties
4. Click **OK** to save

#### Batch Operations

1. Select multiple parameters (Ctrl+Click or Shift+Click)
2. Right-click for options:
   - Delete selected
   - Export to file
   - Apply to categories

#### Saving Presets

1. Configure your parameters
2. Click **Save Preset**
3. Give it a name (e.g., "Structural Parameters")
4. Click **Save**

**Loading Presets:**
1. Click **Load Preset**
2. Select from the dropdown
3. Click **Load**

### Best Practices

✅ **Use shared parameters** for data that needs to be in schedules or exported
✅ **Organize by groups** - use consistent group names across projects
✅ **Save presets** for project types you work on frequently
✅ **Test on a small dataset** before batch operations

❌ **Don't delete** parameters that are used in formulas or schedules
❌ **Avoid special characters** in parameter names (use underscores instead)

---

## Parameter Transfer Pro

**Purpose**: Map and transfer parameter values between elements with flexible configuration.

### Key Features

- Transfer parameter values from source to target elements
- Flexible mapping (source parameter → target parameter)
- Filter by category, type, or selection
- Save mapping configurations as presets
- Batch processing for large projects

### How to Use

#### Opening Parameter Transfer Pro

1. Click **Parameter Transfer Pro** in the BIMKraft ribbon
2. The Parameter Transfer window opens

#### Setting Up a Transfer

**Step 1: Select Source Elements**
1. Click **Select Source Elements**
2. In Revit, select the elements with values you want to copy
3. Press **Finish** or **Esc**

**Step 2: Select Target Elements**
1. Click **Select Target Elements**
2. Select the elements that will receive the values
3. Press **Finish** or **Esc**

**Step 3: Configure Mapping**
1. In the mapping table, set up parameter pairs:
   - **Source Parameter**: Parameter to copy FROM
   - **Target Parameter**: Parameter to copy TO
2. Add multiple mappings by clicking **Add Mapping**

**Step 4: Execute Transfer**
1. Review your mapping
2. Click **Transfer**
3. Confirm the operation
4. BIMKraft will transfer the values

#### Example Use Cases

**Use Case 1: Copy Comments from Walls to Doors**
- Source: Walls (with "Comments" filled in)
- Target: Doors in those walls
- Mapping: Wall.Comments → Door.Comments

**Use Case 2: Transfer Custom Data**
- Source: Structural columns (with "Load" parameter)
- Target: Foundation pads
- Mapping: Column.Load → Pad.Design_Load

#### Saving Transfer Presets

1. Configure your mapping
2. Click **Save Preset**
3. Name it (e.g., "Wall to Door Transfer")
4. Click **Save**

**Loading Presets:**
- Select preset from dropdown
- Click **Load Preset**
- Modify if needed

### Best Practices

✅ **Use consistent parameter names** across element types
✅ **Test with a few elements** before batch transfer
✅ **Save presets** for recurring workflows
✅ **Check parameter types match** (Text→Text, Number→Number)

❌ **Don't transfer incompatible types** (Text to Number will fail)
❌ **Avoid overwriting critical system parameters**

---

## Workset Manager

**Purpose**: Rule-based workset assignment for efficient collaboration and organization.

### Key Features

- Assign elements to worksets based on rules
- Multiple rule types: Category, Class, Parameter Value, Type Name, Family Name
- Built-in presets for common scenarios
- Bulk operations across entire project
- Automatic workset creation if needed

### How to Use

#### Opening Workset Manager

1. **Ensure** your project is workshared (File → Collaborate)
2. Click **Workset Manager** in the BIMKraft ribbon
3. The Workset Manager window opens

#### Understanding Worksets

BIMKraft organizes elements using worksets for:
- **Collaboration**: Multiple team members working simultaneously
- **Visibility**: Show/hide groups of elements
- **Ownership**: Control who can modify elements

#### Loading Built-in Presets

BIMKraft includes 6 presets:

1. **BIMKraft_ALL_Raster-Ebenen** - Grids and Levels
2. **BIMKraft_ALL_Referenzen** - Linked files and references
3. **BIMKraft_ARC** - Architectural elements (non-structural)
4. **BIMKraft_TWP_Bewehrung** - Reinforcement elements
5. **BIMKraft_TWP_Rohbau** - Structural shell (walls, floors, columns)
6. **BIMKraft_TWP_Stahl** - Steel structural elements

**To Load a Preset:**
1. Click **Load Preset** dropdown
2. Select a preset
3. Click **Load**
4. Review the rules
5. Click **Process** to apply

#### Creating Custom Worksets

**Step 1: Create Workset Configuration**
1. Click **New Workset**
2. Enter:
   - **Workset Name**: e.g., "MEP_HVAC"
   - **Description**: e.g., "HVAC equipment and ducts"

**Step 2: Add Rules**
1. Click **Add Rule**
2. Choose rule type:
   - **Category**: By Revit category (Walls, Doors, Windows, etc.)
   - **Element Class**: By programmatic type
   - **Parameter Value**: If parameter equals/contains value
   - **Type Name**: By type name (contains, starts with, equals)
   - **Family Name**: By family name pattern
   - **Structural Filter**: Structural vs non-structural

3. Configure rule details:
   - **Rule Value**: Depends on rule type
   - **Comparison**: Equals, Contains, Starts With, Ends With
   - **Enabled**: Check to activate rule

**Step 3: Process**
1. Click **Process Workset**
2. BIMKraft will:
   - Create the workset if it doesn't exist
   - Find all matching elements
   - Assign them to the workset
3. Review results dialog

#### Example: Create MEP Equipment Workset

```
Workset Name: MEP_Equipment
Description: All MEP equipment (HVAC, Plumbing, Electrical)

Rules:
1. Category = Mechanical Equipment (Enabled)
2. Category = Plumbing Fixtures (Enabled)
3. Category = Electrical Equipment (Enabled)
```

#### Bulk Processing

To process all worksets at once:
1. Load multiple presets or create multiple worksets
2. Click **Load All Presets** (applies all 6 built-in presets)
3. Review and confirm

### Best Practices

✅ **Use descriptive names** with prefixes (e.g., ARC_, STR_, MEP_)
✅ **Test rules** on a small section before processing entire project
✅ **Save custom presets** for project templates
✅ **Combine rules** with AND logic (all rules must match)

❌ **Don't overlap rules** - element should belong to one workset
❌ **Avoid processing during active collaboration** - sync first

---

## Warnings Browser Pro

**Purpose**: Advanced warning analysis with smart filtering, grouping, and resolution tracking.

### Key Features

- Browse all Revit warnings in one place
- Filter by severity, category, or keywords
- Group similar warnings
- Track resolution status
- Export warnings to Excel/CSV
- Statistical analysis

### How to Use

#### Opening Warnings Browser Pro

1. Click **Warnings Browser Pro** in the BIMKraft ribbon
2. The Warnings Browser window opens
3. All project warnings load automatically

#### Understanding the Interface

**Main Table Columns:**
- **ID**: Warning number
- **Severity**: Error, Warning, Info
- **Description**: Warning message
- **Elements**: Element IDs involved
- **Category**: Element category
- **Created**: When warning was generated

**Statistics Panel:**
- Total warnings
- By severity (Errors, Warnings)
- By category breakdown

#### Filtering Warnings

**By Severity:**
- ☑️ Show Errors
- ☑️ Show Warnings
- ☐ Show Info

**By Keyword:**
1. Type in search box
2. Filter by description text
3. Results update instantly

**By Category:**
1. Use category dropdown
2. Select specific element type
3. View only warnings for that category

#### Grouping Warnings

1. Click **Group Similar**
2. BIMKraft groups warnings with:
   - Same description pattern
   - Same category
   - Same warning type
3. Expand groups to see individual instances

#### Resolving Warnings

**Option 1: Zoom to Element**
1. Select a warning
2. Click **Zoom to Element**
3. Revit zooms to the problematic element
4. Fix the issue manually

**Option 2: Batch Resolution**
- Some warnings have **Auto-Fix** button
- Click to automatically resolve if possible
- (Limited to specific warning types)

#### Exporting Warnings

**Export to Excel:**
1. Click **Export to Excel**
2. Choose save location
3. File includes:
   - All warning details
   - Element IDs
   - Categories
   - Timestamps

**Export to CSV:**
1. Click **Export to CSV**
2. Opens in any spreadsheet software
3. Good for sharing with non-Excel users

### Common Warning Types & Solutions

| Warning | Cause | Solution |
|---------|-------|----------|
| Room Not in a Properly Enclosed Region | Room boundaries not closed | Check wall joins, close gaps |
| Highlighted Walls Overlap | Two walls occupy same space | Delete duplicate or adjust location |
| Elements Have Duplicate Mark Values | Same mark number used twice | Renumber elements uniquely |
| Can't Keep Elements Joined | Incompatible geometry | Unjoin, adjust, re-join |
| Identical Instances in Same Place | Duplicate elements | Delete duplicate instances |

### Best Practices

✅ **Review warnings weekly** - don't let them accumulate
✅ **Prioritize errors** - they can cause model corruption
✅ **Export before major changes** - track what's resolved
✅ **Group similar warnings** - resolve in batches

❌ **Don't ignore warnings** - they indicate model health issues
❌ **Avoid deleting elements** to "fix" warnings without understanding

---

## Line Length Calculator

**Purpose**: Calculate connected line lengths with color-coding and export capabilities.

### Key Features

- Identify connected line groups (same style, touching endpoints)
- Calculate total length per group
- Auto-assign colors for visualization
- Apply colors to lines in Revit
- Export to Excel/CSV
- Save/load presets
- Create Revit groups from line groups

### How to Use

#### Opening Line Length Calculator

1. Open a view with Detail Lines or Model Lines
2. Click **Line Length Calculator** in the BIMKraft ribbon
3. The Line Length Calculator window opens

#### Calculating Line Lengths

**Method 1: All Lines in View**
1. Uncheck **Use Selection**
2. Click **Calculate**
3. BIMKraft finds all lines in the active view
4. Groups connected lines
5. Displays results in table

**Method 2: Selected Lines Only**
1. Click **Select Lines**
2. In Revit, select lines (Ctrl+Click for multiple)
3. Press **Finish**
4. Check **Use Selection**
5. Click **Calculate**

#### Understanding Results

The table shows:
- **Group ID**: Unique number per group
- **Line Count**: Number of lines in this group
- **Total Length**: Combined length in meters
- **Line Style**: Revit line style name
- **Line Type**: Detail Line or Model Line
- **View**: Which view (for detail lines)
- **Color**: Auto-assigned color for visualization
- **Description**: Optional user note
- **Element IDs**: Revit element IDs

#### Assigning Colors

**Auto-Assignment:**
- Colors are automatically assigned from a palette
- Each group gets a unique color
- Colors persist if you recalculate

**Manual Color Change:**
1. Click the color box in the table
2. Choose a new color
3. Color updates in table

**Apply Colors to Revit:**
1. Customize colors in table
2. Click **Apply Colors**
3. BIMKraft applies color overrides in Revit view
4. Lines now display in assigned colors

#### Adding Descriptions

1. Click in the **Description** column
2. Type a description (e.g., "North Wall Detail", "Foundation Lines")
3. Description saved with preset
4. Used as group name if creating groups

#### Creating Revit Groups

1. Add descriptions to line groups
2. Click **Create Groups**
3. BIMKraft creates Revit groups with description as name
4. Grouped lines can be copied/moved as unit

#### Exporting Data

**Export to Excel:**
1. Click **Export to Excel**
2. Creates .xlsx file with:
   - Group data table
   - Summary statistics
3. Open in Excel for analysis

**Export to CSV:**
1. Click **Export to CSV**
2. Creates .csv file (plain text)
3. Compatible with any spreadsheet software

**Copy to Clipboard:**
1. Click **Copy to Clipboard**
2. Paste into Excel, Word, or email

#### Saving Presets

**Why Save Presets:**
- Remember color assignments
- Preserve descriptions
- Recall specific line selections
- Reuse in similar views

**How to Save:**
1. Configure colors and descriptions
2. Click **Save Preset**
3. Enter preset name
4. Click **Save**

**Loading Presets:**
1. Select preset from dropdown
2. Click **Load Preset**
3. Results populate with saved data

### Practical Examples

**Example 1: Detail Line Takeoff**
- Calculate lengths of all detail lines in a wall detail
- Assign colors by line type (cut lines, beyond lines, hidden lines)
- Export to Excel for quantity takeoff

**Example 2: Foundation Plan Lines**
- Select all foundation lines
- Calculate total linear meters
- Create groups by foundation type
- Use in construction estimates

**Example 3: Site Plan Linework**
- Calculate property lines, setbacks, utilities
- Color-code by line purpose
- Export for client review

### Best Practices

✅ **Use consistent line styles** - helps with auto-grouping
✅ **Add descriptions** - makes exports more readable
✅ **Save presets** - reuse color schemes across views
✅ **Clean up endpoints** - ensure lines are properly connected

❌ **Don't mix line styles** within a logical group
❌ **Avoid overlapping lines** - can create incorrect groups

---

## Family Renamer

**Purpose**: Batch rename families with powerful naming conventions and filtering.

### Key Features

- Rename families in bulk
- Filter by category, type, or selection
- Naming conventions: Prefix, Suffix, Find & Replace, Number sequences
- Preview before applying
- Undo support
- Export family list

### How to Use

#### Opening Family Renamer

1. Click **Family Renamer** in the BIMKraft ribbon
2. The Family Renamer window opens
3. All project families load

#### Selecting Families to Rename

**Method 1: Category Filter**
1. Choose category from dropdown (Walls, Doors, Windows, Furniture, etc.)
2. All families in that category appear
3. Select families to rename

**Method 2: Selection**
1. Click **Select in Model**
2. Pick elements in Revit
3. Their families appear in list

**Method 3: Search**
1. Type in search box
2. Filter families by current name
3. Select from filtered results

#### Renaming Operations

**Operation 1: Add Prefix**
1. Select families
2. Choose **Add Prefix** from dropdown
3. Enter prefix (e.g., "2025_")
4. Click **Preview**
5. Review: "Door_01" → "2025_Door_01"
6. Click **Apply**

**Operation 2: Add Suffix**
1. Select families
2. Choose **Add Suffix**
3. Enter suffix (e.g., "_Type-A")
4. Preview: "Window" → "Window_Type-A"
5. Click **Apply**

**Operation 3: Find & Replace**
1. Select families
2. Choose **Find & Replace**
3. Find: "Old"
4. Replace: "New"
5. Preview: "OldDoor" → "NewDoor"
6. Click **Apply**

**Operation 4: Number Sequence**
1. Select families
2. Choose **Number Sequence**
3. Enter:
   - Starting number: 1
   - Increment: 1
   - Padding: 001, 002, ...
4. Preview: "Door_001", "Door_002", "Door_003"
5. Click **Apply**

**Operation 5: Remove Characters**
1. Choose **Remove**
2. Select:
   - First N characters
   - Last N characters
   - All numbers
   - All special characters
3. Preview changes
4. Click **Apply**

#### Preview Before Applying

**Always use Preview:**
1. Configure your operation
2. Click **Preview**
3. Review **Before** and **After** columns
4. Verify changes are correct
5. Click **Apply** to commit

**Cancel if Needed:**
- If preview looks wrong, adjust settings
- Click **Clear** to reset
- Reconfigure and preview again

#### Exporting Family List

1. Click **Export**
2. Choose format:
   - Excel (.xlsx)
   - CSV (.csv)
   - Text (.txt)
3. Includes:
   - Current family names
   - Category
   - Family type
   - Count of instances

### Naming Conventions Best Practices

**Standard Naming Format:**
```
[Project]_[Category]_[Type]_[Number]

Example:
2025_Door_SingleSwing_001
2025_Window_DoubleHung_A
PROJ_Wall_Exterior_Brick
```

**Prefix Recommendations:**
- Project code or year
- Discipline (ARC, STR, MEP)
- Phase (PHASE1, PHASE2)

**Suffix Recommendations:**
- Type variation (TypeA, TypeB)
- Performance rating
- Manufacturer code

### Common Use Cases

**Use Case 1: Project Standardization**
- Find: "Generic"
- Replace: "[ProjectName]"
- Result: All "Generic" families now have project name

**Use Case 2: Phase Identification**
- Add prefix: "PHASE2_"
- Result: Easy to filter families by phase

**Use Case 3: Sequential Numbering**
- Doors from D-001 to D-025
- Number sequence with prefix "D-" and padding 001

### Best Practices

✅ **Preview first** - always check before applying
✅ **Use consistent naming** - follow company standards
✅ **Export before renaming** - backup of original names
✅ **Start small** - test on a few families first

❌ **Don't rename system families** - can break dependencies
❌ **Avoid special characters** - use underscores or hyphens
❌ **Don't rename families loaded from standards** - coordinate with team

---

## Tips & Best Practices

### General Workflow Tips

1. **Save Your Work**: Always save before using BIMKraft tools
2. **Use Presets**: Save time by creating and reusing presets
3. **Test First**: Try operations on a small dataset before processing entire project
4. **Sync Regularly**: In workshared projects, sync before major operations
5. **Close Unused Views**: Reduce memory usage for better performance

### Performance Optimization

- **Close Background Apps**: Free up RAM for Revit
- **Use Filters**: Process only needed elements, not entire model
- **Upgrade Hardware**: 16GB+ RAM, SSD recommended
- **Regular Maintenance**: Purge unused families, compact central model

### Collaboration Tips

- **Communicate**: Tell team before bulk operations on shared models
- **Document Changes**: Use presets and exports to track what changed
- **Standard Workflows**: Establish team standards for naming, parameters, worksets
- **Training**: Ensure team knows how to use BIMKraft tools

---

## Troubleshooting

### BIMKraft Ribbon Not Showing

**Solution 1: Check Revit Version**
- BIMKraft supports Revit 2023-2026 only
- Verify your Revit version is supported

**Solution 2: Reinstall**
1. Uninstall BIMKraft
2. Restart computer
3. Reinstall from [bimkraft.de/download](https://bimkraft.de/download)
4. Restart Revit

**Solution 3: Check .addin File**
1. Navigate to `%AppData%\Autodesk\Revit\Addins\[Version]\`
2. Verify `BIMKraft.addin` exists
3. Open in text editor - check paths are correct

### "License Required" Error

**During Trial:**
- Trial period may have expired (30 days)
- Contact support@bimkraft.de for extension

**After Purchase:**
- Verify license key is activated
- Click "Manage License" in BIMKraft ribbon
- Re-enter license key if needed

### Tool Window Won't Open

**Solution:**
1. Close Revit completely
2. Delete temp files: `%LocalAppData%\Temp\`
3. Restart Revit
4. Try opening tool again

**If Persistent:**
- Check Windows Event Viewer for error details
- Contact support@bimkraft.de with error message

### "Transaction Already Started" Error

**Cause:** Another operation is in progress

**Solution:**
1. Cancel any active Revit commands
2. Close other BIMKraft windows
3. Click **Finish** or **Cancel** on any active selections
4. Try again

### Slow Performance with Large Projects

**Solutions:**
1. **Use Filters**: Process categories one at a time
2. **Close Unused Views**: Reduce memory footprint
3. **Process in Sections**: Divide project into worksets or levels
4. **Upgrade RAM**: 32GB+ for very large projects

### Export to Excel Not Working

**Solution 1: Install Excel**
- Excel must be installed for .xlsx export
- Use CSV export as alternative

**Solution 2: File Permissions**
- Ensure you have write access to save location
- Try saving to Desktop instead

**Solution 3: Close Existing File**
- Close the file if it's already open
- Excel can't overwrite open files

---

## Keyboard Shortcuts

Currently, BIMKraft tools are accessed via ribbon buttons. Future versions may include keyboard shortcuts.

**Revit Shortcuts Still Work:**
- `Ctrl+Z` - Undo (works with BIMKraft operations)
- `Esc` - Cancel selection
- `Ctrl+S` - Save project

---

## Getting Help

### Support Channels

**Email Support:**
- support@bimkraft.de
- Response time: 1-2 business days

**Documentation:**
- User Guide: https://bimkraft.de/documentation
- Video Tutorials: https://bimkraft.de/tutorials (coming soon)

**Community:**
- GitHub Issues: https://github.com/manishpaulsimon/bimkraft/issues
- Feature Requests: support@bimkraft.de

### Reporting Bugs

When reporting bugs, please include:
1. **Revit Version**: (e.g., Revit 2026)
2. **BIMKraft Version**: (e.g., 0.1.0)
3. **Operating System**: (e.g., Windows 11)
4. **Steps to Reproduce**: What you did before the error
5. **Error Message**: Full text of any error dialogs
6. **Screenshots**: If applicable

**Send to**: support@bimkraft.de

---

## Feedback & Feature Requests

We love hearing from you! Send feedback to:
- support@bimkraft.de
- GitHub: https://github.com/manishpaulsimon/bimkraft

**Tell us:**
- What features you'd like to see
- How BIMKraft has helped your workflow
- Any pain points or frustrations
- Ideas for improvement

---

**Thank you for using BIMKraft!**

*For the latest updates, visit [bimkraft.de](https://bimkraft.de)*

---

*Version 1.0 - December 26, 2025*
