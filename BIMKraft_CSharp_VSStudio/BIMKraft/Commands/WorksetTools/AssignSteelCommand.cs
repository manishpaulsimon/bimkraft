using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BIMKraft.Services;
using System;
using System.Collections.Generic;

namespace BIMKraft.Commands.WorksetTools
{
    /// <summary>
    /// Command to assign steel structural elements (beams, columns, connections, plates) to the BIMKraft_TWP_Stahl workset.
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class AssignSteelCommand : IExternalCommand
    {
        private const string TARGET_WORKSET_NAME = "BIMKraft_TWP_Stahl";

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
                using (Transaction trans = new Transaction(doc, "Assign Steel Elements to Workset"))
                {
                    trans.Start();

                    int assignedCount = 0;
                    int errorCount = 0;
                    HashSet<string> processedCategories = new HashSet<string>();

                    // Define steel structural categories
                    List<BuiltInCategory> steelCategories = new List<BuiltInCategory>
                    {
                        BuiltInCategory.OST_StructuralFraming,          // Steel Beams, Braces, etc.
                        BuiltInCategory.OST_StructuralColumns,          // Steel Columns
                        BuiltInCategory.OST_Truss                       // Fachwerkbinder (Trusses)
                    };

                    // Process each steel category
                    foreach (BuiltInCategory category in steelCategories)
                    {
                        worksetService.ProcessCategory(
                            category,
                            targetWorkset,
                            ref assignedCount,
                            ref errorCount,
                            processedCategories);
                    }

                    trans.Commit();

                    // Show results
                    WorksetService.ShowResultsDialog(
                        "Steel Element Assignment Complete!",
                        TARGET_WORKSET_NAME,
                        "Stahlbauteile",
                        assignedCount,
                        errorCount,
                        processedCategories,
                        "No steel elements found or all elements were already assigned to this workset.");
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
