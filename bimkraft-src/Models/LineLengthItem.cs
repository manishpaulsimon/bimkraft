using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media;
using Autodesk.Revit.DB;

namespace BIMKraft.Models
{
    /// <summary>
    /// Represents a group of connected lines with total length and color information
    /// </summary>
    public class LineLengthItem : INotifyPropertyChanged
    {
        private bool _isSelected;
        private string _colorHex;
        private SolidColorBrush _colorBrush;
        private string _description;
        private string _groupStatus;

        /// <summary>
        /// Unique identifier for this line group
        /// </summary>
        public int GroupId { get; set; }

        /// <summary>
        /// Status of Revit group creation
        /// </summary>
        public string GroupStatus
        {
            get => _groupStatus;
            set
            {
                if (_groupStatus != value)
                {
                    _groupStatus = value;
                    OnPropertyChanged(nameof(GroupStatus));
                }
            }
        }

        /// <summary>
        /// User-defined description/name for this line group
        /// </summary>
        public string Description
        {
            get => _description;
            set
            {
                if (_description != value)
                {
                    _description = value;
                    OnPropertyChanged(nameof(Description));
                }
            }
        }

        /// <summary>
        /// Number of connected lines in this group
        /// </summary>
        public int LineCount { get; set; }

        /// <summary>
        /// Total length of all connected lines in the group (in project units)
        /// </summary>
        public double TotalLength { get; set; }

        /// <summary>
        /// Formatted total length string with units
        /// </summary>
        public string TotalLengthFormatted { get; set; }

        /// <summary>
        /// Graphics style name (line style) for this group
        /// </summary>
        public string LineStyle { get; set; }

        /// <summary>
        /// Line type (Detail Line, Model Line, etc.)
        /// </summary>
        public string LineType { get; set; }

        /// <summary>
        /// View name where these lines exist (for Detail Lines)
        /// </summary>
        public string ViewName { get; set; }

        /// <summary>
        /// List of element IDs in this group
        /// </summary>
        public List<ElementId> ElementIds { get; set; }

        /// <summary>
        /// Comma-separated string of element IDs
        /// </summary>
        public string ElementIdsString { get; set; }

        /// <summary>
        /// Color assigned to this line group (hex format)
        /// </summary>
        public string ColorHex
        {
            get => _colorHex;
            set
            {
                if (_colorHex != value)
                {
                    _colorHex = value;
                    UpdateColorBrush();
                    OnPropertyChanged(nameof(ColorHex));
                }
            }
        }

        /// <summary>
        /// Color brush for UI display
        /// </summary>
        public SolidColorBrush ColorBrush
        {
            get => _colorBrush;
            private set
            {
                if (_colorBrush != value)
                {
                    _colorBrush = value;
                    OnPropertyChanged(nameof(ColorBrush));
                }
            }
        }

        /// <summary>
        /// Whether this item is selected in the UI
        /// </summary>
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

        public LineLengthItem()
        {
            ElementIds = new List<ElementId>();
            ColorHex = "#808080"; // Default gray
            LineStyle = "Unknown";
            LineType = "Unknown";
            ViewName = "N/A";
            Description = "";
            GroupStatus = "Not Grouped";
        }

        private void UpdateColorBrush()
        {
            try
            {
                var color = (System.Windows.Media.Color)ColorConverter.ConvertFromString(_colorHex);
                ColorBrush = new SolidColorBrush(color);
            }
            catch
            {
                ColorBrush = new SolidColorBrush(Colors.Gray);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// Statistics for line length calculation
    /// </summary>
    public class LineLengthStatistics
    {
        public int TotalLineGroups { get; set; }
        public int TotalLines { get; set; }
        public double TotalLength { get; set; }
        public string TotalLengthFormatted { get; set; }
        public int DetailLineCount { get; set; }
        public int ModelLineCount { get; set; }
        public Dictionary<string, int> LinesByStyle { get; set; }

        public LineLengthStatistics()
        {
            LinesByStyle = new Dictionary<string, int>();
        }
    }
}
