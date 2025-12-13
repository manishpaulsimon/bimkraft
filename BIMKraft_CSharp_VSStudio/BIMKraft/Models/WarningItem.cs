using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Autodesk.Revit.DB;

namespace BIMKraft.Models
{
    /// <summary>
    /// Represents a warning item with associated elements and metadata
    /// </summary>
    public class WarningItem : INotifyPropertyChanged
    {
        private bool _isExpanded;
        private bool _isSelected;

        public string Message { get; set; }
        public int ElementCount { get; set; }
        public int OccurrenceCount { get; set; }
        public string ElementIds { get; set; }
        public string ElementNames { get; set; }
        public string Levels { get; set; }
        public string Views { get; set; }
        public string Categories { get; set; }
        public List<ElementId> ElementIdList { get; set; }
        public bool IsGroup { get; set; }
        public List<WarningItem> Children { get; set; }
        public WarningItem Parent { get; set; }
        public WarningSeverity Severity { get; set; }

        public string ExpandSymbol => IsGroup && OccurrenceCount > 1 ? (IsExpanded ? "[-]" : "[+]") : "";

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    OnPropertyChanged(nameof(IsExpanded));
                    OnPropertyChanged(nameof(ExpandSymbol));
                }
            }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        public WarningItem()
        {
            Children = new List<WarningItem>();
            ElementIdList = new List<ElementId>();
            Severity = WarningSeverity.Warning;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// Warning severity levels
    /// </summary>
    public enum WarningSeverity
    {
        Info,
        Warning,
        Error
    }

    /// <summary>
    /// Element information details
    /// </summary>
    public class ElementInfo
    {
        public string Name { get; set; }
        public string Level { get; set; }
        public string Category { get; set; }
        public List<string> Views { get; set; }

        public ElementInfo()
        {
            Views = new List<string>();
            Name = "Unknown";
            Level = "N/A";
            Category = "N/A";
        }
    }

    /// <summary>
    /// Warning statistics for dashboard
    /// </summary>
    public class WarningStatistics
    {
        public int TotalWarnings { get; set; }
        public int UniqueWarningTypes { get; set; }
        public int TotalElements { get; set; }
        public Dictionary<string, int> WarningsByCategory { get; set; }
        public Dictionary<string, int> WarningsByLevel { get; set; }

        public WarningStatistics()
        {
            WarningsByCategory = new Dictionary<string, int>();
            WarningsByLevel = new Dictionary<string, int>();
        }
    }
}
