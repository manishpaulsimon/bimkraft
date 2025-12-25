using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BIMKraft.Models;
using BIMKraft.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

// Aliases to resolve ambiguities between WPF and Revit API
using WpfTextBox = System.Windows.Controls.TextBox;
using WpfComboBox = System.Windows.Controls.ComboBox;
using WpfGrid = System.Windows.Controls.Grid;
using WorksetConfig = BIMKraft.Models.WorksetConfiguration;
using WpfVisibility = System.Windows.Visibility;
using WpfThickness = System.Windows.Thickness;

namespace BIMKraft.Commands.WorksetTools
{
    public partial class WorksetManagerWindow : Window
    {
        private readonly UIDocument _uidoc;
        private readonly Document _doc;
        private readonly WorksetService _worksetService;
        private ObservableCollection<WorksetConfig> _worksetConfigurations;
        private ObservableCollection<WorksetRule> _currentRules;
        private readonly ExternalEvent _externalEvent;
        private readonly WorksetApplyHandler _applyHandler;

        public WorksetManagerWindow(UIDocument uidoc)
        {
            InitializeComponent();
            _uidoc = uidoc;
            _doc = uidoc.Document;
            _worksetService = new WorksetService(_doc);

            // Initialize collections
            _worksetConfigurations = new ObservableCollection<WorksetConfig>();
            _currentRules = new ObservableCollection<WorksetRule>();

            // Set data context and item sources
            WorksetConfigurationsGrid.ItemsSource = _worksetConfigurations;
            RulesGrid.ItemsSource = _currentRules;

            // Load presets into combo box
            LoadPresetsIntoComboBox();

            // Create ExternalEvent for applying worksets
            _applyHandler = new WorksetApplyHandler();
            _externalEvent = ExternalEvent.Create(_applyHandler);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _externalEvent?.Dispose();
        }

        private void LoadPresetsIntoComboBox()
        {
            var presets = WorksetPresets.GetDefaultPresets();
            PresetComboBox.ItemsSource = presets;
            if (presets.Count > 0)
            {
                PresetComboBox.SelectedIndex = 0;
            }
        }

        private void LoadPreset_Click(object sender, RoutedEventArgs e)
        {
            if (PresetComboBox.SelectedItem is WorksetConfig preset)
            {
                // Create a deep copy of the preset
                var newConfig = new WorksetConfig(preset.WorksetName, preset.Description);
                foreach (var rule in preset.Rules)
                {
                    newConfig.Rules.Add(new WorksetRule
                    {
                        Enabled = rule.Enabled,
                        Name = rule.Name,
                        RuleType = rule.RuleType,
                        RuleValue = rule.RuleValue,
                        ParameterName = rule.ParameterName,
                        ParameterValue = rule.ParameterValue,
                        ComparisonType = rule.ComparisonType
                    });
                }

                _worksetConfigurations.Add(newConfig);
                StatusTextBlock.Text = $"Loaded preset: {preset.WorksetName}";
            }
        }

        private void LoadAllPresets_Click(object sender, RoutedEventArgs e)
        {
            _worksetConfigurations.Clear();
            var presets = WorksetPresets.GetDefaultPresets();

            foreach (var preset in presets)
            {
                // Create deep copy
                var newConfig = new WorksetConfig(preset.WorksetName, preset.Description);
                foreach (var rule in preset.Rules)
                {
                    newConfig.Rules.Add(new WorksetRule
                    {
                        Enabled = rule.Enabled,
                        Name = rule.Name,
                        RuleType = rule.RuleType,
                        RuleValue = rule.RuleValue,
                        ParameterName = rule.ParameterName,
                        ParameterValue = rule.ParameterValue,
                        ComparisonType = rule.ComparisonType
                    });
                }
                _worksetConfigurations.Add(newConfig);
            }

            StatusTextBlock.Text = $"Loaded all {presets.Count} preset configurations";
        }

