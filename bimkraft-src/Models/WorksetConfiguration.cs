using System.Collections.Generic;
using System.ComponentModel;

namespace BIMKraft.Models
{
    /// <summary>
    /// Represents a complete workset configuration with name and rules
    /// </summary>
    public class WorksetConfiguration : INotifyPropertyChanged
    {
        private string _worksetName;
        private string _description;
        private bool _enabled;
        private List<WorksetRule> _rules;

        public string WorksetName
        {
            get => _worksetName;
            set
            {
                _worksetName = value;
                OnPropertyChanged(nameof(WorksetName));
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged(nameof(Description));
            }
        }

        public bool Enabled
        {
            get => _enabled;
            set
            {
                _enabled = value;
                OnPropertyChanged(nameof(Enabled));
            }
        }

        public List<WorksetRule> Rules
        {
            get => _rules;
            set
            {
                _rules = value;
                OnPropertyChanged(nameof(Rules));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public WorksetConfiguration()
        {
            Enabled = true;
            Rules = new List<WorksetRule>();
        }

        public WorksetConfiguration(string name, string description)
        {
            WorksetName = name;
            Description = description;
            Enabled = true;
            Rules = new List<WorksetRule>();
        }
    }
}
