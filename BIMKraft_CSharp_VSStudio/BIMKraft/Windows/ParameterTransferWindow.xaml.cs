using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BIMKraft.Models;
using Newtonsoft.Json;
using Color = System.Windows.Media.Color;
using Grid = System.Windows.Controls.Grid;
using ComboBox = System.Windows.Controls.ComboBox;
using TextBox = System.Windows.Controls.TextBox;

namespace BIMKraft.Windows
{
    public partial class ParameterTransferWindow : Window
    {
        private readonly Document _doc;
        private readonly string _presetsDirectory;
        private ParameterTransferPreset _currentPreset;
        private readonly Dictionary<string, BuiltInCategory> _availableCategories;

        public ParameterTransferWindow(Document doc)
        {
            InitializeComponent();

            _doc = doc;
            _currentPreset = new ParameterTransferPreset();

            // Set up presets directory
            string assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string assemblyDir = Path.GetDirectoryName(assemblyLocation);
            _presetsDirectory = Path.Combine(assemblyDir, "ParameterTransferPresets");

            if (!Directory.Exists(_presetsDirectory))
            {
                Directory.CreateDirectory(_presetsDirectory);
            }

            // Initialize category mappings
            _availableCategories = InitializeCategories();

            // Load UI
            LoadPresets();
            InitializeCategoryTabs();
        }

        #region Initialization

        private Dictionary<string, BuiltInCategory> InitializeCategories()
        {
            return new Dictionary<string, BuiltInCategory>
            {
                // Architectural
                { "Walls", BuiltInCategory.OST_Walls },
                { "Doors", BuiltInCategory.OST_Doors },
                { "Windows", BuiltInCategory.OST_Windows },
                { "Floors", BuiltInCategory.OST_Floors },
                { "Roofs", BuiltInCategory.OST_Roofs },
                { "Ceilings", BuiltInCategory.OST_Ceilings },
                { "Columns", BuiltInCategory.OST_Columns },
                { "Stairs", BuiltInCategory.OST_Stairs },
                { "Railings", BuiltInCategory.OST_Railings },
                { "Curtain Panels", BuiltInCategory.OST_CurtainWallPanels },
                { "Curtain Wall Mullions", BuiltInCategory.OST_CurtainWallMullions },

                // Structural
                { "Structural Framing", BuiltInCategory.OST_StructuralFraming },
                { "Structural Columns", BuiltInCategory.OST_StructuralColumns },
                { "Structural Foundations", BuiltInCategory.OST_StructuralFoundation },

                // MEP - Mechanical
                { "Mechanical Equipment", BuiltInCategory.OST_MechanicalEquipment },
                { "Ducts", BuiltInCategory.OST_DuctCurves },
                { "Duct Fittings", BuiltInCategory.OST_DuctFitting },
                { "Air Terminals", BuiltInCategory.OST_DuctTerminal },

                // MEP - Plumbing
                { "Plumbing Fixtures", BuiltInCategory.OST_PlumbingFixtures },
                { "Pipes", BuiltInCategory.OST_PipeCurves },
                { "Pipe Fittings", BuiltInCategory.OST_PipeFitting },

                // MEP - Electrical
                { "Electrical Equipment", BuiltInCategory.OST_ElectricalEquipment },
                { "Electrical Fixtures", BuiltInCategory.OST_ElectricalFixtures },
                { "Lighting Fixtures", BuiltInCategory.OST_LightingFixtures },
                { "Conduits", BuiltInCategory.OST_Conduit },
                { "Cable Trays", BuiltInCategory.OST_CableTray },

                // Other
                { "Generic Models", BuiltInCategory.OST_GenericModel },
                { "Furniture", BuiltInCategory.OST_Furniture },
                { "Specialty Equipment", BuiltInCategory.OST_SpecialityEquipment }
            };
        }

        private void InitializeCategoryTabs()
        {
            CategoryTabControl.Items.Clear();

            foreach (var categoryName in _availableCategories.Keys.OrderBy(k => k))
            {
                var tab = CreateCategoryTab(categoryName);
                CategoryTabControl.Items.Add(tab);
            }
        }

