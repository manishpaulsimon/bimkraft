using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BIMKraft.Windows;

namespace BIMKraft.Commands.ParameterTools
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class ParameterTransferProCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                UIApplication uiApp = commandData.Application;
                UIDocument uiDoc = uiApp.ActiveUIDocument;
                Document doc = uiDoc.Document;

                // Show the parameter transfer window
                ParameterTransferWindow window = new ParameterTransferWindow(doc);
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
