# Quick Start Guide for Developers

## Adding a New Tool to ICLTools

This is a condensed step-by-step guide for adding a new tool. For detailed information, see [PROJECT_STRUCTURE.md](PROJECT_STRUCTURE.md).

---

## 5-Step Process

### Step 1: Create Your Command Class

**File:** `Commands/[PanelName]/YourToolCommand.cs`

**Note:** Organize commands by panel. For example:
- Parameter tools go in `Commands/ParameterTools/`
- Workset tools go in `Commands/WorksetTools/`
- Create new folders for new panels

```csharp
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace ICLTools.Commands.YourPanelName
{
    [Transaction(TransactionMode.Manual)]
    public class YourToolCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                UIDocument uiDoc = commandData.Application.ActiveUIDocument;
                Document doc = uiDoc.Document;

                var window = new Windows.YourToolWindow(doc);
                window.ShowDialog();

                return Result.Succeeded;
            }
            catch (System.Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }
    }
}
```

### Step 2: Create Your Window

**File:** `Windows/YourToolWindow.xaml`

```xml
<Window x:Class="ICLTools.Windows.YourToolWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Your Tool Name"
        Width="800" Height="600"
        WindowStartupLocation="CenterScreen">
    <Grid Margin="15">
        <!-- Your UI here -->
        <TextBlock Text="Hello from Your Tool!"
                   HorizontalAlignment="Center"
                   VerticalAlignment="Center"
                   FontSize="24"/>
    </Grid>
</Window>
```

**File:** `Windows/YourToolWindow.xaml.cs`

```csharp
using System.Windows;
using Autodesk.Revit.DB;

namespace ICLTools.Windows
{
    public partial class YourToolWindow : Window
    {
        private Document _doc;

        public YourToolWindow(Document doc)
        {
            InitializeComponent();
            _doc = doc;
        }
    }
}
```

### Step 3: Update Project File

Edit `ICLTools.csproj` and add to the appropriate ItemGroup:

```xml
<!-- Add to Compile ItemGroup -->
<ItemGroup>
    <Compile Include="Commands\YourPanelName\YourToolCommand.cs" />
    <Compile Include="Windows\YourToolWindow.xaml.cs">
        <DependentUpon>YourToolWindow.xaml</DependentUpon>
    </Compile>
</ItemGroup>

<!-- Add to Page ItemGroup -->
<ItemGroup>
    <Page Include="Windows\YourToolWindow.xaml">
        <SubType>Designer</SubType>
        <Generator>MSBuild:Compile</Generator>
    </Page>
</ItemGroup>
```

### Step 4: Register in Ribbon

Edit `ICLRibbonApplication.cs`:

**Add to OnStartup method:**
```csharp
public Result OnStartup(UIControlledApplication application)
{
    try
    {
        string tabName = "ICL";

        // Create tab (existing code)
        try { application.CreateRibbonTab(tabName); } catch { }

        // Get or create your panel
        RibbonPanel panel = application.CreateRibbonPanel(tabName, "Your Tools");

        // Add existing buttons (existing code)
        // ...

        // ADD YOUR NEW BUTTON HERE:
        CreateYourToolButton(panel);

        return Result.Succeeded;
    }
    catch (Exception ex)
    {
        TaskDialog.Show("Error", ex.Message);
        return Result.Failed;
    }
}
```

**Add the button creation method:**
```csharp
private void CreateYourToolButton(RibbonPanel panel)
{
    string assemblyPath = Assembly.GetExecutingAssembly().Location;

    PushButtonData buttonData = new PushButtonData(
        "YourToolID",              // Internal ID
        "Your Tool\nName",         // Display name (use \n for two lines)
        assemblyPath,
        "ICLTools.Commands.YourPanelName.YourToolCommand"  // Full namespace.class
    );

    buttonData.ToolTip = "Short description";
    buttonData.LongDescription =
        "Detailed description of what your tool does.\n" +
        "Can be multiple lines.";

    // Optional: Add icon
    // buttonData.LargeImage = new BitmapImage(new Uri("pack://application:,,,/ICLTools;component/Resources/Icons/YourIcon.png"));

    panel.AddItem(buttonData);
}
```

### Step 5: Build and Test

