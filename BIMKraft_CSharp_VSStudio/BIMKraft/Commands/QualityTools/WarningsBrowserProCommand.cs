using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BIMKraft.Windows;

namespace BIMKraft.Commands.QualityTools
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class WarningsBrowserProCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
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

                // Get warnings from document
                var warnings = doc.GetWarnings();

                if (warnings.Count == 0)
                {
                    TaskDialog mainDialog = new TaskDialog("Warnings Browser Pro")
                    {
                        MainIcon = TaskDialogIcon.TaskDialogIconInformation,
                        MainInstruction = "No Warnings Found",
                        MainContent = "Congratulations! Your project has no warnings.",
                        CommonButtons = TaskDialogCommonButtons.Ok
                    };
                    mainDialog.Show();
                    return Result.Succeeded;
                }

                // Show the warnings browser window
                WarningsBrowserWindow window = new WarningsBrowserWindow(doc, uiDoc);
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
                    "An error occurred while opening Warnings Browser:\n\n" +
                    ex.Message + "\n\n" +
                    ex.StackTrace);
                return Result.Failed;
            }
        }
    }
}
