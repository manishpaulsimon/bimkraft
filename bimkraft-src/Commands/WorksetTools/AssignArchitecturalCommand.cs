using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BIMKraft.Services;
using System;
using System.Collections.Generic;

namespace BIMKraft.Commands.WorksetTools
{
    /// <summary>
    /// Command to assign architectural elements (doors, windows, non-structural walls, etc.) to the BIMKraft_ARC workset.
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class AssignArchitecturalCommand : IExternalCommand
    {
        private const string TARGET_WORKSET_NAME = "BIMKraft_ARC";

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
                using (Transaction trans = new Transaction(doc, "Assign Architectural Elements to Workset"))
                {
                    trans.Start();

                    int assignedCount = 0;
                    int errorCount = 0;
                    HashSet<string> processedCategories = new HashSet<string>();

                    // Define architectural categories
                    List<BuiltInCategory> architecturalCategories = new List<BuiltInCategory>
                    {
                        // Doors and Windows
                        BuiltInCategory.OST_Doors,
                        BuiltInCategory.OST_Windows,

                        // Floors (non-structural)
                        BuiltInCategory.OST_Floors,

                        // Ceilings
                        BuiltInCategory.OST_Ceilings,

                        // Walls (non-structural)
                        BuiltInCategory.OST_Walls,

                        // Roofs
                        BuiltInCategory.OST_Roofs,
                        BuiltInCategory.OST_Fascia,

                        // Stairs and Railings
                        BuiltInCategory.OST_Stairs,
                        BuiltInCategory.OST_StairsRailing,
                        BuiltInCategory.OST_Railings,

                        // Curtain Systems
                        BuiltInCategory.OST_CurtainWallPanels,
                        BuiltInCategory.OST_CurtainWallMullions,
                        BuiltInCategory.OST_CurtaSystem,

                        // Specialty Equipment
                        BuiltInCategory.OST_SpecialityEquipment,
                        BuiltInCategory.OST_Furniture,
                        BuiltInCategory.OST_FurnitureSystems,
                        BuiltInCategory.OST_Casework,

                        // Plumbing Fixtures
                        BuiltInCategory.OST_PlumbingFixtures,

                        // Pipes and Conduits (Entwässerungsleitung, Leerrohren)
                        BuiltInCategory.OST_PipeCurves,
                        BuiltInCategory.OST_PipeFitting,
                        BuiltInCategory.OST_Conduit,
                        BuiltInCategory.OST_ConduitFitting,

                        // Site Elements
                        BuiltInCategory.OST_Site,
                        BuiltInCategory.OST_Topography,
                        BuiltInCategory.OST_Planting,
                        BuiltInCategory.OST_Entourage,

                        // Rooms
                        BuiltInCategory.OST_Rooms,

                        // Mass elements
                        BuiltInCategory.OST_Mass,

                        // Columns (non-structural)
                        BuiltInCategory.OST_Columns,

                        // Lighting Fixtures
                        BuiltInCategory.OST_LightingFixtures,

                        // Parking
                        BuiltInCategory.OST_Parking
                    };

                    // Process each category with appropriate filters
                    foreach (BuiltInCategory category in architecturalCategories)
                    {
                        Func<Element, bool> filterFunc = null;

                        // Apply filters for walls, columns, and floors to exclude structural elements
                        if (category == BuiltInCategory.OST_Walls)
                        {
                            filterFunc = (element) => !WorksetService.IsStructuralWall(element);
                        }
                        else if (category == BuiltInCategory.OST_Columns)
                        {
                            filterFunc = (element) => !WorksetService.IsStructuralColumn(element);
                        }
                        else if (category == BuiltInCategory.OST_Floors)
                        {
                            filterFunc = (element) => !WorksetService.IsStructuralFloor(element);
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
                        "Architectural Assignment Complete!",
                        TARGET_WORKSET_NAME,
                        "Architekturobjekte",
                        assignedCount,
                        errorCount,
                        processedCategories,
                        "No architectural elements found or all elements were already assigned to this workset.",
                        "Structural walls, floors, and columns were intentionally skipped.");
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