        private void WorksetConfigurationsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WorksetConfigurationsGrid.SelectedItem is WorksetConfig config)
            {
                // Update detail panel
                WorksetNameTextBox.Text = config.WorksetName;
                DescriptionTextBox.Text = config.Description;
                EnabledCheckBox.IsChecked = config.Enabled;

                // Update rules grid
                _currentRules.Clear();
                foreach (var rule in config.Rules)
                {
                    _currentRules.Add(rule);
                }

                StatusTextBlock.Text = $"Selected: {config.WorksetName} ({config.Rules.Count} rules)";
            }
            else
            {
                // Clear detail panel
                WorksetNameTextBox.Text = string.Empty;
                DescriptionTextBox.Text = string.Empty;
                EnabledCheckBox.IsChecked = true;
                _currentRules.Clear();
                StatusTextBlock.Text = "Ready";
            }
        }

        private void AddWorkset_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new WorksetEditDialog();
            if (dialog.ShowDialog() == true)
            {
                var newConfig = new WorksetConfig(dialog.WorksetName, dialog.WorksetDescription);
                _worksetConfigurations.Add(newConfig);
                WorksetConfigurationsGrid.SelectedItem = newConfig;
                StatusTextBlock.Text = $"Added new workset: {dialog.WorksetName}";
            }
        }

        private void EditWorkset_Click(object sender, RoutedEventArgs e)
        {
            if (WorksetConfigurationsGrid.SelectedItem is WorksetConfig config)
            {
                var dialog = new WorksetEditDialog(config.WorksetName, config.Description);
                if (dialog.ShowDialog() == true)
                {
                    config.WorksetName = dialog.WorksetName;
                    config.Description = dialog.WorksetDescription;
                    WorksetNameTextBox.Text = config.WorksetName;
                    DescriptionTextBox.Text = config.Description;
                    WorksetConfigurationsGrid.Items.Refresh();
                    StatusTextBlock.Text = $"Updated workset: {dialog.WorksetName}";
                }
            }
        }

        private void DeleteWorkset_Click(object sender, RoutedEventArgs e)
        {
            if (WorksetConfigurationsGrid.SelectedItem is WorksetConfig config)
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to delete the workset configuration '{config.WorksetName}'?",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _worksetConfigurations.Remove(config);
                    StatusTextBlock.Text = $"Deleted workset: {config.WorksetName}";
                }
            }
        }

        private void AddRule_Click(object sender, RoutedEventArgs e)
        {
            if (WorksetConfigurationsGrid.SelectedItem is WorksetConfig config)
            {
                var dialog = new RuleEditDialog(_doc);
                if (dialog.ShowDialog() == true)
                {
                    var newRule = dialog.GetRule();
                    config.Rules.Add(newRule);
                    _currentRules.Add(newRule);
                    WorksetConfigurationsGrid.Items.Refresh();
                    StatusTextBlock.Text = $"Added rule: {newRule.Name}";
                }
            }
            else
            {
                MessageBox.Show("Please select a workset configuration first.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void EditRule_Click(object sender, RoutedEventArgs e)
        {
            if (RulesGrid.SelectedItem is WorksetRule rule)
            {
                var dialog = new RuleEditDialog(_doc, rule);
                if (dialog.ShowDialog() == true)
                {
                    var updatedRule = dialog.GetRule();
                    rule.Enabled = updatedRule.Enabled;
                    rule.Name = updatedRule.Name;
                    rule.RuleType = updatedRule.RuleType;
                    rule.RuleValue = updatedRule.RuleValue;
                    rule.ParameterName = updatedRule.ParameterName;
                    rule.ParameterValue = updatedRule.ParameterValue;
                    rule.ComparisonType = updatedRule.ComparisonType;
                    RulesGrid.Items.Refresh();
                    StatusTextBlock.Text = $"Updated rule: {rule.Name}";
                }
            }
        }

        private void DeleteRule_Click(object sender, RoutedEventArgs e)
        {
            if (RulesGrid.SelectedItem is WorksetRule rule &&
                WorksetConfigurationsGrid.SelectedItem is WorksetConfig config)
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to delete the rule '{rule.Name}'?",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    config.Rules.Remove(rule);
                    _currentRules.Remove(rule);
                    WorksetConfigurationsGrid.Items.Refresh();
                    StatusTextBlock.Text = $"Deleted rule: {rule.Name}";
                }
            }
        }

        private void SaveChanges_Click(object sender, RoutedEventArgs e)
        {
            if (WorksetConfigurationsGrid.SelectedItem is WorksetConfig config)
            {
                config.WorksetName = WorksetNameTextBox.Text;
                config.Description = DescriptionTextBox.Text;
                config.Enabled = EnabledCheckBox.IsChecked ?? true;
                WorksetConfigurationsGrid.Items.Refresh();
                StatusTextBlock.Text = $"Saved changes to: {config.WorksetName}";
            }
        }

        private void Preview_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var previewResults = new List<string>();

                foreach (var config in _worksetConfigurations.Where(c => c.Enabled))
                {
                    var matchedElements = GetMatchingElements(config);
                    previewResults.Add($"{config.WorksetName}: {matchedElements.Count} elements will be assigned");
                }

                var previewMessage = string.Join("\n", previewResults);
                if (string.IsNullOrEmpty(previewMessage))
                {
                    previewMessage = "No enabled workset configurations found.";
                }

                TaskDialog td = new TaskDialog("Preview Changes");
                td.MainInstruction = "Workset Assignment Preview";
                td.MainContent = previewMessage;
                td.Show();

                StatusTextBlock.Text = "Preview completed";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during preview:\n{ex.Message}", "Preview Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_worksetConfigurations.Count == 0)
                {
                    MessageBox.Show("No workset configurations to apply. Please add or load configurations first.",
                        "No Configurations", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var enabledConfigs = _worksetConfigurations.Where(c => c.Enabled).ToList();
                if (enabledConfigs.Count == 0)
                {
                    MessageBox.Show("No enabled workset configurations. Please enable at least one configuration.",
                        "No Enabled Configurations", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var result = MessageBox.Show(
                    $"This will assign elements to {enabledConfigs.Count} workset(s) based on the configured rules.\n\n" +
                    "Do you want to continue?",
                    "Confirm Apply",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes)
                {
                    return;
                }

                // Prepare data for external event
                _applyHandler.Document = _doc;
                _applyHandler.WorksetService = _worksetService;
                _applyHandler.Configurations = enabledConfigs;
                _applyHandler.StatusCallback = (message) => Dispatcher.Invoke(() => StatusTextBlock.Text = message);
                _applyHandler.GetMatchingElementsFunc = GetMatchingElements;

                // Raise external event to execute on Revit's main thread
                _externalEvent.Raise();

                StatusTextBlock.Text = "Applying worksets...";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error applying worksets:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private List<Element> GetMatchingElements(WorksetConfig config)
        {
            var matchedElements = new HashSet<Element>();

            foreach (var rule in config.Rules.Where(r => r.Enabled))
            {
                var ruleMatches = EvaluateRule(rule);
                foreach (var element in ruleMatches)
                {
                    matchedElements.Add(element);
                }
            }

            return matchedElements.ToList();
        }

        private List<Element> EvaluateRule(WorksetRule rule)
        {
            var results = new List<Element>();

            switch (rule.RuleType)
            {
                case RuleType.Category:
                    results = EvaluateCategoryRule(rule);
                    break;

                case RuleType.ElementClass:
                    results = EvaluateElementClassRule(rule);
                    break;

                case RuleType.ParameterValue:
                    results = EvaluateParameterValueRule(rule);
                    break;

                case RuleType.TypeName:
                    results = EvaluateTypeNameRule(rule);
                    break;

                case RuleType.FamilyName:
                    results = EvaluateFamilyNameRule(rule);
                    break;

                case RuleType.StructuralFilter:
                    results = EvaluateStructuralFilterRule(rule);
                    break;
            }

            return results;
        }

        private List<Element> EvaluateCategoryRule(WorksetRule rule)
        {
            var results = new List<Element>();

            if (int.TryParse(rule.RuleValue, out int categoryId))
            {
                var category = (BuiltInCategory)categoryId;
                var collector = new FilteredElementCollector(_doc)
                    .OfCategory(category)
                    .WhereElementIsNotElementType();

                var elements = collector.ToList();

                // Apply additional parameter filter if specified
                if (!string.IsNullOrEmpty(rule.ParameterName))
                {
                    elements = elements.Where(e => EvaluateParameterCondition(e, rule)).ToList();
                }

                results.AddRange(elements);
            }

            return results;
        }

        private List<Element> EvaluateElementClassRule(WorksetRule rule)
        {
            var results = new List<Element>();
            var className = rule.RuleValue;

            var collector = new FilteredElementCollector(_doc)
                .WhereElementIsNotElementType();

            foreach (var element in collector)
            {
                if (element.GetType().Name == className || element.GetType().BaseType?.Name == className)
                {
                    results.Add(element);
                }
            }

            return results;
        }

        private List<Element> EvaluateParameterValueRule(WorksetRule rule)
        {
            var results = new List<Element>();

            var collector = new FilteredElementCollector(_doc)
                .WhereElementIsNotElementType();

            foreach (var element in collector)
            {
                if (EvaluateParameterCondition(element, rule))
                {
                    results.Add(element);
                }
            }

            return results;
        }

        private List<Element> EvaluateTypeNameRule(WorksetRule rule)
        {
            var results = new List<Element>();

            var collector = new FilteredElementCollector(_doc)
                .WhereElementIsNotElementType();

            foreach (var element in collector)
            {
                var elementType = _doc.GetElement(element.GetTypeId()) as ElementType;
                if (elementType != null)
                {
                    if (CompareStrings(elementType.Name, rule.RuleValue, rule.ComparisonType))
                    {
                        results.Add(element);
                    }
                }
            }

            return results;
        }

        private List<Element> EvaluateFamilyNameRule(WorksetRule rule)
        {
            var results = new List<Element>();

            var collector = new FilteredElementCollector(_doc)
                .WhereElementIsNotElementType();

            foreach (var element in collector)
            {
                if (element is FamilyInstance familyInstance)
                {
                    var familyName = familyInstance.Symbol?.FamilyName;
                    if (familyName != null && CompareStrings(familyName, rule.RuleValue, rule.ComparisonType))
                    {
                        results.Add(element);
                    }
                }
            }

            return results;
        }

        private List<Element> EvaluateStructuralFilterRule(WorksetRule rule)
        {
            var results = new List<Element>();

            var collector = new FilteredElementCollector(_doc)
                .WhereElementIsNotElementType();

            foreach (var element in collector)
            {
                bool isStructural = IsStructuralElement(element);

                if ((rule.RuleValue == "Structural" && isStructural) ||
                    (rule.RuleValue == "NonStructural" && !isStructural) ||
                    rule.RuleValue == "All")
                {
                    results.Add(element);
                }
            }

            return results;
        }

        private bool IsStructuralElement(Element element)
        {
            return WorksetService.IsStructuralWall(element) ||
                   WorksetService.IsStructuralFloor(element) ||
                   WorksetService.IsStructuralColumn(element);
        }

        private bool EvaluateParameterCondition(Element element, WorksetRule rule)
        {
            if (string.IsNullOrEmpty(rule.ParameterName))
            {
                return true;
            }

            var param = element.LookupParameter(rule.ParameterName);
            if (param == null || !param.HasValue)
            {
                return false;
            }

            string paramValue = GetParameterValueAsString(param);
            return CompareValues(paramValue, rule.ParameterValue, rule.ComparisonType);
        }

        private string GetParameterValueAsString(Parameter param)
        {
            switch (param.StorageType)
            {
                case StorageType.String:
                    return param.AsString() ?? string.Empty;
                case StorageType.Integer:
                    return param.AsInteger().ToString();
                case StorageType.Double:
                    return param.AsDouble().ToString(CultureInfo.InvariantCulture);
                case StorageType.ElementId:
                    return param.AsElementId().ToString();
                default:
                    return string.Empty;
            }
        }

        private bool CompareValues(string actualValue, string expectedValue, ComparisonType comparisonType)
        {
            if (string.IsNullOrEmpty(actualValue))
            {
                return false;
            }

            // Try numeric comparison first
            if (double.TryParse(actualValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double actualNum) &&
                double.TryParse(expectedValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double expectedNum))
            {
                switch (comparisonType)
                {
                    case ComparisonType.Equals:
                        return Math.Abs(actualNum - expectedNum) < 0.0001;
                    case ComparisonType.NotEquals:
                        return Math.Abs(actualNum - expectedNum) >= 0.0001;
                    case ComparisonType.GreaterThan:
                        return actualNum > expectedNum;
                    case ComparisonType.LessThan:
                        return actualNum < expectedNum;
                }
            }

            // String comparison
            return CompareStrings(actualValue, expectedValue, comparisonType);
        }

        private bool CompareStrings(string actualValue, string expectedValue, ComparisonType comparisonType)
        {
            if (string.IsNullOrEmpty(actualValue))
            {
                return false;
            }

            switch (comparisonType)
            {
                case ComparisonType.Equals:
                    return actualValue.Equals(expectedValue, StringComparison.OrdinalIgnoreCase);
                case ComparisonType.NotEquals:
                    return !actualValue.Equals(expectedValue, StringComparison.OrdinalIgnoreCase);
                case ComparisonType.Contains:
                    return actualValue.IndexOf(expectedValue, StringComparison.OrdinalIgnoreCase) >= 0;
                case ComparisonType.StartsWith:
                    return actualValue.StartsWith(expectedValue, StringComparison.OrdinalIgnoreCase);
                case ComparisonType.EndsWith:
                    return actualValue.EndsWith(expectedValue, StringComparison.OrdinalIgnoreCase);
                default:
                    return false;
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    // Value converter for null to boolean
    public class NullToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Dialog for adding/editing workset configuration
    public class WorksetEditDialog : Window
    {
        private WpfTextBox _nameTextBox;
        private WpfTextBox _descriptionTextBox;

        public string WorksetName { get; private set; }
        public string WorksetDescription { get; private set; }

        public WorksetEditDialog(string name = "", string description = "")
        {
            Title = string.IsNullOrEmpty(name) ? "Add Workset Configuration" : "Edit Workset Configuration";
            Width = 500;
            Height = 250;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            var grid = new WpfGrid { Margin = new Thickness(15) };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Name
            var nameLabel = new TextBlock { Text = "Workset Name:", Margin = new Thickness(0, 0, 0, 5), FontWeight = FontWeights.SemiBold };
            System.Windows.Controls.Grid.SetRow(nameLabel, 0);
            grid.Children.Add(nameLabel);

            _nameTextBox = new WpfTextBox { Text = name, Margin = new Thickness(0, 0, 0, 15), Padding = new Thickness(8) };
            System.Windows.Controls.Grid.SetRow(_nameTextBox, 1);
            grid.Children.Add(_nameTextBox);

            // Description
            var descLabel = new TextBlock { Text = "Description:", Margin = new Thickness(0, 0, 0, 5), FontWeight = FontWeights.SemiBold };
            System.Windows.Controls.Grid.SetRow(descLabel, 2);
            grid.Children.Add(descLabel);

            _descriptionTextBox = new WpfTextBox
            {
                Text = description,
                Margin = new Thickness(0, 0, 0, 15),
                Padding = new Thickness(8),
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };
            System.Windows.Controls.Grid.SetRow(_descriptionTextBox, 2);
            grid.Children.Add(_descriptionTextBox);

            // Buttons
            var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            System.Windows.Controls.Grid.SetRow(buttonPanel, 3);

            var okButton = new Button { Content = "OK", Width = 80, Margin = new System.Windows.Thickness(0, 0, 10, 0), Padding = new System.Windows.Thickness(15, 8, 15, 8) };
            okButton.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(_nameTextBox.Text))
                {
                    MessageBox.Show("Please enter a workset name.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                WorksetName = _nameTextBox.Text.Trim();
                WorksetDescription = _descriptionTextBox.Text.Trim();
                DialogResult = true;
                Close();
            };
            buttonPanel.Children.Add(okButton);

            var cancelButton = new Button { Content = "Cancel", Width = 80, Padding = new System.Windows.Thickness(15, 8, 15, 8) };
            cancelButton.Click += (s, e) => { DialogResult = false; Close(); };
            buttonPanel.Children.Add(cancelButton);

            grid.Children.Add(buttonPanel);
            Content = grid;
        }
    }

    // Dialog for adding/editing rules
    public class RuleEditDialog : Window
    {
        private readonly Document _doc;
        private WpfComboBox _ruleTypeCombo;
        private WpfTextBox _ruleNameTextBox;
        private WpfComboBox _ruleValueCombo;
        private WpfTextBox _ruleValueTextBox;
        private WpfTextBox _parameterNameTextBox;
        private WpfTextBox _parameterValueTextBox;
        private WpfComboBox _comparisonTypeCombo;
        private CheckBox _enabledCheckBox;

        public RuleEditDialog(Document doc, WorksetRule existingRule = null)
        {
            _doc = doc;
            Title = existingRule == null ? "Add Rule" : "Edit Rule";
            Width = 550;
            Height = 450;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            BuildUI(existingRule);
        }

        private void BuildUI(WorksetRule existingRule)
        {
            var grid = new WpfGrid { Margin = new Thickness(15) };
            for (int i = 0; i < 8; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            }

            int row = 0;

            // Enabled
            _enabledCheckBox = new CheckBox
            {
                Content = "Enabled",
                IsChecked = existingRule?.Enabled ?? true,
                Margin = new Thickness(0, 0, 0, 15)
            };
            System.Windows.Controls.Grid.SetRow(_enabledCheckBox, row++);
            grid.Children.Add(_enabledCheckBox);

            // Rule Name
            AddLabel(grid, "Rule Name:", row++);
            _ruleNameTextBox = new WpfTextBox { Text = existingRule?.Name ?? "", Margin = new Thickness(0, 0, 0, 10), Padding = new Thickness(8) };
            System.Windows.Controls.Grid.SetRow(_ruleNameTextBox, row++);
            grid.Children.Add(_ruleNameTextBox);

            // Rule Type
            AddLabel(grid, "Rule Type:", row++);
            _ruleTypeCombo = new WpfComboBox { Margin = new Thickness(0, 0, 0, 10), Padding = new Thickness(8) };
            _ruleTypeCombo.ItemsSource = Enum.GetValues(typeof(RuleType));
            _ruleTypeCombo.SelectedItem = existingRule?.RuleType ?? RuleType.Category;
            _ruleTypeCombo.SelectionChanged += RuleTypeCombo_SelectionChanged;
            System.Windows.Controls.Grid.SetRow(_ruleTypeCombo, row++);
            grid.Children.Add(_ruleTypeCombo);

            // Rule Value (ComboBox for categories, TextBox for others)
            AddLabel(grid, "Rule Value:", row++);
            _ruleValueCombo = new WpfComboBox { Margin = new Thickness(0, 0, 0, 10), Padding = new Thickness(8), Visibility = WpfVisibility.Collapsed };
            _ruleValueTextBox = new WpfTextBox { Text = existingRule?.RuleValue ?? "", Margin = new Thickness(0, 0, 0, 10), Padding = new Thickness(8) };
            System.Windows.Controls.Grid.SetRow(_ruleValueCombo, row);
            System.Windows.Controls.Grid.SetRow(_ruleValueTextBox, row++);
            grid.Children.Add(_ruleValueCombo);
            grid.Children.Add(_ruleValueTextBox);

            // Parameter Name (optional)
            AddLabel(grid, "Parameter Name (optional):", row++);
            _parameterNameTextBox = new WpfTextBox { Text = existingRule?.ParameterName ?? "", Margin = new Thickness(0, 0, 0, 10), Padding = new Thickness(8) };
            System.Windows.Controls.Grid.SetRow(_parameterNameTextBox, row++);
            grid.Children.Add(_parameterNameTextBox);

            // Parameter Value
            AddLabel(grid, "Parameter Value (optional):", row++);
            _parameterValueTextBox = new WpfTextBox { Text = existingRule?.ParameterValue ?? "", Margin = new Thickness(0, 0, 0, 10), Padding = new Thickness(8) };
            System.Windows.Controls.Grid.SetRow(_parameterValueTextBox, row++);
            grid.Children.Add(_parameterValueTextBox);

            // Comparison Type
            AddLabel(grid, "Comparison Type:", row++);
            _comparisonTypeCombo = new WpfComboBox { Margin = new Thickness(0, 0, 0, 15), Padding = new Thickness(8) };
            _comparisonTypeCombo.ItemsSource = Enum.GetValues(typeof(ComparisonType));
            _comparisonTypeCombo.SelectedItem = existingRule?.ComparisonType ?? ComparisonType.Equals;
            System.Windows.Controls.Grid.SetRow(_comparisonTypeCombo, row++);
            grid.Children.Add(_comparisonTypeCombo);

            // Buttons
            var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            System.Windows.Controls.Grid.SetRow(buttonPanel, row++);

            var okButton = new Button { Content = "OK", Width = 80, Margin = new System.Windows.Thickness(0, 0, 10, 0), Padding = new System.Windows.Thickness(15, 8, 15, 8) };
            okButton.Click += OkButton_Click;
            buttonPanel.Children.Add(okButton);

            var cancelButton = new Button { Content = "Cancel", Width = 80, Padding = new System.Windows.Thickness(15, 8, 15, 8) };
            cancelButton.Click += (s, e) => { DialogResult = false; Close(); };
            buttonPanel.Children.Add(cancelButton);

            grid.Children.Add(buttonPanel);
            Content = grid;

            // Initialize UI based on rule type
            UpdateRuleValueUI();
        }

        private void AddLabel(WpfGrid grid, string text, int row)
        {
            var label = new TextBlock { Text = text, Margin = new Thickness(0, 0, 0, 5), FontWeight = FontWeights.SemiBold };
            System.Windows.Controls.Grid.SetRow(label, row);
            grid.Children.Add(label);
        }

        private void RuleTypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateRuleValueUI();
        }

        private void UpdateRuleValueUI()
        {
            if (_ruleTypeCombo.SelectedItem is RuleType ruleType)
            {
                if (ruleType == RuleType.Category)
                {
                    // Show ComboBox with categories
                    _ruleValueCombo.Visibility = WpfVisibility.Visible;
                    _ruleValueTextBox.Visibility = WpfVisibility.Collapsed;
                    PopulateCategoryComboBox();
                }
                else
                {
                    // Show TextBox
                    _ruleValueCombo.Visibility = WpfVisibility.Collapsed;
                    _ruleValueTextBox.Visibility = WpfVisibility.Visible;
                }
            }
        }

        private void PopulateCategoryComboBox()
        {
            var categories = new List<CategoryItem>();
            foreach (BuiltInCategory bic in Enum.GetValues(typeof(BuiltInCategory)))
            {
                try
                {
                    string displayName = LabelUtils.GetLabelFor(bic);
                    if (!string.IsNullOrEmpty(displayName))
                    {
                        categories.Add(new CategoryItem { DisplayName = displayName, CategoryId = (int)bic });
                    }
                }
                catch { }
            }

            _ruleValueCombo.ItemsSource = categories.OrderBy(c => c.DisplayName);
            _ruleValueCombo.DisplayMemberPath = "DisplayName";
            _ruleValueCombo.SelectedValuePath = "CategoryId";
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_ruleNameTextBox.Text))
            {
                MessageBox.Show("Please enter a rule name.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;
            Close();
        }

        public WorksetRule GetRule()
        {
            string ruleValue = "";
            if (_ruleValueCombo.Visibility == WpfVisibility.Visible && _ruleValueCombo.SelectedValue != null)
            {
                ruleValue = _ruleValueCombo.SelectedValue.ToString();
            }
            else
            {
                ruleValue = _ruleValueTextBox.Text.Trim();
            }

            return new WorksetRule
            {
                Enabled = _enabledCheckBox.IsChecked ?? true,
                Name = _ruleNameTextBox.Text.Trim(),
                RuleType = (RuleType)_ruleTypeCombo.SelectedItem,
                RuleValue = ruleValue,
                ParameterName = _parameterNameTextBox.Text.Trim(),
                ParameterValue = _parameterValueTextBox.Text.Trim(),
                ComparisonType = (ComparisonType)_comparisonTypeCombo.SelectedItem
            };
        }

        private class CategoryItem
        {
            public string DisplayName { get; set; }
            public int CategoryId { get; set; }
        }
    }

    // External event handler for applying worksets
    public class WorksetApplyHandler : IExternalEventHandler
    {
        public Document Document { get; set; }
        public WorksetService WorksetService { get; set; }
        public List<WorksetConfig> Configurations { get; set; }
        public Action<string> StatusCallback { get; set; }
        public Func<WorksetConfig, List<Element>> GetMatchingElementsFunc { get; set; }

        public void Execute(UIApplication app)
        {
            try
            {
                // Verify document is workshared
                if (!Document.IsWorkshared)
                {
                    TaskDialog.Show("Error", "Document is not workshared. Worksets can only be created in workshared documents.");
                    StatusCallback?.Invoke("Error: Document is not workshared");
                    return;
                }

                // Verify configurations
                if (Configurations == null || Configurations.Count == 0)
                {
                    TaskDialog.Show("Error", "No configurations provided to apply.");
                    StatusCallback?.Invoke("Error: No configurations");
                    return;
                }

                int totalAssigned = 0;
                int totalErrors = 0;
                var resultMessages = new List<string>();

                using (Transaction trans = new Transaction(Document, "Apply Workset Assignments"))
                {
                    trans.Start();

                    foreach (var config in Configurations)
                    {
                        int assignedCount = 0;
                        int errorCount = 0;

                        // Find or create workset
                        Workset targetWorkset = WorksetService.FindOrCreateWorkset(config.WorksetName);

                        if (targetWorkset == null)
                        {
                            resultMessages.Add($"❌ {config.WorksetName}: Failed to create workset");
                            continue;
                        }

                        // Get matching elements
                        var matchedElements = GetMatchingElementsFunc(config);

                        // Log how many elements were found
                        if (matchedElements.Count == 0)
                        {
                            resultMessages.Add($"⚠ {config.WorksetName}: No elements matched the rules (workset created but empty)");
                            continue;
                        }

                        // Assign elements to workset
                        foreach (var element in matchedElements)
                        {
                            try
                            {
                                WorksetService.ChangeElementWorkset(element, targetWorkset.Id);
                                assignedCount++;
                            }
                            catch
                            {
                                errorCount++;
                            }
                        }

                        totalAssigned += assignedCount;
                        totalErrors += errorCount;

                        string status = assignedCount > 0 ? "✓" : "○";
                        resultMessages.Add($"{status} {config.WorksetName}: {assignedCount} assigned, {errorCount} errors");
                    }

                    trans.Commit();
                }

                // Show results
                var resultDialog = new TaskDialog("Workset Assignment Complete");
                if (totalAssigned == 0)
                {
                    resultDialog.MainInstruction = "No elements were assigned";
                    resultDialog.MainContent = "Worksets were created but no elements matched the configured rules.\n\n" +
                                             string.Join("\n", resultMessages);
                }
                else
                {
                    resultDialog.MainInstruction = $"Successfully assigned {totalAssigned} elements";
                    resultDialog.MainContent = string.Join("\n", resultMessages);
                    if (totalErrors > 0)
                    {
                        resultDialog.MainContent += $"\n\nTotal errors: {totalErrors}";
                    }
                }
                resultDialog.Show();

                if (totalAssigned > 0)
                {
                    StatusCallback?.Invoke($"Applied: {totalAssigned} elements assigned to {Configurations.Count} worksets");
                }
                else
                {
                    StatusCallback?.Invoke("Worksets created but no elements were assigned");
                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", $"Error applying worksets:\n{ex.Message}");
            }
        }

        public string GetName()
        {
            return "Workset Apply Handler";
        }
    }
}
