using System;
using System.Reflection;
using System.Windows.Media.Imaging;
using Autodesk.Revit.UI;

namespace BIMKraft
{
    public class BIMKraftRibbonApplication : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                // Use BIMKraft tab name
                string tabName = "BIMKraft";

                try
                {
                    application.CreateRibbonTab(tabName);
                }
                catch
                {
                    // Tab might already exist, that's okay
                }

                // Get assembly path
                string assemblyPath = Assembly.GetExecutingAssembly().Location;

                // Create Parameter Tools Panel
                RibbonPanel parameterPanel;
                try
                {
                    parameterPanel = application.CreateRibbonPanel(tabName, "Parameter Tools");
                }
                catch
                {
                    // Panel might exist, try to get it
                    parameterPanel = GetRibbonPanel(application, tabName, "Parameter Tools");
                }

                if (parameterPanel != null)
                {
                    CreateParameterProButton(parameterPanel, assemblyPath);
                    CreateParameterTransferProButton(parameterPanel, assemblyPath);
                }

                // Create Workset Tools Panel
                RibbonPanel worksetPanel;
                try
                {
                    worksetPanel = application.CreateRibbonPanel(tabName, "Workset Tools");
                }
                catch
                {
                    // Panel might exist, try to get it
                    worksetPanel = GetRibbonPanel(application, tabName, "Workset Tools");
                }

                if (worksetPanel != null)
                {
                    CreateWorksetButtons(worksetPanel, assemblyPath);
                }

                // Create Quality Tools Panel
                RibbonPanel qualityPanel;
                try
                {
                    qualityPanel = application.CreateRibbonPanel(tabName, "Quality Tools");
                }
                catch
                {
                    // Panel might exist, try to get it
                    qualityPanel = GetRibbonPanel(application, tabName, "Quality Tools");
                }

                if (qualityPanel != null)
                {
                    CreateWarningsBrowserProButton(qualityPanel, assemblyPath);
                }

                // Create Measurement Tools Panel
                RibbonPanel measurementPanel;
                try
                {
                    measurementPanel = application.CreateRibbonPanel(tabName, "Measurement Tools");
                }
                catch
                {
                    // Panel might exist, try to get it
                    measurementPanel = GetRibbonPanel(application, tabName, "Measurement Tools");
                }

                if (measurementPanel != null)
                {
                    CreateLineLengthCalculatorButton(measurementPanel, assemblyPath);
                }

                // Create Family Tools Panel
                RibbonPanel familyPanel;
                try
                {
                    familyPanel = application.CreateRibbonPanel(tabName, "Family Tools");
                }
                catch
                {
                    // Panel might exist, try to get it
                    familyPanel = GetRibbonPanel(application, tabName, "Family Tools");
                }