        private TabItem CreateCategoryTab(string categoryName)
        {
            var tab = new TabItem
            {
                Header = categoryName,
                Style = (Style)FindResource("CategoryTabStyle")
            };

            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Padding = new Thickness(10)
            };

            var mainStack = new StackPanel();

            // Header
            var headerText = new TextBlock
            {
                Text = $"Parameter Mappings for {categoryName}",
                Style = (Style)FindResource("SectionHeaderStyle"),
                Margin = new Thickness(0, 0, 0, 15)
            };
            mainStack.Children.Add(headerText);

            // Instructions
            var instructions = new TextBlock
            {
                Text = "Define parameter mappings to transfer values from source parameters (built-in or project) to target parameters. Click 'Add Mapping' to create new mappings.",
                TextWrapping = TextWrapping.Wrap,
                FontSize = 11,
                Foreground = new SolidColorBrush(Colors.Gray),
                Margin = new Thickness(0, 0, 0, 15)
            };
            mainStack.Children.Add(instructions);

            // Mappings container
            var mappingsPanel = new StackPanel
            {
                Name = $"MappingsPanel_{categoryName.Replace(" ", "_")}"
            };
            mainStack.Children.Add(mappingsPanel);

            // Add mapping button
            var addButton = new Button
            {
                Content = "+ Add Mapping",
                Margin = new Thickness(0, 10, 0, 0),
                Padding = new Thickness(10, 5, 10, 5),
                HorizontalAlignment = HorizontalAlignment.Left,
                Tag = categoryName
            };
            addButton.Click += AddMappingButton_Click;
            mainStack.Children.Add(addButton);

            scrollViewer.Content = mainStack;
            tab.Content = scrollViewer;

            return tab;
        }

        #endregion

        #region Mapping UI

        private void AddMappingButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            string categoryName = button.Tag.ToString();

            // Find the mappings panel for this category
            var tab = CategoryTabControl.Items.Cast<TabItem>()
                .FirstOrDefault(t => t.Header.ToString() == categoryName);

