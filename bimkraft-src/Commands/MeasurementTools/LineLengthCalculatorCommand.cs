using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BIMKraft.Windows;
using BIMKraft.Services;

namespace BIMKraft.Commands.MeasurementTools
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class LineLengthCalculatorCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                // Check license
                if (!LicenseManager.IsFeatureAvailable("all"))
                {
                    TaskDialog.Show(
                        "License Required",
                        "Your BIMKraft license has expired or is invalid.\n\n" +
                        "Please activate a license or start a trial to use this feature.\n\n" +
                        "Use the 'Manage License' button in the BIMKraft ribbon."
                    );
                    return Result.Cancelled;
                }

                UIApplication uiApp = commandData.Application;
                UIDocument uiDoc = uiApp.ActiveUIDocument;
                Document doc = uiDoc.Document;

                if (doc == null)
                {
                    TaskDialog.Show("No Document",
                        "No active Revit document found.\n\n" +
                        "Please open a Revit project and try again.");
                    return Result.Cancelled;
                }

                // Check if we're in a 3D view or a view that doesn't support detail lines
                View activeView = doc.ActiveView;
                if (activeView == null)
                {
                    TaskDialog.Show("No Active View",
                        "No active view found.\n\n" +
                        "Please ensure you have an active view and try again.");
                    return Result.Cancelled;
                }

                // Show the line length calculator window
                LineLengthCalculatorWindow window = new LineLengthCalculatorWindow(doc, uiDoc);
                bool? dialogResult = window.ShowDialog();

                if (dialogResult == true)
                {
                    return Result.Succeeded;
                }
                else
                {
                    return Result.Cancelled;
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
                TaskDialog.Show("Error",
                    "An error occurred while opening Line Length Calculator:\n\n" +
                    ex.Message + "\n\n" +
                    ex.StackTrace);
                return Result.Failed;
            }
        }
    }
}
