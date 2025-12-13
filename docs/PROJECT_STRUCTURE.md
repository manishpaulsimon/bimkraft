# BIMKraft Project Structure Guide

## Overview
This guide explains how to organize multiple tools within the BIMKraft project for Revit.

## Current Project Structure

```
BIMKraft/
├── BIMKraft.csproj                    # Main project file
├── BIMKraft.sln                       # Solution file
├── BIMKraft.addin                     # Revit addin manifest
├── App.config                         # Application configuration
├── packages.config                    # NuGet packages
├── BIMKraftRibbonApplication.cs            # **Main Ribbon Setup**
│
├── build/                             # **Build Scripts**
│   ├── build_all_versions.bat         # Build for all Revit versions
│   ├── build_2023_2024.bat            # Build for Revit 2023-2024
│   ├── build_2025.bat                 # Build for Revit 2025
│   └── build_2025_2026.bat            # Build for Revit 2025-2026
│
├── Commands/                          # **Tool Command Classes (Organized by Panel)**
│   ├── ParameterTools/                # Parameter Tools panel commands
│   │   ├── AddParameterProCommand.cs
│   │   └── ParameterTransferProCommand.cs
│   ├── WorksetTools/                  # Workset Tools panel commands
│   │   ├── AssignGridsLevelsCommand.cs
│   │   ├── AssignReferencesCommand.cs
│   │   ├── AssignArchitecturalCommand.cs
│   │   ├── AssignReinforcementCommand.cs
│   │   ├── AssignStructuralShellCommand.cs
│   │   └── AssignSteelCommand.cs
│   └── [YourPanelName]/               # Add new panel folders here
│       └── [YourCommand].cs
│
├── Windows/                           # **UI Windows for Each Tool**
│   ├── ParameterManagerWindow.xaml    # Parameter Pro UI
│   ├── ParameterManagerWindow.xaml.cs # Parameter Pro code-behind
│   ├── ParameterTransferWindow.xaml   # Parameter Transfer Pro UI
│   ├── ParameterTransferWindow.xaml.cs # Parameter Transfer Pro code-behind
│   └── [YourNewWindow].xaml           # Add new tool windows here
│
├── Models/                            # **Shared Data Models**
│   ├── ParameterConfig.cs             # Parameter Pro config model
│   ├── PresetData.cs                  # Parameter Pro preset model
│   ├── ParameterMappingConfig.cs      # Parameter Transfer Pro mapping model
│   └── [YourNewModel].cs              # Add shared models here
│
├── Services/                          # **Shared Services**
│   ├── WorksetService.cs              # Workset assignment service
│   └── [YourService].cs               # Add shared services here
│
├── Utilities/                         # **Helper Classes (Optional)**
│   └── [HelperClass].cs               # Common utilities
│
├── Resources/                         # **Icons, Images, etc.**
│   ├── Icons/
│   │   ├── ParameterPro_32x32.png
│   │   └── [YourToolIcon]_32x32.png
│   └── Images/
│
├── Properties/
│   └── AssemblyInfo.cs                # Assembly information
│
└── docs/                              # **Documentation**
    ├── en/                            # English documentation
    │   ├── README.md
    │   ├── ParameterPro_UserGuide.md
    │   └── [YourTool]_UserGuide.md
    ├── de/                            # German documentation
    │   ├── README.md
    │   ├── ParameterPro_Benutzerhandbuch.md
    │   └── [YourTool]_Benutzerhandbuch.md
    ├── PROJECT_STRUCTURE.md           # This file
    ├── QUICK_START_DEVELOPERS.md      # Developer quick start guide
    ├── DOCUMENTATION_INDEX.md         # Documentation index
    └── README.md                      # Documentation overview
```

## Recommended Structure for Multiple Tools

### 1. **Organize by Feature/Tool**

Each tool should have:
- **Command class** (in `Commands/` folder)
- **Window/UI** (in `Windows/` folder)
- **Models** (in `Models/` folder, can be shared)
- **Documentation** (in `docs/`)

### 2. **Naming Convention**

Follow this pattern:
- Commands: `[ToolName]Command.cs` (e.g., `AddParameterProCommand.cs`)
- Windows: `[ToolName]Window.xaml` (e.g., `ParameterManagerWindow.xaml`)
- Models: `[EntityName].cs` (e.g., `ParameterConfig.cs`)

### 3. **BIMKraftRibbonApplication.cs**

This is the central file where all tools are registered to the Revit ribbon. Structure it like this:

```csharp
public Result OnStartup(UIControlledApplication application)
{
    // Create BIMKraft Tab (once)
    string tabName = "BIMKraft";
    try
    {
        application.CreateRibbonTab(tabName);
    }
    catch { /* Tab exists */ }

    // Create Panels for different tool categories
    RibbonPanel parameterPanel = application.CreateRibbonPanel(tabName, "Parameter Tools");
    RibbonPanel modelingPanel = application.CreateRibbonPanel(tabName, "Modeling Tools");

    // Add Tool 1: Parameter Pro
    CreateParameterProButton(parameterPanel);

    // Add Tool 2: Your Next Tool
    CreateYourNextToolButton(parameterPanel);

    // Add Tool 3: Another Tool
    CreateAnotherToolButton(modelingPanel);

    return Result.Succeeded;
}

private void CreateParameterProButton(RibbonPanel panel)
{
    string assemblyPath = Assembly.GetExecutingAssembly().Location;

    PushButtonData buttonData = new PushButtonData(
        "BIMKraftParameterPro",
        "Parameter\nPro",
        assemblyPath,
        "BIMKraftParameterPro.AddParameterProCommand"  // Full namespace.classname
    );

    buttonData.ToolTip = "BIM Kraft Parameter Pro";
    buttonData.LongDescription = "Manage project parameters...";

    // Set icon (optional)
    // buttonData.LargeImage = new BitmapImage(new Uri(...));

    panel.AddItem(buttonData);
}

private void CreateYourNextToolButton(RibbonPanel panel)
{
    // Similar structure for next tool
}
```

