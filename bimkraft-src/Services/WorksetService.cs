using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BIMKraft.Services
{
    /// <summary>
    /// Service class for workset-related operations.
    /// Provides common functionality for finding, creating, and assigning elements to worksets.
    /// </summary>
    public class WorksetService
    {
        private readonly Document _doc;

        public WorksetService(Document doc)
        {
            _doc = doc ?? throw new ArgumentNullException(nameof(doc));
        }

        /// <summary>
        /// Finds an existing workset by name, or creates it if it doesn't exist.
        /// </summary>
        /// <param name="worksetName">Name of the workset to find or create</param>
        /// <returns>The workset, or null if creation failed</returns>
        public Workset FindOrCreateWorkset(string worksetName)
        {
            // Get all worksets in the document
            FilteredWorksetCollector worksets = new FilteredWorksetCollector(_doc)
                .OfKind(WorksetKind.UserWorkset);

            // Try to find existing workset
            foreach (Workset workset in worksets)
            {
                if (workset.Name == worksetName)
                {
                    return workset;
                }
            }

            // Workset doesn't exist, create it
            using (Transaction t = new Transaction(_doc, "Create Workset"))
            {
                t.Start();
                try
                {
                    Workset newWorkset = Workset.Create(_doc, worksetName);
                    t.Commit();
                    return newWorkset;
                }
                catch (Exception ex)
                {
                    t.RollBack();
                    TaskDialog.Show("Error", $"Failed to create workset '{worksetName}': {ex.Message}");
                    return null;
                }
            }
        }

        /// <summary>
        /// Changes an element's workset using the parameter method.
        /// </summary>
        /// <param name="element">Element to reassign</param>
        /// <param name="targetWorksetId">Target workset ID</param>
        /// <returns>True if successful, false otherwise</returns>
        public bool ChangeElementWorkset(Element element, WorksetId targetWorksetId)
        {
            try
            {
                Parameter worksetParam = element.get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM);
                if (worksetParam != null && !worksetParam.IsReadOnly)
                {
                    worksetParam.Set(targetWorksetId.IntegerValue);
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Processes elements of a specific category and assigns them to the target workset.
        /// </summary>
        /// <param name="category">Category to process</param>
        /// <param name="targetWorkset">Target workset</param>
        /// <param name="assignedCount">Counter for successfully assigned elements</param>
        /// <param name="errorCount">Counter for failed assignments</param>
        /// <param name="processedCategories">Set of category names that had elements processed</param>
        /// <param name="filterFunc">Optional filter function for elements (return true to include)</param>
        public void ProcessCategory(
            BuiltInCategory category,
            Workset targetWorkset,
            ref int assignedCount,
            ref int errorCount,
            HashSet<string> processedCategories,
            Func<Element, bool> filterFunc = null)
        {
            try
            {
                // Get all elements in this category
                FilteredElementCollector collector = new FilteredElementCollector(_doc)
                    .OfCategory(category)
                    .WhereElementIsNotElementType();

                string categoryName = GetCategoryName(category);
                int categoryCount = 0;

                foreach (Element element in collector)
                {
                    try
                    {
                        // Skip if element is already in the target workset
                        if (element.WorksetId == targetWorkset.Id)
                        {
                            continue;
                        }

                        // Apply filter if provided
                        if (filterFunc != null && !filterFunc(element))
                        {
                            continue;
                        }

                        // Assign element to workset
                        if (ChangeElementWorkset(element, targetWorkset.Id))
                        {
                            assignedCount++;
                            categoryCount++;
                        }
                        else
                        {
                            errorCount++;
                        }
                    }
                    catch (Exception)
                    {
                        errorCount++;
                    }
                }

                if (categoryCount > 0)
                {
                    processedCategories.Add(categoryName);
                }

                // Process element types
                ProcessElementTypes(category, targetWorkset, ref assignedCount, ref errorCount);
            }
            catch (Exception)
            {
                // Category processing error, continue with next
            }
        }

        /// <summary>
        /// Processes element types for a specific category.
        /// </summary>
        private void ProcessElementTypes(
            BuiltInCategory category,
            Workset targetWorkset,
            ref int assignedCount,
            ref int errorCount)
        {
            try
            {
                FilteredElementCollector typeCollector = new FilteredElementCollector(_doc)
                    .OfCategory(category)
                    .WhereElementIsElementType();

                foreach (Element elementType in typeCollector)
                {
                    try
                    {
                        if (elementType.WorksetId == targetWorkset.Id)
                        {
                            continue;
                        }

                        if (ChangeElementWorkset(elementType, targetWorkset.Id))
                        {
                            assignedCount++;
                        }
                        else
                        {
                            errorCount++;
                        }
                    }
                    catch (Exception)
                    {
                        errorCount++;
                    }
                }
            }
            catch (Exception)
            {
                // Type processing error, continue
            }
        }

        /// <summary>
        /// Processes elements of a specific class type and assigns them to the target workset.
        /// </summary>
        /// <typeparam name="T">Element class type</typeparam>
        /// <param name="targetWorkset">Target workset</param>
        /// <param name="assignedCount">Counter for successfully assigned elements</param>
        /// <param name="errorCount">Counter for failed assignments</param>
        /// <param name="processedTypes">Set of type names that had elements processed</param>
        /// <param name="typeName">Display name for this type</param>
        public void ProcessClass<T>(
            Workset targetWorkset,
            ref int assignedCount,
            ref int errorCount,
            HashSet<string> processedTypes,
            string typeName) where T : Element
        {
            try
            {
                FilteredElementCollector collector = new FilteredElementCollector(_doc)
                    .OfClass(typeof(T));

                int typeCount = 0;

                foreach (Element element in collector)
                {
                    try
                    {
                        if (element.WorksetId == targetWorkset.Id)
                        {
                            continue;
                        }

                        if (ChangeElementWorkset(element, targetWorkset.Id))
                        {
                            assignedCount++;
                            typeCount++;
                        }
                        else
                        {
                            errorCount++;
                        }
                    }
                    catch (Exception)
                    {
                        errorCount++;
                    }
                }

                if (typeCount > 0)
                {
                    processedTypes.Add(typeName);
                }
            }
            catch (Exception)
            {
                // Class processing error, continue
            }
        }

        /// <summary>
        /// Gets a user-friendly category name.
        /// </summary>
        private string GetCategoryName(BuiltInCategory category)
        {
            try
            {
                return LabelUtils.GetLabelFor(category);
            }
            catch
            {
                return category.ToString();
            }
        }

        /// <summary>
        /// Checks if a wall is structural.
        /// </summary>
        public static bool IsStructuralWall(Element element)
        {
            try
            {
                Parameter param = element.get_Parameter(BuiltInParameter.WALL_STRUCTURAL_SIGNIFICANT);
                return param != null && param.AsInteger() == 1;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if a column is structural by checking if it's in the Structural Columns category.
        /// </summary>
        public static bool IsStructuralColumn(Element element)
        {
            try
            {
                // In Revit API, structural columns are typically in OST_StructuralColumns category
                // If it's in OST_Columns and we need to check structural, try the parameter
                Category category = element.Category;
#if REVIT2025
                if (category != null && category.Id.Value == (long)BuiltInCategory.OST_StructuralColumns)
#else
                if (category != null && category.Id.IntegerValue == (int)BuiltInCategory.OST_StructuralColumns)
#endif
                {
                    return true;
                }

                // Try to find structural parameter (different names in different versions)
                Parameter param = element.LookupParameter("Structural");
                if (param != null && param.AsInteger() == 1)
                {
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if a floor is structural.
        /// </summary>
        public static bool IsStructuralFloor(Element element)
        {
            try
            {
                // Try the built-in parameter
                Parameter param = element.get_Parameter(BuiltInParameter.FLOOR_PARAM_IS_STRUCTURAL);
                if (param != null && param.AsInteger() == 1)
                {
                    return true;
                }

                // Try by parameter name as fallback
                param = element.LookupParameter("Structural");
                return param != null && param.AsInteger() == 1;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if an element is a steel element based on category.
        /// Steel elements include: Structural Framing, Structural Columns, Trusses, and certain Generic Models.
        /// </summary>
        public static bool IsSteelElement(Element element)
        {
            try
            {
                Category category = element.Category;
                if (category == null)
                {
                    return false;
                }

#if REVIT2025
                long categoryId = category.Id.Value;
#else
                int categoryId = category.Id.IntegerValue;
#endif

                // Check if element is in steel-related categories
                return categoryId == (long)BuiltInCategory.OST_StructuralFraming ||
                       categoryId == (long)BuiltInCategory.OST_StructuralColumns ||
                       categoryId == (long)BuiltInCategory.OST_Truss;
                // Note: OST_GenericModel is intentionally not included here to avoid false positives
                // Generic models should be evaluated case-by-case based on material or family name
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if an element is a reinforcement element based on category.
        /// Reinforcement elements include: Rebar, Fabric Reinforcement, Fabric Areas, and Fabric Reinforcement Wire.
        /// </summary>
        public static bool IsReinforcementElement(Element element)
        {
            try
            {
                Category category = element.Category;
                if (category == null)
                {
                    return false;
                }

#if REVIT2025
                long categoryId = category.Id.Value;
#else
                int categoryId = category.Id.IntegerValue;
#endif

                // Check if element is in reinforcement-related categories
                return categoryId == (long)BuiltInCategory.OST_Rebar ||
                       categoryId == (long)BuiltInCategory.OST_FabricReinforcement ||
                       categoryId == (long)BuiltInCategory.OST_FabricAreas ||
                       categoryId == (long)BuiltInCategory.OST_FabricReinforcementWire;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Builds and displays a results dialog.
        /// </summary>
        /// <param name="title">Dialog title</param>
        /// <param name="worksetName">Name of the workset</param>
        /// <param name="worksetDescription">Description of workset purpose</param>
        /// <param name="assignedCount">Number of elements assigned</param>
        /// <param name="errorCount">Number of errors encountered</param>
        /// <param name="processedItems">Collection of processed item names</param>
        /// <param name="noElementsMessage">Message to show when no elements found</param>
        /// <param name="additionalNotes">Optional additional notes</param>
        public static void ShowResultsDialog(
            string title,
            string worksetName,
            string worksetDescription,
            int assignedCount,
            int errorCount,
            IEnumerable<string> processedItems,
            string noElementsMessage = null,
            string additionalNotes = null)
        {
            StringBuilder messageBuilder = new StringBuilder();
            messageBuilder.AppendLine($"{title}\n");
            messageBuilder.AppendLine($"Workset: {worksetName} – {worksetDescription}");
            messageBuilder.AppendLine($"Elements assigned: {assignedCount}");

            if (errorCount > 0)
            {
                messageBuilder.AppendLine($"Errors encountered: {errorCount}");
            }

            if (processedItems != null && processedItems.Any())
            {
                messageBuilder.AppendLine("\nProcessed categories/types:");
                foreach (string item in processedItems.OrderBy(i => i))
                {
                    messageBuilder.AppendLine($"- {item}");
                }
            }

            if (assignedCount == 0 && errorCount == 0)
            {
                messageBuilder.AppendLine("\n" + (noElementsMessage ?? "No elements found or all elements were already assigned to this workset."));
            }
            else if (errorCount > 0)
            {
                messageBuilder.AppendLine("\nNote: Some elements could not be moved due to API limitations or element constraints.");
            }

            if (!string.IsNullOrEmpty(additionalNotes))
            {
                messageBuilder.AppendLine("\n" + additionalNotes);
            }

            TaskDialog.Show("Success", messageBuilder.ToString());
        }
    }
}
