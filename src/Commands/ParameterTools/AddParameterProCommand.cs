using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BIMKraft.Windows;

namespace BIMKraft.Commands.ParameterTools
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class AddParameterProCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                UIApplication uiApp = commandData.Application;
                UIDocument uiDoc = uiApp.ActiveUIDocument;
                Document doc = uiDoc.Document;

                // Check if shared parameter file is loaded
                if (uiApp.Application.OpenSharedParameterFile() == null)
                {
                    TaskDialog.Show("No Shared Parameters",
                        "No shared parameter file is loaded.\n\n" +
                        "Please set up your shared parameter file first:\n" +
                        "1. Go to Manage > Shared Parameters\n" +
                        "2. Browse and select your .txt shared parameter file\n" +
                        "3. Run this tool again");
                    return Result.Cancelled;
                }

                // Show the parameter manager window
                ParameterManagerWindow window = new ParameterManagerWindow(doc);
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
                TaskDialog.Show("Error", "An error occurred:\n\n" + ex.Message + "\n\n" + ex.StackTrace);
                return Result.Failed;
            }
        }
    }
}