            if (tab != null)
            {
                var scrollViewer = tab.Content as ScrollViewer;
                var mainStack = scrollViewer.Content as StackPanel;
                var mappingsPanel = mainStack.Children.OfType<StackPanel>()
                    .FirstOrDefault(sp => sp.Name == $"MappingsPanel_{categoryName.Replace(" ", "_")}");

                if (mappingsPanel != null)
                {
                    var mappingUI = CreateMappingUI(categoryName, null);
                    mappingsPanel.Children.Add(mappingUI);
                }
            }
        }

        private Border CreateMappingUI(string categoryName, ParameterMapping existingMapping)
        {
            var border = new Border
            {
                BorderBrush = new SolidColorBrush(Colors.LightGray),
                BorderThickness = new Thickness(1),
                Margin = new Thickness(0, 5, 0, 5),
                Padding = new Thickness(10),
                Background = new SolidColorBrush(Color.FromRgb(250, 250, 250))
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });

            // Source parameter combo
            var sourceLabel = new TextBlock
            {
                Text = "Source:",
                FontSize = 10,
                Foreground = new SolidColorBrush(Colors.Gray)
            };

            var sourceCombo = new ComboBox
            {
                Margin = new Thickness(0, 3, 5, 0),
                Tag = "Source",
                ToolTip = "Select the source parameter to copy from"
            };
            PopulateParameterComboBox(sourceCombo, categoryName);

            if (existingMapping != null && !string.IsNullOrEmpty(existingMapping.SourceParameter))
            {
                sourceCombo.SelectedItem = existingMapping.SourceParameter;
            }

            var sourceStack = new StackPanel();
            sourceStack.Children.Add(sourceLabel);
            sourceStack.Children.Add(sourceCombo);
            Grid.SetColumn(sourceStack, 0);

            // Arrow
            var arrow = new TextBlock
            {
                Text = "→",
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush(Colors.DodgerBlue)
            };
            Grid.SetColumn(arrow, 1);

            // Target parameter combo
            var targetLabel = new TextBlock
            {
                Text = "Target:",
                FontSize = 10,
                Foreground = new SolidColorBrush(Colors.Gray)
            };

            var targetCombo = new ComboBox
            {
                Margin = new Thickness(5, 3, 5, 0),
                Tag = "Target",
                ToolTip = "Select the target parameter to copy to"
            };
            PopulateParameterComboBox(targetCombo, categoryName);

            if (existingMapping != null && !string.IsNullOrEmpty(existingMapping.TargetParameter))
            {
                targetCombo.SelectedItem = existingMapping.TargetParameter;
            }

            var targetStack = new StackPanel();
            targetStack.Children.Add(targetLabel);
            targetStack.Children.Add(targetCombo);
            Grid.SetColumn(targetStack, 2);

            // Enabled checkbox
            var enabledCheckBox = new CheckBox
            {
                Content = "Enabled",
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(5, 0, 5, 0),
                IsChecked = existingMapping?.Enabled ?? true,
                Tag = "Enabled"
            };
            Grid.SetColumn(enabledCheckBox, 3);

            // Remove button
            var removeButton = new Button
            {
                Content = "Remove",
                Margin = new Thickness(5, 0, 0, 0),
                Padding = new Thickness(8, 3, 8, 3),
                Tag = border
            };
            removeButton.Click += RemoveMappingButton_Click;
            Grid.SetColumn(removeButton, 4);

            grid.Children.Add(sourceStack);
            grid.Children.Add(arrow);
            grid.Children.Add(targetStack);
            grid.Children.Add(enabledCheckBox);
            grid.Children.Add(removeButton);

            border.Child = grid;
            border.Tag = categoryName;

            return border;
        }

        private void RemoveMappingButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var border = button.Tag as Border;

            if (border != null && border.Parent is StackPanel parent)
            {
                parent.Children.Remove(border);
            }
        }

        private void PopulateParameterComboBox(ComboBox combo, string categoryName)
        {
            var parameters = GetAllParametersForCategory(categoryName);
            combo.Items.Clear();

            foreach (var param in parameters.OrderBy(p => p))
            {
                combo.Items.Add(param);
            }
        }

        private List<string> GetAllParametersForCategory(string categoryName)
        {
            var parameters = new HashSet<string>();

            try
            {
                if (!_availableCategories.ContainsKey(categoryName))
                    return parameters.ToList();

                BuiltInCategory builtInCat = _availableCategories[categoryName];
                Category category = _doc.Settings.Categories.get_Item(builtInCat);

                if (category == null)
                    return parameters.ToList();

                // Get sample elements from this category
                FilteredElementCollector collector = new FilteredElementCollector(_doc)
                    .OfCategoryId(category.Id)
                    .WhereElementIsNotElementType();

                // Also get types
                FilteredElementCollector typeCollector = new FilteredElementCollector(_doc)
                    .OfCategoryId(category.Id)
                    .WhereElementIsElementType();

                var sampleElements = collector.Take(50).ToList();
                var sampleTypes = typeCollector.Take(50).ToList();
                var allSamples = sampleElements.Concat(sampleTypes).ToList();

                if (allSamples.Count == 0)
                {
                    // If no elements exist, get parameters from a generic element
                    // This will at least show built-in parameters
                    return parameters.ToList();
                }

                // Collect parameters from sample elements
                foreach (var element in allSamples)
                {
                    if (element == null) continue;

                    foreach (Parameter param in element.Parameters)
                    {
                        try
                        {
                            if (param == null || param.Definition == null) continue;
                            string paramName = param.Definition.Name;
                            if (!string.IsNullOrEmpty(paramName))
                            {
                                parameters.Add(paramName);
                            }
                        }
                        catch { }
                    }
                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Parameter Collection Error",
                    $"Error collecting parameters for {categoryName}:\n{ex.Message}");
            }

            return parameters.ToList();
        }

        #endregion

        #region Presets

        private void LoadPresets()
        {
            PresetsComboBox.Items.Clear();
            PresetsComboBox.Items.Add("-- Select Preset --");

            try
            {
                if (Directory.Exists(_presetsDirectory))
                {
                    foreach (var file in Directory.GetFiles(_presetsDirectory, "*.json"))
                    {
                        string presetName = Path.GetFileNameWithoutExtension(file);
                        PresetsComboBox.Items.Add(presetName);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading presets: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            PresetsComboBox.SelectedIndex = 0;
        }

        private void PresetsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Auto-load on selection change
            if (PresetsComboBox.SelectedIndex > 0)
            {
                LoadSelectedPreset();
            }
        }

        private void LoadPresetButton_Click(object sender, RoutedEventArgs e)
        {
            if (PresetsComboBox.SelectedIndex <= 0)
            {
                MessageBox.Show("Please select a preset to load.", "No Preset Selected",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            LoadSelectedPreset();
        }

        private void LoadSelectedPreset()
        {
            string presetName = PresetsComboBox.SelectedItem.ToString();
            string presetFile = Path.Combine(_presetsDirectory, $"{presetName}.json");

            try
            {
                string json = File.ReadAllText(presetFile);
                _currentPreset = JsonConvert.DeserializeObject<ParameterTransferPreset>(json);

                // Clear all existing mappings
                ClearAllMappings();

                // Populate mappings from preset
                foreach (var categoryMapping in _currentPreset.CategoryMappings)
                {
                    var tab = CategoryTabControl.Items.Cast<TabItem>()
                        .FirstOrDefault(t => t.Header.ToString() == categoryMapping.CategoryName);

                    if (tab != null)
                    {
                        var scrollViewer = tab.Content as ScrollViewer;
                        var mainStack = scrollViewer.Content as StackPanel;
                        var mappingsPanel = mainStack.Children.OfType<StackPanel>()
                            .FirstOrDefault(sp => sp.Name == $"MappingsPanel_{categoryMapping.CategoryName.Replace(" ", "_")}");

                        if (mappingsPanel != null)
                        {
                            foreach (var mapping in categoryMapping.Mappings)
                            {
                                var mappingUI = CreateMappingUI(categoryMapping.CategoryName, mapping);
                                mappingsPanel.Children.Add(mappingUI);
                            }
                        }
                    }
                }

                StatusTextBlock.Text = $"Loaded preset: {presetName}";
                StatusTextBlock.Foreground = new SolidColorBrush(Colors.Green);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading preset: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SavePresetButton_Click(object sender, RoutedEventArgs e)
        {
            // Collect all mappings from UI
            var preset = CollectMappingsFromUI();

            if (preset.CategoryMappings.Sum(cm => cm.Mappings.Count) == 0)
            {
                MessageBox.Show("No mappings configured to save.", "No Mappings",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Create input dialog
            var inputDialog = new Window
            {
                Title = "Save Preset",
                Width = 400,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                ResizeMode = ResizeMode.NoResize
            };

            var stackPanel = new StackPanel { Margin = new Thickness(15) };

            var nameLabel = new TextBlock
            {
                Text = "Preset Name:",
                Margin = new Thickness(0, 0, 0, 5)
            };
            stackPanel.Children.Add(nameLabel);

            var nameTextBox = new TextBox
            {
                Margin = new Thickness(0, 0, 0, 10)
            };
            stackPanel.Children.Add(nameTextBox);

            var descLabel = new TextBlock
            {
                Text = "Description (optional):",
                Margin = new Thickness(0, 0, 0, 5)
            };
            stackPanel.Children.Add(descLabel);

            var descTextBox = new TextBox
            {
                Margin = new Thickness(0, 0, 0, 15),
                Height = 50,
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true
            };
            stackPanel.Children.Add(descTextBox);

            var buttonStack = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            var okButton = new Button
            {
                Content = "Save",
                Width = 75,
                Margin = new Thickness(0, 0, 10, 0),
                IsDefault = true
            };
            okButton.Click += (s, args) =>
            {
                inputDialog.DialogResult = true;
                inputDialog.Close();
            };

            var cancelButton = new Button
            {
                Content = "Cancel",
                Width = 75,
                IsCancel = true
            };

            buttonStack.Children.Add(okButton);
            buttonStack.Children.Add(cancelButton);
            stackPanel.Children.Add(buttonStack);

            inputDialog.Content = stackPanel;

            if (inputDialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(nameTextBox.Text))
            {
                string presetName = nameTextBox.Text.Trim();
                preset.PresetName = presetName;
                preset.Description = descTextBox.Text.Trim();

                try
                {
                    string json = JsonConvert.SerializeObject(preset, Formatting.Indented);
                    string presetFile = Path.Combine(_presetsDirectory, $"{presetName}.json");

                    File.WriteAllText(presetFile, json);

                    MessageBox.Show($"Preset '{presetName}' saved successfully!", "Preset Saved",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    LoadPresets();
                    StatusTextBlock.Text = $"Saved preset: {presetName}";
                    StatusTextBlock.Foreground = new SolidColorBrush(Colors.Green);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving preset: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DeletePresetButton_Click(object sender, RoutedEventArgs e)
        {
            if (PresetsComboBox.SelectedIndex <= 0)
            {
                MessageBox.Show("Please select a preset to delete.", "No Preset Selected",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string presetName = PresetsComboBox.SelectedItem.ToString();

            var result = MessageBox.Show($"Are you sure you want to delete preset '{presetName}'?",
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    string presetFile = Path.Combine(_presetsDirectory, $"{presetName}.json");
                    File.Delete(presetFile);

                    MessageBox.Show($"Preset '{presetName}' deleted successfully!", "Preset Deleted",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    LoadPresets();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting preset: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private ParameterTransferPreset CollectMappingsFromUI()
        {
            var preset = new ParameterTransferPreset();

            foreach (TabItem tab in CategoryTabControl.Items)
            {
                string categoryName = tab.Header.ToString();
                var scrollViewer = tab.Content as ScrollViewer;
                var mainStack = scrollViewer.Content as StackPanel;
                var mappingsPanel = mainStack.Children.OfType<StackPanel>()
                    .FirstOrDefault(sp => sp.Name == $"MappingsPanel_{categoryName.Replace(" ", "_")}");

                if (mappingsPanel != null)
                {
                    var categoryMapping = new CategoryMappingConfig
                    {
                        CategoryName = categoryName
                    };

                    foreach (Border mappingBorder in mappingsPanel.Children.OfType<Border>())
                    {
                        var grid = mappingBorder.Child as Grid;
                        if (grid == null) continue;

                        var sourceCombo = grid.Children.OfType<StackPanel>().FirstOrDefault()?.Children.OfType<ComboBox>().FirstOrDefault();
                        var targetCombo = grid.Children.OfType<StackPanel>().LastOrDefault()?.Children.OfType<ComboBox>().FirstOrDefault();
                        var enabledCheckBox = grid.Children.OfType<CheckBox>().FirstOrDefault();

                        if (sourceCombo != null && targetCombo != null &&
                            sourceCombo.SelectedItem != null && targetCombo.SelectedItem != null)
                        {
                            var mapping = new ParameterMapping
                            {
                                SourceParameter = sourceCombo.SelectedItem.ToString(),
                                TargetParameter = targetCombo.SelectedItem.ToString(),
                                Enabled = enabledCheckBox?.IsChecked ?? true
                            };

                            categoryMapping.Mappings.Add(mapping);
                        }
                    }

                    if (categoryMapping.Mappings.Count > 0)
                    {
                        preset.CategoryMappings.Add(categoryMapping);
                    }
                }
            }

            return preset;
        }

        private void ClearAllMappings()
        {
            foreach (TabItem tab in CategoryTabControl.Items)
            {
                string categoryName = tab.Header.ToString();
                var scrollViewer = tab.Content as ScrollViewer;
                var mainStack = scrollViewer.Content as StackPanel;
                var mappingsPanel = mainStack.Children.OfType<StackPanel>()
                    .FirstOrDefault(sp => sp.Name == $"MappingsPanel_{categoryName.Replace(" ", "_")}");

                if (mappingsPanel != null)
                {
                    mappingsPanel.Children.Clear();
                }
            }
        }

        #endregion

        #region Parameter Transfer

        private void TransferParametersButton_Click(object sender, RoutedEventArgs e)
        {
            // Collect mappings
            var preset = CollectMappingsFromUI();

            if (preset.CategoryMappings.Sum(cm => cm.Mappings.Count) == 0)
            {
                MessageBox.Show("No mappings configured. Please add parameter mappings before transferring.",
                    "No Mappings", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Get transfer options
            bool overwriteExisting = OverwriteExistingCheckBox.IsChecked ?? false;
            bool onlyNonEmptySource = OnlyNonEmptySourceCheckBox.IsChecked ?? true;
            bool showDetailedLog = ShowDetailedLogCheckBox.IsChecked ?? true;

            // Confirm
            int totalMappings = preset.CategoryMappings.Sum(cm => cm.Mappings.Where(m => m.Enabled).Count());
            var confirmResult = MessageBox.Show(
                $"Ready to transfer parameters:\n\n" +
                $"• Total mappings: {totalMappings}\n" +
                $"• Categories: {preset.CategoryMappings.Count}\n" +
                $"• Overwrite existing: {(overwriteExisting ? "Yes" : "No")}\n" +
                $"• Only non-empty source: {(onlyNonEmptySource ? "Yes" : "No")}\n\n" +
                $"This operation will modify element parameters. Continue?",
                "Confirm Parameter Transfer",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirmResult != MessageBoxResult.Yes)
                return;

            // Perform transfer
            try
            {
                var results = PerformParameterTransfer(preset, overwriteExisting, onlyNonEmptySource);

                // Show results
                string resultMessage = $"Parameter Transfer Complete!\n\n" +
                    $"Total processed: {results.TotalProcessed}\n" +
                    $"Successful: {results.SuccessCount}\n" +
                    $"Failed: {results.FailedCount}\n" +
                    $"Skipped: {results.SkippedCount}";

                if (showDetailedLog && results.Details.Count > 0)
                {
                    resultMessage += "\n\nDetailed Log (first 50 entries):\n\n";
                    resultMessage += string.Join("\n", results.Details.Take(50));

                    if (results.Details.Count > 50)
                    {
                        resultMessage += $"\n\n... and {results.Details.Count - 50} more entries";
                    }
                }

                MessageBox.Show(resultMessage, "Transfer Complete",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                StatusTextBlock.Text = $"Transfer complete: {results.SuccessCount} successful";
                StatusTextBlock.Foreground = new SolidColorBrush(Colors.Green);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during transfer: {ex.Message}\n\n{ex.StackTrace}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private TransferResults PerformParameterTransfer(ParameterTransferPreset preset,
            bool overwriteExisting, bool onlyNonEmptySource)
        {
            var results = new TransferResults();

            using (Transaction trans = new Transaction(_doc, "Transfer Parameters"))
            {
                trans.Start();

                foreach (var categoryMapping in preset.CategoryMappings)
                {
                    if (!_availableCategories.ContainsKey(categoryMapping.CategoryName))
                        continue;

                    BuiltInCategory builtInCat = _availableCategories[categoryMapping.CategoryName];

                    try
                    {
                        Category category = _doc.Settings.Categories.get_Item(builtInCat);
                        if (category == null) continue;

                        // Get all elements in this category
                        FilteredElementCollector collector = new FilteredElementCollector(_doc)
                            .OfCategoryId(category.Id)
                            .WhereElementIsNotElementType();

                        foreach (var element in collector)
                        {
                            foreach (var mapping in categoryMapping.Mappings.Where(m => m.Enabled))
                            {
                                try
                                {
                                    results.TotalProcessed++;

                                    Parameter sourceParam = element.LookupParameter(mapping.SourceParameter);
                                    Parameter targetParam = element.LookupParameter(mapping.TargetParameter);

                                    if (sourceParam == null)
                                    {
                                        results.SkippedCount++;
#if REVIT2025
                                        results.Details.Add($"SKIP: Element {element.Id.Value} - Source parameter '{mapping.SourceParameter}' not found");
#else
                                        results.Details.Add($"SKIP: Element {element.Id.IntegerValue} - Source parameter '{mapping.SourceParameter}' not found");
#endif
                                        continue;
                                    }

                                    if (targetParam == null)
                                    {
                                        results.SkippedCount++;
#if REVIT2025
                                        results.Details.Add($"SKIP: Element {element.Id.Value} - Target parameter '{mapping.TargetParameter}' not found");
#else
                                        results.Details.Add($"SKIP: Element {element.Id.IntegerValue} - Target parameter '{mapping.TargetParameter}' not found");
#endif
                                        continue;
                                    }

                                    if (targetParam.IsReadOnly)
                                    {
                                        results.SkippedCount++;
#if REVIT2025
                                        results.Details.Add($"SKIP: Element {element.Id.Value} - Target parameter is read-only");
#else
                                        results.Details.Add($"SKIP: Element {element.Id.IntegerValue} - Target parameter is read-only");
#endif
                                        continue;
                                    }

                                    // Check if source has value
                                    if (onlyNonEmptySource && !sourceParam.HasValue)
                                    {
                                        results.SkippedCount++;
                                        continue;
                                    }

                                    // Check if target already has value
                                    if (!overwriteExisting && targetParam.HasValue)
                                    {
                                        bool targetEmpty = false;
                                        if (targetParam.StorageType == StorageType.String)
                                        {
                                            targetEmpty = string.IsNullOrEmpty(targetParam.AsString());
                                        }
                                        else if (targetParam.StorageType == StorageType.ElementId)
                                        {
                                            targetEmpty = targetParam.AsElementId() == ElementId.InvalidElementId;
                                        }

                                        if (!targetEmpty)
                                        {
                                            results.SkippedCount++;
                                            continue;
                                        }
                                    }

                                    // Transfer value based on storage type
                                    bool transferred = TransferParameterValue(sourceParam, targetParam);

                                    if (transferred)
                                    {
                                        results.SuccessCount++;
#if REVIT2025
                                        results.Details.Add($"SUCCESS: Element {element.Id.Value} - {mapping.SourceParameter} → {mapping.TargetParameter}");
#else
                                        results.Details.Add($"SUCCESS: Element {element.Id.IntegerValue} - {mapping.SourceParameter} → {mapping.TargetParameter}");
#endif
                                    }
                                    else
                                    {
                                        results.FailedCount++;
#if REVIT2025
                                        results.Details.Add($"FAIL: Element {element.Id.Value} - Type mismatch or error");
#else
                                        results.Details.Add($"FAIL: Element {element.Id.IntegerValue} - Type mismatch or error");
#endif
                                    }
                                }
                                catch (Exception ex)
                                {
                                    results.FailedCount++;
#if REVIT2025
                                    results.Details.Add($"ERROR: Element {element.Id.Value} - {ex.Message}");
#else
                                    results.Details.Add($"ERROR: Element {element.Id.IntegerValue} - {ex.Message}");
#endif
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        results.Details.Add($"ERROR: Category {categoryMapping.CategoryName} - {ex.Message}");
                    }
                }

                trans.Commit();
            }

            return results;
        }

        private bool TransferParameterValue(Parameter source, Parameter target)
        {
            try
            {
                if (source.StorageType != target.StorageType)
                {
                    // Try to handle some type conversions
                    if (source.StorageType == StorageType.Double && target.StorageType == StorageType.String)
                    {
                        target.Set(source.AsDouble().ToString());
                        return true;
                    }
                    else if (source.StorageType == StorageType.Integer && target.StorageType == StorageType.String)
                    {
                        target.Set(source.AsInteger().ToString());
                        return true;
                    }
                    else if (source.StorageType == StorageType.String && target.StorageType == StorageType.Double)
                    {
                        if (double.TryParse(source.AsString(), out double val))
                        {
                            target.Set(val);
                            return true;
                        }
                    }
                    return false;
                }

                switch (source.StorageType)
                {
                    case StorageType.String:
                        target.Set(source.AsString() ?? "");
                        return true;

                    case StorageType.Integer:
                        target.Set(source.AsInteger());
                        return true;

                    case StorageType.Double:
                        target.Set(source.AsDouble());
                        return true;

                    case StorageType.ElementId:
                        target.Set(source.AsElementId());
                        return true;

                    default:
                        return false;
                }
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Helper Classes

        private class TransferResults
        {
            public int TotalProcessed { get; set; }
            public int SuccessCount { get; set; }
            public int FailedCount { get; set; }
            public int SkippedCount { get; set; }
            public List<string> Details { get; set; }

            public TransferResults()
            {
                Details = new List<string>();
            }
        }

        #endregion

        #region Window Actions

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        #endregion
    }
}
