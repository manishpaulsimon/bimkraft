using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BIMKraft.Services;
using System;
using System.Collections.Generic;

namespace BIMKraft.Commands.WorksetTools
{
    /// <summary>
    /// Command to assign Grids and Levels to the BIMKraft_ALL_Raster-Ebenen workset.
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class AssignGridsLevelsCommand : IExternalCommand
    {
        private const string TARGET_WORKSET_NAME = "BIMKraft_ALL_Raster-Ebenen";

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                UIDocument uiDoc = commandData.Application.ActiveUIDocument;
                Document doc = uiDoc.Document;

                // Check if the document is workshared
                if (!doc.IsWorkshared)
                {
                    TaskDialog.Show("Error", "This document is not workshared. Worksets are only available in workshared models.");
                    return Result.Failed;
                }

                // Create workset service
                WorksetService worksetService = new WorksetService(doc);

                // Find or create the target workset
                Workset targetWorkset = worksetService.FindOrCreateWorkset(TARGET_WORKSET_NAME);
                if (targetWorkset == null)
                {
                    return Result.Failed;
                }

                // Start transaction for assigning elements
                using (Transaction trans = new Transaction(doc, "Assign Grids and Levels to Workset"))
                {
                    trans.Start();

                    int assignedCount = 0;
                    int errorCount = 0;
                    HashSet<string> processedCategories = new HashSet<string>();

                    // Process Grids
                    worksetService.ProcessCategory(
                        BuiltInCategory.OST_Grids,
                        targetWorkset,
                        ref assignedCount,
                        ref errorCount,
                        processedCategories);

                    // Process Levels
                    worksetService.ProcessCategory(
                        BuiltInCategory.OST_Levels,
                        targetWorkset,
                        ref assignedCount,
                        ref errorCount,
                        processedCategories);

                    trans.Commit();

                    // Show results
                    WorksetService.ShowResultsDialog(
                        "Grids and Levels Assignment Complete!",
                        TARGET_WORKSET_NAME,
                        "Achsen und Ebenen",
                        assignedCount,
                        errorCount,
                        processedCategories,
                        "No Grids or Levels found or all elements were already assigned to this workset.");
                }

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                TaskDialog.Show("Error", "Script error: " + ex.Message);
                return Result.Failed;
            }
        }
    }
}
