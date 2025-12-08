using System.Collections.Generic;
using Newtonsoft.Json;

namespace BIMKraft.Models
{
    /// <summary>
    /// Represents a single parameter mapping (source -> target)
    /// </summary>
    public class ParameterMapping
    {
        [JsonProperty("source_parameter")]
        public string SourceParameter { get; set; }

        [JsonProperty("target_parameter")]
        public string TargetParameter { get; set; }

        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        public ParameterMapping()
        {
            Enabled = true;
        }
    }

    /// <summary>
    /// Configuration for parameter mappings for a specific category
    /// </summary>
    public class CategoryMappingConfig
    {
        [JsonProperty("category_name")]
        public string CategoryName { get; set; }

        [JsonProperty("mappings")]
        public List<ParameterMapping> Mappings { get; set; }

        public CategoryMappingConfig()
        {
            Mappings = new List<ParameterMapping>();
        }
    }

    /// <summary>
    /// Preset data containing all category mappings
    /// </summary>
    public class ParameterTransferPreset
    {
        [JsonProperty("preset_name")]
        public string PresetName { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("category_mappings")]
        public List<CategoryMappingConfig> CategoryMappings { get; set; }

        public ParameterTransferPreset()
        {
            CategoryMappings = new List<CategoryMappingConfig>();
        }
    }
}