                if (familyPanel != null)
                {
                    CreateFamilyRenamerButton(familyPanel, assemblyPath);
                }

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", "Failed to create BIM Kraft ribbon:\n" + ex.Message);
                return Result.Failed;
            }
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            // Cleanup if needed
            return Result.Succeeded;
        }

        private RibbonPanel GetRibbonPanel(UIControlledApplication application, string tabName, string panelName)
        {
            try
            {
                foreach (RibbonPanel panel in application.GetRibbonPanels(tabName))
                {
                    if (panel.Name == panelName)
                    {
                        return panel;
                    }
                }
            }
            catch
            {
                // Ignore
            }
            return null;
        }

        private void CreateParameterProButton(RibbonPanel panel, string assemblyPath)
        {
            PushButtonData buttonData = new PushButtonData(
                "BIMKraftParameterPro",
                "Parameter\nPro",
                assemblyPath,
                "BIMKraft.Commands.ParameterTools.AddParameterProCommand"
            );

            buttonData.ToolTip = "BIM Kraft Parameter Pro - Advanced Parameter Management";
            buttonData.LongDescription =
                "Manage project parameters with advanced features:\n" +
                "• Select from shared parameters\n" +
                "• Configure binding types (Instance/Type)\n" +
                "• Apply to multiple categories\n" +
                "• Save and load presets\n" +
                "• Search and filter parameters";

            panel.AddItem(buttonData);
        }

        private void CreateParameterTransferProButton(RibbonPanel panel, string assemblyPath)
        {
            PushButtonData buttonData = new PushButtonData(
                "BIMKraftParameterTransferPro",
                "Parameter\nTransfer Pro",
                assemblyPath,
                "BIMKraft.Commands.ParameterTools.ParameterTransferProCommand"
            );

            buttonData.ToolTip = "BIM Kraft Parameter Transfer Pro - Advanced Parameter Value Mapping & Transfer";
            buttonData.LongDescription =
                "Transfer parameter values with advanced mapping capabilities:\n" +
                "• Map built-in or project parameters to custom parameters\n" +
                "• Category-based mapping interface\n" +
                "• Configure mappings for each category (Walls, Doors, etc.)\n" +
                "• Save and load mapping presets\n" +
                "• Bulk transfer with validation\n" +
                "• Perfect for IFC parameter mapping (Pset_Item Width, etc.)";

            panel.AddItem(buttonData);
        }

        private void CreateWorksetButtons(RibbonPanel panel, string assemblyPath)
        {
            // Workset Manager - Unified workset configuration tool
            PushButtonData worksetManagerButton = new PushButtonData(
                "BIMKraftWorksetManager",
                "Workset\nManager",
                assemblyPath,
                "BIMKraft.Commands.WorksetTools.WorksetManagerCommand"
            );

            worksetManagerButton.ToolTip = "BIM Kraft Workset Manager - Configure & Apply Rule-Based Workset Assignments";
            worksetManagerButton.LongDescription =
                "Powerful workset assignment tool with configurable rules:\n" +
                "• Load BIMKraft default presets (Raster & Ebenen, Referenzen, Architektur, etc.)\n" +
                "• Create custom workset configurations with rule-based assignments\n" +
                "• Rules based on: Categories, Element Classes, Parameter Values, Type/Family Names\n" +
                "• Flexible comparisons: Equals, Contains, StartsWith, GreaterThan, etc.\n" +
                "• Multiple rules per workset (AND logic)\n" +
                "• Enable/disable individual rules or entire configurations\n" +
                "• Preview assignments before applying\n" +
                "• Batch apply multiple workset assignments at once\n" +
                "• Power & Speed - Configure once, apply consistently across projects!";

            panel.AddItem(worksetManagerButton);
        }

        private void CreateWarningsBrowserProButton(RibbonPanel panel, string assemblyPath)
        {
            PushButtonData buttonData = new PushButtonData(
                "BIMKraftWarningsBrowserPro",
                "Warnings\nBrowser Pro",
                assemblyPath,
                "BIMKraft.Commands.QualityTools.WarningsBrowserProCommand"
            );

            buttonData.ToolTip = "BIM Kraft Warnings Browser Pro - Advanced Warning Analysis";
            buttonData.LongDescription =
                "Browse and analyze Revit warnings with advanced features:\n" +
                "• View warnings with detailed element information\n" +
                "• Group similar warnings by occurrence\n" +
                "• Highlight elements in red with auto-highlighting\n" +
                "• Save highlights for persistent visibility\n" +
                "• Zoom to elements in current or best view\n" +
                "• Create isolated 3D views with crop box\n" +
                "• Filter by category and search\n" +
                "• Statistics dashboard\n" +
                "• Export to HTML or CSV/Excel\n" +
                "• Expand/collapse warning groups";

            panel.AddItem(buttonData);
        }

        private void CreateLineLengthCalculatorButton(RibbonPanel panel, string assemblyPath)
        {
            PushButtonData buttonData = new PushButtonData(
                "BIMKraftLineLengthCalculator",
                "Line Length\nCalculator",
                assemblyPath,
                "BIMKraft.Commands.MeasurementTools.LineLengthCalculatorCommand"
            );

            buttonData.ToolTip = "BIM Kraft Line Length Calculator - Calculate Connected Line Lengths";
            buttonData.LongDescription =
                "Calculate total length of connected Detail Lines and Model Lines:\n" +
                "• Select specific lines or calculate all in view\n" +
                "• Auto-detect connected lines by endpoint and style\n" +
                "• Group connected lines and calculate total lengths\n" +
                "• Assign colors to line groups automatically\n" +
                "• Modify colors in UI table\n" +
                "• Apply colors to lines in Revit\n" +
                "• Export to Excel or CSV\n" +
                "• Copy data to clipboard\n" +
                "• Zoom to selected line groups\n" +
                "• Statistics dashboard";

            panel.AddItem(buttonData);
        }

        private void CreateFamilyRenamerButton(RibbonPanel panel, string assemblyPath)
        {
            PushButtonData buttonData = new PushButtonData(
                "BIMKraftFamilyRenamer",
                "Family\nRenamer",
                assemblyPath,
                "BIMKraft.Commands.FamilyTools.FamilyRenamerCommand"
            );

            buttonData.ToolTip = "BIM Kraft Family Renamer - Batch Rename Families with Power & Speed";
            buttonData.LongDescription =
                "Rename system families and loadable families in bulk:\n" +
                "• System Families: Duplicate with new names (Walls, Floors, Roofs, etc.)\n" +
                "• Loadable Families: Rename directly\n" +
                "• Apply naming conventions (prefix, suffix, find/replace)\n" +
                "• Skelettbau naming convention for structural families\n" +
                "• Auto-detect material codes (HEA, IPE, STB, etc.)\n" +
                "• Category-based filtering\n" +
                "• Preview changes before applying\n" +
                "• Delete original system types (optional)\n" +
                "• Power & Speed - Rename hundreds of families in seconds!";

            panel.AddItem(buttonData);
        }
    }
}