1. **Build the project:**
   ```bash
   msbuild ICLTools.csproj /p:Configuration=Debug /p:RevitVersion=2025
   ```

2. **Test in Revit:**
   - Close Revit
   - Build will copy DLL automatically (if build succeeds)
   - Open Revit
   - Look for ICL tab → Your button

3. **Debug if needed:**
   - Attach Visual Studio debugger to Revit.exe
   - Set breakpoints in your code
   - Click your button in Revit

---

## Common Patterns

### Using Transactions

```csharp
using (Transaction trans = new Transaction(doc, "My Operation"))
{
    trans.Start();

    // Your code that modifies the document

    trans.Commit();
}
```

### Error Handling

```csharp
try
{
    // Your risky operation
}
catch (Autodesk.Revit.Exceptions.OperationCanceledException)
{
    return Result.Cancelled;
}
catch (Exception ex)
{
    TaskDialog.Show("Error", $"Operation failed: {ex.Message}");
    return Result.Failed;
}
```

### Accessing UI Elements

```csharp
UIApplication uiApp = commandData.Application;
UIDocument uiDoc = uiApp.ActiveUIDocument;
Document doc = uiDoc.Document;
Autodesk.Revit.ApplicationServices.Application app = uiApp.Application;
```

---

## Folder Organization

```
ICLTools/
├── Commands/
│   ├── ParameterTools/                 ← Parameter Tools panel
│   │   ├── AddParameterProCommand.cs
│   │   └── ParameterTransferProCommand.cs
│   ├── WorksetTools/                   ← Workset Tools panel
│   │   └── [Workset commands]
│   └── YourPanelName/                  ← Your new panel
│       └── YourToolCommand.cs          ← Your new command
│
├── Windows/
│   ├── ParameterManagerWindow.xaml     ← Existing
│   └── YourToolWindow.xaml             ← Your new window
│
├── Models/
│   ├── ParameterConfig.cs              ← Shared models
│   └── YourDataModel.cs                ← Your models (if needed)
│
├── Services/                           ← Optional: shared logic
│   └── YourService.cs
│
├── Utilities/                          ← Optional: helpers
│   └── YourHelper.cs
│
└── ICLRibbonApplication.cs             ← Register your tool here
```

---

## Build Configurations

The project supports multiple Revit versions:

```bash
# Revit 2023
msbuild ICLTools.csproj /p:RevitVersion=2023

# Revit 2024
msbuild ICLTools.csproj /p:RevitVersion=2024

# Revit 2025
msbuild ICLTools.csproj /p:RevitVersion=2025

# Revit 2026
msbuild ICLTools.csproj /p:RevitVersion=2026
```

---

## Namespaces

Keep namespaces organized:

```csharp
namespace ICLTools.Commands { }    // Command classes
namespace ICLTools.Windows { }     // UI windows
namespace ICLTools.Models { }      // Data models
namespace ICLTools.Services { }    // Business logic
namespace ICLTools.Utilities { }   // Helper classes
```

---

## Testing Checklist

- [ ] Tool appears in ICL ribbon tab
- [ ] Button tooltip shows correctly
- [ ] Window opens without errors
- [ ] All UI elements render properly
- [ ] Tool performs expected operations
- [ ] Errors are handled gracefully
- [ ] Works in multiple Revit versions
- [ ] Documentation created (en & de)

---

## Documentation

After creating your tool, add documentation:

1. **English:** `docs/en/YourTool_UserGuide.md`
2. **German:** `docs/de/YourTool_Benutzerhandbuch.md`
3. Update `docs/en/README.md` and `docs/de/README.md`

---

## Tips

1. **Start Simple:** Get a basic "Hello World" window working first
2. **Copy Patterns:** Look at Parameter Pro for advanced patterns
3. **Test Frequently:** Build and test after each major change
4. **Version Control:** Commit working code before major refactoring
5. **Document:** Write docs as you develop, not after

---

## Need More Help?

- **Full Guide:** [PROJECT_STRUCTURE.md](PROJECT_STRUCTURE.md)
- **Example Tool:** Study `AddParameterProCommand.cs` and `ParameterManagerWindow.xaml`
- **Revit API:** https://www.revitapidocs.com/

---

**Happy Coding!** 🚀
