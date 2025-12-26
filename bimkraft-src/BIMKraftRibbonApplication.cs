using System;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;
using Autodesk.Revit.UI;
using BIMKraft.Models;
using BIMKraft.Services;
using BIMKraft.UI.Licensing;

namespace BIMKraft
{
    public class BIMKraftRibbonApplication : IExternalApplication
    {
        /// <summary>
        /// Load an icon from the Resources/Icons folder
        /// </summary>
        private BitmapImage LoadIcon(string iconName)
        {
            try
            {
                string assemblyPath = Assembly.GetExecutingAssembly().Location;
                string assemblyDir = Path.GetDirectoryName(assemblyPath);
                string iconPath = Path.Combine(assemblyDir, "Resources", "Icons", iconName);

                if (File.Exists(iconPath))
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(iconPath);
                    bitmap.EndInit();
                    return bitmap;
                }
            }
            catch
            {
                // Icon loading failed, button will show without icon
            }
            return null;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                // Validate license at startup
                LicenseInfo licenseInfo = LicenseManager.ValidateLicense();

                // Check license status
                if (licenseInfo.Status == LicenseStatus.Invalid)
                {
                    // No license or trial - prompt user to start trial or activate
                    TaskDialogResult result = TaskDialog.Show(
                        "BIMKraft License",
                        "Welcome to BIMKraft!\n\n" +
                        "You need a license to use BIMKraft.\n\n" +
                        "Would you like to start a free 30-day trial?",
                        TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No
                    );

                    if (result == TaskDialogResult.Yes)
                    {
                        // Start trial
                        try
                        {
                            TrialInfo trial = LicenseManager.StartTrial();
                            TaskDialog.Show(
                                "Trial Started",
                                $"Your 30-day free trial has begun!\n\n" +
                                $"Trial ends on: {trial.TrialEndDate:yyyy-MM-dd}\n\n" +
                                $"Enjoy full access to all BIMKraft features!"
                            );
                        }
                        catch (Exception ex)
                        {
                            TaskDialog.Show("Error", "Failed to start trial:\n\n" + ex.Message);
                            return Result.Failed;
                        }
                    }
                    else
                    {
                        // Show license activation window
                        LicenseActivationWindow licenseWindow = new LicenseActivationWindow();
                        bool? dialogResult = licenseWindow.ShowDialog();

                        if (dialogResult != true)
                        {
                            // User cancelled - don't load ribbon
                            return Result.Cancelled;
                        }
                    }
                }
                else if (licenseInfo.Status == LicenseStatus.Expired)
                {
                    // License expired - show activation window
                    TaskDialog.Show(
                        "License Expired",
                        "Your BIMKraft license has expired.\n\n" +
                        "Please renew your license to continue using BIMKraft.",
                        TaskDialogCommonButtons.Ok
                    );

                    LicenseActivationWindow licenseWindow = new LicenseActivationWindow();
                    bool? dialogResult = licenseWindow.ShowDialog();

                    if (dialogResult != true)
                    {
                        return Result.Cancelled;
                    }
                }
                else if (licenseInfo.Status == LicenseStatus.Trial)
                {
                    // Check if we should show trial reminder
                    if (LicenseManager.ShouldShowTrialReminder(out int daysRemaining))
                    {
                        TaskDialog.Show(
                            "BIMKraft Trial Reminder",
                            $"Your BIMKraft trial has {daysRemaining} day{(daysRemaining != 1 ? "s" : "")} remaining.\n\n" +
                            $"Purchase a license at https://bimkraft.com/pricing to continue using BIMKraft after the trial ends.",
                            TaskDialogCommonButtons.Ok
                        );
                    }
                }
                else if (licenseInfo.Status == LicenseStatus.GracePeriod)
                {
                    // Grace period - warn user
                    TaskDialog.Show(
                        "License Grace Period",
                        $"Your BIMKraft license has expired.\n\n" +
                        $"You have {licenseInfo.RemainingDays} day{(licenseInfo.RemainingDays != 1 ? "s" : "")} remaining in the grace period.\n\n" +
                        $"Please renew your license to avoid interruption.",
                        TaskDialogCommonButtons.Ok
                    );
                }

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

                // Create License Tools Panel
                RibbonPanel licensePanel;
                try
                {
                    licensePanel = application.CreateRibbonPanel(tabName, "License");
                }
                catch
                {
                    // Panel might exist, try to get it
                    licensePanel = GetRibbonPanel(application, tabName, "License");
                }

                if (licensePanel != null)
                {
                    CreateManageLicenseButton(licensePanel, assemblyPath);
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

            // Set icon
            buttonData.LargeImage = LoadIcon("parameter_pro.png");

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

            // Set icon
            buttonData.LargeImage = LoadIcon("parameter_transfer_pro.png");

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

            // Set icon
            worksetManagerButton.LargeImage = LoadIcon("workset_manager.png");

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

            // Set icon
            buttonData.LargeImage = LoadIcon("warnings_browser_pro.png");

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

            // Set icon
            buttonData.LargeImage = LoadIcon("line_length_calculator.png");

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

            // Set icon
            buttonData.LargeImage = LoadIcon("family_renamer.png");

            panel.AddItem(buttonData);
        }

        private void CreateManageLicenseButton(RibbonPanel panel, string assemblyPath)
        {
            PushButtonData buttonData = new PushButtonData(
                "BIMKraftManageLicense",
                "Manage\nLicense",
                assemblyPath,
                "BIMKraft.Commands.LicenseTools.ManageLicenseCommand"
            );

            buttonData.ToolTip = "BIMKraft License Manager - Activate License or Start Trial";
            buttonData.LongDescription =
                "Manage your BIMKraft license:\n" +
                "• View license status and expiry date\n" +
                "• Start 30-day free trial\n" +
                "• Activate paid license with license key\n" +
                "• Deactivate current license\n" +
                "• Purchase license at bimkraft.com/pricing";

            // Set icon (if available)
            buttonData.LargeImage = LoadIcon("manage_license.png");

            panel.AddItem(buttonData);
        }
    }
}
