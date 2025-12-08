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
            // BIMKraft_ALL_Raster - Grids and Levels
            PushButtonData rastersButton = new PushButtonData(
                "BIMKraft_ALL_Raster",
                "Raster &\nEbenen",
                assemblyPath,
                "BIMKraft.Commands.WorksetTools.AssignGridsLevelsCommand"
            );
            rastersButton.ToolTip = "Assign Grids and Levels to Workset";
            rastersButton.LongDescription = "Assigns all grid and level elements to the BIMKraft_ALL_Raster-Ebenen workset.";

            // BIMKraft_ALL_Referenzen - References/Links
            PushButtonData referencesButton = new PushButtonData(
                "BIMKraft_ALL_Referenzen",
                "Referenzen",
                assemblyPath,
                "BIMKraft.Commands.WorksetTools.AssignReferencesCommand"
            );
            referencesButton.ToolTip = "Assign All References to Workset";
            referencesButton.LongDescription = "Assigns all linked files (RVT, IFC, DWG, PDF, images, point clouds, etc.) to the BIMKraft_ALL_Referenzen workset.";

            // BIMKraft_ARC - Architectural Elements
            PushButtonData arcButton = new PushButtonData(
                "BIMKraft_ARC",
                "Architektur",
                assemblyPath,
                "BIMKraft.Commands.WorksetTools.AssignArchitecturalCommand"
            );
            arcButton.ToolTip = "Assign Architectural Elements to Workset";
            arcButton.LongDescription = "Assigns architectural elements (doors, windows, non-structural walls, furniture, etc.) to the BIMKraft_ARC workset.";

            // BIMKraft_TWP_Bewehrung - Reinforcement
            PushButtonData reinforcementButton = new PushButtonData(
                "BIMKraft_TWP_Bewehrung",
                "Bewehrung",
                assemblyPath,
                "BIMKraft.Commands.WorksetTools.AssignReinforcementCommand"
            );
            reinforcementButton.ToolTip = "Assign Reinforcement Elements to Workset";
            reinforcementButton.LongDescription = "Assigns reinforcement elements (rebar, fabric sheets, fabric areas) to the BIMKraft_TWP_Bewehrung workset.";

            // BIMKraft_TWP_Rohbau - Structural Shell
            PushButtonData rohbauButton = new PushButtonData(
                "BIMKraft_TWP_Rohbau",
                "Rohbau",
                assemblyPath,
                "BIMKraft.Commands.WorksetTools.AssignStructuralShellCommand"
            );
            rohbauButton.ToolTip = "Assign Structural Shell Elements to Workset";
            rohbauButton.LongDescription = "Assigns structural shell elements (bearing walls, structural floors, columns, foundations) to the BIMKraft_TWP_Rohbau workset.";

            // BIMKraft_TWP_Stahl - Steel
            PushButtonData steelButton = new PushButtonData(
                "BIMKraft_TWP_Stahl",
                "Stahl",
                assemblyPath,
                "BIMKraft.Commands.WorksetTools.AssignSteelCommand"
            );
            steelButton.ToolTip = "Assign Steel Elements to Workset";
            steelButton.LongDescription = "Assigns steel structural elements (beams, columns, connections, plates) to the BIMKraft_TWP_Stahl workset.";

            // Add all buttons to the panel
            panel.AddItem(rastersButton);
            panel.AddItem(referencesButton);
            panel.AddItem(arcButton);
            panel.AddItem(reinforcementButton);
            panel.AddItem(rohbauButton);
            panel.AddItem(steelButton);
        }
    }
}
