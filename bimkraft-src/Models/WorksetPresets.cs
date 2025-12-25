using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace BIMKraft.Models
{
    /// <summary>
    /// Provides default BIMKraft workset presets
    /// </summary>
    public static class WorksetPresets
    {
        public static List<WorksetConfiguration> GetDefaultPresets()
        {
            var presets = new List<WorksetConfiguration>();

            // 1. Raster & Ebenen (Grids & Levels)
            var rastersEbenen = new WorksetConfiguration(
                "BIMKraft_ALL_Raster-Ebenen",
                "Grids and Levels");
            rastersEbenen.Rules.Add(new WorksetRule
            {
                Name = "Grids",
                RuleType = RuleType.Category,
                RuleValue = ((int)BuiltInCategory.OST_Grids).ToString()
            });
            rastersEbenen.Rules.Add(new WorksetRule
            {
                Name = "Levels",
                RuleType = RuleType.Category,
                RuleValue = ((int)BuiltInCategory.OST_Levels).ToString()
            });
            presets.Add(rastersEbenen);

            // 2. Referenzen (Links & References)
            var referenzen = new WorksetConfiguration(
                "BIMKraft_ALL_Referenzen",
                "All linked files and references");
            referenzen.Rules.Add(new WorksetRule
            {
                Name = "Revit Links",
                RuleType = RuleType.ElementClass,
                RuleValue = "RevitLinkInstance"
            });
            referenzen.Rules.Add(new WorksetRule
            {
                Name = "CAD Links",
                RuleType = RuleType.ElementClass,
                RuleValue = "ImportInstance"
            });
            referenzen.Rules.Add(new WorksetRule
            {
                Name = "Images",
                RuleType = RuleType.Category,
                RuleValue = ((int)BuiltInCategory.OST_RasterImages).ToString()
            });
            referenzen.Rules.Add(new WorksetRule
            {
                Name = "Point Clouds",
                RuleType = RuleType.ElementClass,
                RuleValue = "PointCloudInstance"
            });
            presets.Add(referenzen);

            // 3. Architektur (Architectural)
            var architektur = new WorksetConfiguration(
                "BIMKraft_ARC",
                "Architectural elements (non-structural)");
            architektur.Rules.Add(new WorksetRule
            {
                Name = "Doors",
                RuleType = RuleType.Category,
                RuleValue = ((int)BuiltInCategory.OST_Doors).ToString()
            });
            architektur.Rules.Add(new WorksetRule
            {
                Name = "Windows",
                RuleType = RuleType.Category,
                RuleValue = ((int)BuiltInCategory.OST_Windows).ToString()
            });
            architektur.Rules.Add(new WorksetRule
            {
                Name = "Non-Structural Walls",
                RuleType = RuleType.Category,
                RuleValue = ((int)BuiltInCategory.OST_Walls).ToString(),
                ParameterName = "Structural",
                ParameterValue = "0",
                ComparisonType = ComparisonType.Equals
            });
            architektur.Rules.Add(new WorksetRule
            {
                Name = "Furniture",
                RuleType = RuleType.Category,
                RuleValue = ((int)BuiltInCategory.OST_Furniture).ToString()
            });
            architektur.Rules.Add(new WorksetRule
            {
                Name = "Stairs",
                RuleType = RuleType.Category,
                RuleValue = ((int)BuiltInCategory.OST_Stairs).ToString()
            });
            architektur.Rules.Add(new WorksetRule
            {
                Name = "Railings",
                RuleType = RuleType.Category,
                RuleValue = ((int)BuiltInCategory.OST_Railings).ToString()
            });
            presets.Add(architektur);

            // 4. Bewehrung (Reinforcement)
            var bewehrung = new WorksetConfiguration(
                "BIMKraft_TWP_Bewehrung",
                "Reinforcement elements");
            bewehrung.Rules.Add(new WorksetRule
            {
                Name = "Rebar",
                RuleType = RuleType.Category,
                RuleValue = ((int)BuiltInCategory.OST_Rebar).ToString()
            });
            bewehrung.Rules.Add(new WorksetRule
            {
                Name = "Fabric Reinforcement",
                RuleType = RuleType.Category,
                RuleValue = ((int)BuiltInCategory.OST_FabricReinforcement).ToString()
            });
            bewehrung.Rules.Add(new WorksetRule
            {
                Name = "Fabric Areas",
                RuleType = RuleType.Category,
                RuleValue = ((int)BuiltInCategory.OST_FabricAreas).ToString()
            });
            presets.Add(bewehrung);

            // 5. Rohbau (Structural Shell)
            var rohbau = new WorksetConfiguration(
                "BIMKraft_TWP_Rohbau",
                "Structural shell elements");
            rohbau.Rules.Add(new WorksetRule
            {
                Name = "Structural Walls",
                RuleType = RuleType.Category,
                RuleValue = ((int)BuiltInCategory.OST_Walls).ToString(),
                ParameterName = "Structural",
                ParameterValue = "1",
                ComparisonType = ComparisonType.Equals
            });
            rohbau.Rules.Add(new WorksetRule
            {
                Name = "Structural Floors",
                RuleType = RuleType.Category,
                RuleValue = ((int)BuiltInCategory.OST_Floors).ToString(),
                ParameterName = "Structural",
                ParameterValue = "1",
                ComparisonType = ComparisonType.Equals
            });
            rohbau.Rules.Add(new WorksetRule
            {
                Name = "Foundations",
                RuleType = RuleType.Category,
                RuleValue = ((int)BuiltInCategory.OST_StructuralFoundation).ToString()
            });
            rohbau.Rules.Add(new WorksetRule
            {
                Name = "Structural Columns (Concrete)",
                RuleType = RuleType.Category,
                RuleValue = ((int)BuiltInCategory.OST_StructuralColumns).ToString()
            });
            presets.Add(rohbau);

            // 6. Stahl (Steel)
            var stahl = new WorksetConfiguration(
                "BIMKraft_TWP_Stahl",
                "Steel structural elements");
            stahl.Rules.Add(new WorksetRule
            {
                Name = "Structural Framing",
                RuleType = RuleType.Category,
                RuleValue = ((int)BuiltInCategory.OST_StructuralFraming).ToString()
            });
            stahl.Rules.Add(new WorksetRule
            {
                Name = "Trusses",
                RuleType = RuleType.Category,
                RuleValue = ((int)BuiltInCategory.OST_Truss).ToString()
            });
            stahl.Rules.Add(new WorksetRule
            {
                Name = "Structural Connections",
                RuleType = RuleType.Category,
                RuleValue = ((int)BuiltInCategory.OST_StructConnections).ToString()
            });
            presets.Add(stahl);

            return presets;
        }

        /// <summary>
        /// Gets a friendly name for a BuiltInCategory
        /// </summary>
        public static string GetCategoryDisplayName(BuiltInCategory category)
        {
            try
            {
                return LabelUtils.GetLabelFor(category);
            }
            catch
            {
                return category.ToString().Replace("OST_", "").Replace("_", " ");
            }
        }
    }
}
