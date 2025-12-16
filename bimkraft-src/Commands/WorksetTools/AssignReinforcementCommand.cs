using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BIMKraft.Services;
using System;
using System.Collections.Generic;

namespace BIMKraft.Commands.WorksetTools
{
    /// <summary>
    /// Command to assign reinforcement elements (rebar, fabric sheets) to the BIMKraft_TWP_Bewehrung workset.
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class AssignReinforcementCommand : IExternalCommand
    {
        private const string TARGET_WORKSET_NAME = "BIMKraft_TWP_Bewehrung";

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
                using (Transaction trans = new Transaction(doc, "Assign Reinforcement Elements to Workset"))
                {
                    trans.Start();

                    int assignedCount = 0;
                    int errorCount = 0;
                    HashSet<string> processedCategories = new HashSet<string>();

                    // Define reinforcement categories
                    List<BuiltInCategory> reinforcementCategories = new List<BuiltInCategory>
                    {
                        BuiltInCategory.OST_Rebar,                      // Rebar (Reinforcing Bars)
                        BuiltInCategory.OST_FabricReinforcement,        // Fabric Sheets
                        BuiltInCategory.OST_FabricAreas,                // Fabric Areas (for layout)
                        BuiltInCategory.OST_FabricReinforcementWire     // Wires within fabric
                    };

                    // Process each reinforcement category
                    foreach (BuiltInCategory category in reinforcementCategories)
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
                        "Reinforcement Assignment Complete!",
                        TARGET_WORKSET_NAME,
                        "Bewehrungsobjekte",
                        assignedCount,
                        errorCount,
                        processedCategories,
                        "No reinforcement elements found or all elements were already assigned to this workset.");
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
