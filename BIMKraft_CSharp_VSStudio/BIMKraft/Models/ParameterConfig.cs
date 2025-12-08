using System.Collections.Generic;
using Newtonsoft.Json;

namespace BIMKraft.Models
{
    /// <summary>
    /// Configuration for a single parameter
    /// </summary>
    public class ParameterConfig
    {
        [JsonProperty("group")]
        public string Group { get; set; }

        [JsonProperty("categories")]
        public List<string> Categories { get; set; }

        [JsonProperty("param_group")]
        public string ParameterGroup { get; set; }

        [JsonProperty("is_instance")]
        public bool IsInstance { get; set; }

        public ParameterConfig()
        {
            Categories = new List<string>();
            ParameterGroup = "Data";
            IsInstance = true;
        }
    }
}
