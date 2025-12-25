using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace BIMKraft.Models
{
    /// <summary>
    /// Represents a rule for assigning elements to a workset
    /// </summary>
    public class WorksetRule : INotifyPropertyChanged
    {
        private bool _enabled;
        private string _name;
        private RuleType _ruleType;
        private string _ruleValue;
        private string _parameterName;
        private string _parameterValue;
        private ComparisonType _comparisonType;

        public bool Enabled
        {
            get => _enabled;
            set
            {
                _enabled = value;
                OnPropertyChanged(nameof(Enabled));
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public RuleType RuleType
        {
            get => _ruleType;
            set
            {
                _ruleType = value;
                OnPropertyChanged(nameof(RuleType));
            }
        }

        public string RuleValue
        {
            get => _ruleValue;
            set
            {
                _ruleValue = value;
                OnPropertyChanged(nameof(RuleValue));
            }
        }

        public string ParameterName
        {
            get => _parameterName;
            set
            {
                _parameterName = value;
                OnPropertyChanged(nameof(ParameterName));
            }
        }

        public string ParameterValue
        {
            get => _parameterValue;
            set
            {
                _parameterValue = value;
                OnPropertyChanged(nameof(ParameterValue));
            }
        }

        public ComparisonType ComparisonType
        {
            get => _comparisonType;
            set
            {
                _comparisonType = value;
                OnPropertyChanged(nameof(ComparisonType));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public WorksetRule()
        {
            Enabled = true;
            ComparisonType = ComparisonType.Equals;
        }
    }

    public enum RuleType
    {
        Category,
        ElementClass,
        ParameterValue,
        TypeName,
        FamilyName,
        StructuralFilter
    }

    public enum ComparisonType
    {
        Equals,
        Contains,
        StartsWith,
        EndsWith,
        NotEquals,
        GreaterThan,
        LessThan
    }

    public enum StructuralFilterType
    {
        Structural,
        NonStructural,
        All
    }
}
