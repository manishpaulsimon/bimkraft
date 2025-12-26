using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using BIMKraft.Services;

namespace BIMKraft.Commands.WorksetTools
{
    [Transaction(TransactionMode.Manual)]
    public class WorksetManagerCommand : IExternalCommand
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

                UIDocument uidoc = commandData.Application.ActiveUIDocument;
                Document doc = uidoc.Document;

                // Check if the document is workshared
                if (!doc.IsWorkshared)
                {
                    TaskDialog.Show("Workset Manager",
                        "This document is not workshared. Worksets are only available in workshared documents.");
                    return Result.Cancelled;
                }

                // Show the Workset Manager window (modeless to allow ExternalEvent to execute)
                var window = new WorksetManagerWindow(uidoc);
                window.Show();

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                TaskDialog.Show("Error", $"An error occurred:\n{ex.Message}");
                return Result.Failed;
            }
        }
    }
}
