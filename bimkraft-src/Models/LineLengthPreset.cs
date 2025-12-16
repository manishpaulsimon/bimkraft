using System;
using System.Collections.Generic;

namespace BIMKraft.Models
{
    /// <summary>
    /// Represents a saved preset for line groups with their descriptions and colors
    /// </summary>
    public class LineLengthPreset
    {
        /// <summary>
        /// Name of the preset
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Date when the preset was created
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// List of saved line groups
        /// </summary>
        public List<PresetLineGroup> LineGroups { get; set; }

        /// <summary>
        /// List of selected element IDs (for restoring selection)
        /// </summary>
        public List<int> SelectedElementIds { get; set; }

        public LineLengthPreset()
        {
            LineGroups = new List<PresetLineGroup>();
            SelectedElementIds = new List<int>();
            CreatedDate = DateTime.Now;
        }
    }

    /// <summary>
    /// Represents a line group in a preset
    /// </summary>
    public class PresetLineGroup
    {
        /// <summary>
        /// Element IDs in this group
        /// </summary>
        public List<int> ElementIds { get; set; }

        /// <summary>
        /// Description/name for this group
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Color in hex format
        /// </summary>
        public string ColorHex { get; set; }

        public PresetLineGroup()
        {
            ElementIds = new List<int>();
            Description = "";
            ColorHex = "#808080";
        }
    }
}
