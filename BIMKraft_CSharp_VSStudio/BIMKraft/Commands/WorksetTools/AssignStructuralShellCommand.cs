using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BIMKraft.Services;
using System;
using System.Collections.Generic;

namespace BIMKraft.Commands.WorksetTools
{
    /// <summary>
    /// Command to assign structural shell elements (bearing walls, structural floors, etc.) to the BIMKraft_TWP_Rohbau workset.
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class AssignStructuralShellCommand : IExternalCommand
    {
        private const string TARGET_WORKSET_NAME = "BIMKraft_TWP_Rohbau";

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
                using (Transaction trans = new Transaction(doc, "Assign Structural Shell Elements to Workset"))
                {
                    trans.Start();

                    int assignedCount = 0;
                    int errorCount = 0;
                    HashSet<string> processedCategories = new HashSet<string>();

                    // Define structural shell categories
                    // Note: Steel elements (StructuralFraming, StructuralColumns, Trusses) are
                    // intentionally excluded as they belong to BIMKraft_TWP_Stahl workset
                    List<BuiltInCategory> structuralShellCategories = new List<BuiltInCategory>
                    {
                        BuiltInCategory.OST_Walls,                      // Walls (specifically structural ones)
                        BuiltInCategory.OST_Floors,                     // Floors (specifically structural ones)
                        BuiltInCategory.OST_StructuralFraming,          // Structural framing (excluding steel)
                        BuiltInCategory.OST_StructuralColumns,          // Structural columns (excluding steel)
                        BuiltInCategory.OST_StructuralFoundation        // Foundations
                    };

                    // Process each category with appropriate filters
                    foreach (BuiltInCategory category in structuralShellCategories)
                    {
                        Func<Element, bool> filterFunc = null;

                        // Apply filters for walls, columns, and floors to include only structural elements
                        // and exclude steel and reinforcement elements
                        if (category == BuiltInCategory.OST_Walls)
                        {
                            filterFunc = (element) => WorksetService.IsStructuralWall(element) &&
                                                     !WorksetService.IsSteelElement(element) &&
                                                     !WorksetService.IsReinforcementElement(element);
                        }
                        else if (category == BuiltInCategory.OST_Columns)
                        {
                            filterFunc = (element) => WorksetService.IsStructuralColumn(element) &&
                                                     !WorksetService.IsSteelElement(element) &&
                                                     !WorksetService.IsReinforcementElement(element);
                        }
                        else if (category == BuiltInCategory.OST_Floors)
                        {
                            filterFunc = (element) => WorksetService.IsStructuralFloor(element) &&
                                                     !WorksetService.IsSteelElement(element) &&
                                                     !WorksetService.IsReinforcementElement(element);
                        }
                        else if (category == BuiltInCategory.OST_StructuralFraming ||
                                 category == BuiltInCategory.OST_StructuralColumns)
                        {
                            // Exclude steel elements from structural framing and columns
                            filterFunc = (element) => !WorksetService.IsSteelElement(element) &&
                                                     !WorksetService.IsReinforcementElement(element);
                        }
                        else
                        {
                            // For other categories, just exclude steel and reinforcement
                            filterFunc = (element) => !WorksetService.IsSteelElement(element) &&
                                                     !WorksetService.IsReinforcementElement(element);
                        }

                        worksetService.ProcessCategory(
                            category,
                            targetWorkset,
                            ref assignedCount,
                            ref errorCount,
                            processedCategories,
                            filterFunc);
                    }

                    trans.Commit();

                    // Show results
                    WorksetService.ShowResultsDialog(
                        "Structural Shell Assignment Complete!",
                        TARGET_WORKSET_NAME,
                        "Rohbauteile",
                        assignedCount,
                        errorCount,
                        processedCategories,
                        "No structural shell elements found or all elements were already assigned to this workset.",
                        "Note: Steel elements (Stahl) and reinforcement (Bewehrung) are excluded from this workset.");
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