## Adding a New Tool - Step by Step

### Step 1: Create Command Class

Create `Commands/YourNewCommand.cs`:

```csharp
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace BIMKraft.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class YourNewCommand : IExternalCommand
    {
        public Result Execute(
            ExternalCommandData commandData,
            ref string message,
            ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            UIDocument uiDoc = uiApp.ActiveUIDocument;
            Document doc = uiDoc.Document;

            // Show your window
            var window = new YourNewWindow(doc);
            window.ShowDialog();

            return Result.Succeeded;
        }
    }
}
```

### Step 2: Create UI Window

Create `Windows/YourNewWindow.xaml`:

```xml
<Window x:Class="BIMKraft.Windows.YourNewWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Your Tool Name"
        Width="800" Height="600">
    <Grid>
        <!-- Your UI here -->
    </Grid>
</Window>
```

Create `Windows/YourNewWindow.xaml.cs`:

```csharp
using System.Windows;
using Autodesk.Revit.DB;

namespace BIMKraft.Windows
{
    public partial class YourNewWindow : Window
    {
        private Document _doc;

        public YourNewWindow(Document doc)
        {
            InitializeComponent();
            _doc = doc;
        }

        // Your event handlers and logic here
    }
}
```

### Step 3: Update Project File

Update `BIMKraft.csproj` to include new files:

```xml
<ItemGroup>
    <Compile Include="Commands\YourNewCommand.cs" />
    <Compile Include="Windows\YourNewWindow.xaml.cs">
        <DependentUpon>YourNewWindow.xaml</DependentUpon>
    </Compile>
</ItemGroup>

<ItemGroup>
    <Page Include="Windows\YourNewWindow.xaml">
        <SubType>Designer</SubType>
        <Generator>MSBuild:Compile</Generator>
    </Page>
</ItemGroup>
```

### Step 4: Register in Ribbon

Update `BIMKraftRibbonApplication.cs`:

```csharp
public Result OnStartup(UIControlledApplication application)
{
    // ... existing code ...

    // Add your new tool
    CreateYourNewToolButton(parameterPanel);

    return Result.Succeeded;
}

private void CreateYourNewToolButton(RibbonPanel panel)
{
    string assemblyPath = Assembly.GetExecutingAssembly().Location;

    PushButtonData buttonData = new PushButtonData(
        "YourToolID",
        "Your Tool\nName",
        assemblyPath,
        "BIMKraft.Commands.YourNewCommand"
    );

    buttonData.ToolTip = "Your tool description";

    panel.AddItem(buttonData);
}
```

### Step 5: Create Documentation

Create documentation files:
- `docs/en/YourTool_UserGuide.md`
- `docs/de/YourTool_Benutzerhandbuch.md`

## Best Practices

### 1. **Separation of Concerns**
- Keep UI logic in Window classes
- Keep business logic in Command classes or Services
- Keep data structures in Models

### 2. **Shared Code**
- Put reusable utilities in `Utilities/` folder
- Put shared services in `Services/` folder
- Models can be shared across tools

### 3. **Namespace Organization**
Use consistent namespaces:
```csharp
namespace BIMKraft.Commands { }
namespace BIMKraft.Windows { }
namespace BIMKraft.Models { }
namespace BIMKraft.Services { }
namespace BIMKraft.Utilities { }
```

### 4. **Resource Management**
- Store icons in `Resources/Icons/`
- Use 32x32 PNG for Revit ribbon icons
- Use relative paths when possible

### 5. **Error Handling**
Always wrap risky operations in try-catch:
```csharp
try
{
    // Your code
    return Result.Succeeded;
}
catch (Exception ex)
{
    TaskDialog.Show("Error", ex.Message);
    return Result.Failed;
}
```

## Example: Multiple Tools Setup

```
BIMKraft Tab
├── Parameter Tools Panel
│   ├── Parameter Pro        (Existing)
│   ├── Parameter Sync       (New tool)
│   └── Parameter Report     (New tool)
│
├── Modeling Tools Panel
│   ├── Smart Copy           (New tool)
│   └── Element Manager      (New tool)
│
└── Documentation Panel
    └── Help                 (New tool)
```

## Deployment

All tools compile into a single DLL (`BIMKraft.dll`):
- Easier to maintain
- Single addin file
- Shared resources and dependencies
- One ribbon tab with multiple panels/buttons

## Tips

1. **Start Simple**: Clone an existing tool structure
2. **Test Incrementally**: Build and test after each tool addition
3. **Document As You Go**: Update docs immediately
4. **Use Version Control**: Commit after each working tool
5. **Icon Consistency**: Use similar visual style for all tools

## Need Help?

Refer to existing tools as templates:
- Parameter Pro: Full-featured example with UI, presets, and configuration
