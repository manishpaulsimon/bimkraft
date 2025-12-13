using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BIMKraft.Services;
using System;
using System.Collections.Generic;

namespace BIMKraft.Commands.WorksetTools
{
    /// <summary>
    /// Command to assign all linked references (RVT, IFC, DWG, PDF, Images, etc.) to the BIMKraft_ALL_Referenzen workset.
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class AssignReferencesCommand : IExternalCommand
    {
        private const string TARGET_WORKSET_NAME = "BIMKraft_ALL_Referenzen";

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
                using (Transaction trans = new Transaction(doc, "Assign Links to Workset"))
                {
                    trans.Start();

                    int assignedCount = 0;
                    int errorCount = 0;
                    HashSet<string> processedTypes = new HashSet<string>();

                    // Process Revit Link Instances
                    worksetService.ProcessClass<RevitLinkInstance>(
                        targetWorkset,
                        ref assignedCount,
                        ref errorCount,
                        processedTypes,
                        "Revit Link Instances");

                    // Process Revit Link Types
                    worksetService.ProcessClass<RevitLinkType>(
                        targetWorkset,
                        ref assignedCount,
                        ref errorCount,
                        processedTypes,
                        "Revit Link Types");

                    // Process CAD Link Types
                    worksetService.ProcessClass<CADLinkType>(
                        targetWorkset,
                        ref assignedCount,
                        ref errorCount,
                        processedTypes,
                        "CAD Link Types");

                    // Process CAD Import Instances
                    worksetService.ProcessClass<ImportInstance>(
                        targetWorkset,
                        ref assignedCount,
                        ref errorCount,
                        processedTypes,
                        "CAD Import Instances");

                    // Process Image Types
                    worksetService.ProcessClass<ImageType>(
                        targetWorkset,
                        ref assignedCount,
                        ref errorCount,
                        processedTypes,
                        "Image Types");

                    // Process Image Instances
                    worksetService.ProcessCategory(
                        BuiltInCategory.OST_RasterImages,
                        targetWorkset,
                        ref assignedCount,
                        ref errorCount,
                        processedTypes);

                    // Process Point Cloud Instances
                    worksetService.ProcessClass<PointCloudInstance>(
                        targetWorkset,
                        ref assignedCount,
                        ref errorCount,
                        processedTypes,
                        "Point Cloud Instances");

                    // Process Point Cloud Types
                    worksetService.ProcessClass<PointCloudType>(
                        targetWorkset,
                        ref assignedCount,
                        ref errorCount,
                        processedTypes,
                        "Point Cloud Types");

                    trans.Commit();

                    // Show results
                    WorksetService.ShowResultsDialog(
                        "Assignment Complete!",
                        TARGET_WORKSET_NAME,
                        "Referenzen",
                        assignedCount,
                        errorCount,
                        processedTypes,
                        "No linked elements found or all elements were already assigned to this workset.");
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
