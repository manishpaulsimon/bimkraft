using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BIMKraft.Models;
using BIMKraft.Services;
using Microsoft.Win32;
using System.IO;

namespace BIMKraft.Windows
{
    public partial class LineLengthCalculatorWindow : Window
    {
        private Document _doc;
        private UIDocument _uiDoc;
        private ObservableCollection<LineLengthItem> _lineGroups;
        private List<ElementId> _selectedElementIds;
        private LineLengthStatistics _statistics;
        private ExternalEvent _applyColorsEvent;
        private ApplyColorsHandler _applyColorsHandler;

        // Predefined color palette for auto-assignment
        private readonly string[] ColorPalette = new string[]
        {
            "#E74C3C", "#3498DB", "#2ECC71", "#F39C12", "#9B59B6",
            "#1ABC9C", "#E67E22", "#34495E", "#16A085", "#C0392B",
            "#2980B9", "#8E44AD", "#D35400", "#27AE60", "#F1C40F",
            "#95A5A6", "#DC7633", "#5DADE2", "#48C9B0", "#EC7063"
        };

        public LineLengthCalculatorWindow(Document doc, UIDocument uiDoc)
        {
            InitializeComponent();
            _doc = doc;
            _uiDoc = uiDoc;
            _lineGroups = new ObservableCollection<LineLengthItem>();
            _selectedElementIds = new List<ElementId>();
            _statistics = new LineLengthStatistics();

            // Create external event for applying colors
            _applyColorsHandler = new ApplyColorsHandler();
            _applyColorsEvent = ExternalEvent.Create(_applyColorsHandler);

            LineGroupsDataGrid.ItemsSource = _lineGroups;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Load available presets
            LoadPresetsList();

            // Check if there's a previous selection to restore
            var lastSelection = LineLengthPresetService.LoadLastSelection();
            if (lastSelection != null && lastSelection.Count > 0)
            {
                RestoreSelectionButton.IsEnabled = true;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Dispose of the external event to prevent crashes
            try
            {
                if (_applyColorsEvent != null)
                {
                    _applyColorsEvent.Dispose();
                    _applyColorsEvent = null;
                }
                _applyColorsHandler = null;
            }
            catch
            {
                // Ignore any errors during disposal
            }
        }

        private void SelectLines_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Hide();

                // Allow user to select lines
                _selectedElementIds.Clear();
                IList<Reference> selectedRefs = _uiDoc.Selection.PickObjects(
                    Autodesk.Revit.UI.Selection.ObjectType.Element,
                    new LineSelectionFilter(),
                    "Select Detail Lines and/or Model Lines");

                foreach (Reference selRef in selectedRefs)
                {
                    _selectedElementIds.Add(selRef.ElementId);
                }

                UseSelectionCheckBox.IsChecked = _selectedElementIds.Count > 0;

                // Save the selection for later restoration
#if REVIT2025
                LineLengthPresetService.SaveLastSelection(_selectedElementIds.Select(id => (int)id.Value).ToList());
#else
                LineLengthPresetService.SaveLastSelection(_selectedElementIds.Select(id => id.IntegerValue).ToList());
#endif
                RestoreSelectionButton.IsEnabled = true;

                Show();
                Activate(); // Fix: Bring window back to focus
                Focus();

                TaskDialog.Show("Selection Complete",
                    $"{_selectedElementIds.Count} line(s) selected.");
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                Show();
                Activate(); // Fix: Bring window back to focus
                Focus();
            }
            catch (Exception ex)
            {
                Show();
                Activate(); // Fix: Bring window back to focus
                Focus();
                TaskDialog.Show("Error", $"Error selecting lines:\n{ex.Message}");
            }
        }

        private void Calculate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _lineGroups.Clear();

                // Collect lines based on selection option
                List<Element> lines = CollectLines();

                if (lines.Count == 0)
                {
                    TaskDialog.Show("No Lines Found",
                        "No Detail Lines or Model Lines found in the current view or selection.\n\n" +
                        "Please ensure you have lines in your view or select specific lines.");
                    return;
                }

                // Group connected lines
                List<LineLengthItem> groups = GroupConnectedLines(lines);

                // Auto-assign colors
                AssignColors(groups);

                // Populate data grid
                foreach (var group in groups)
                {
                    _lineGroups.Add(group);
                }

                // Update statistics
                UpdateStatistics();

                // Enable buttons
                ExportExcelButton.IsEnabled = true;
                ExportCSVButton.IsEnabled = true;
                CopyButton.IsEnabled = true;
                ApplyColorsButton.IsEnabled = true;
                SavePresetButton.IsEnabled = true;

                TaskDialog.Show("Calculation Complete",
                    $"Found {_lineGroups.Count} connected line group(s) with a total of {_statistics.TotalLines} line(s).");
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", $"Error calculating line lengths:\n{ex.Message}\n\n{ex.StackTrace}");
            }
        }

        private List<Element> CollectLines()
        {
            List<Element> lines = new List<Element>();

            if (UseSelectionCheckBox.IsChecked == true && _selectedElementIds.Count > 0)
            {
                // Use selected elements
                foreach (ElementId id in _selectedElementIds)
                {
                    Element elem = _doc.GetElement(id);
                    if (IsValidLine(elem))
                    {
                        lines.Add(elem);
                    }
                }
            }
            else
            {
                // Collect all detail lines and model lines in the active view
                View activeView = _doc.ActiveView;

                // Get Detail Lines
                FilteredElementCollector detailLineCollector = new FilteredElementCollector(_doc, activeView.Id)
                    .OfClass(typeof(CurveElement));

                foreach (Element elem in detailLineCollector)
                {
                    if (elem is DetailLine || elem is DetailCurve)
                    {
                        lines.Add(elem);
                    }
                }

                // Get Model Lines
                FilteredElementCollector modelLineCollector = new FilteredElementCollector(_doc, activeView.Id)
                    .OfClass(typeof(CurveElement));

                foreach (Element elem in modelLineCollector)
                {
                    if (elem is ModelLine || elem is ModelCurve)
                    {
                        lines.Add(elem);
                    }
                }
            }

            return lines;
        }

        private bool IsValidLine(Element elem)
        {
            return elem is DetailLine || elem is DetailCurve || elem is ModelLine || elem is ModelCurve;
        }

        private List<LineLengthItem> GroupConnectedLines(List<Element> lines)
        {
            List<LineLengthItem> groups = new List<LineLengthItem>();
            HashSet<ElementId> processedLines = new HashSet<ElementId>();
            int groupId = 1;

            foreach (Element line in lines)
            {
                if (processedLines.Contains(line.Id))
                    continue;

                // Start a new group
                List<Element> connectedGroup = new List<Element>();
                Queue<Element> queue = new Queue<Element>();
                queue.Enqueue(line);
                processedLines.Add(line.Id);

                while (queue.Count > 0)
                {
                    Element currentLine = queue.Dequeue();
                    connectedGroup.Add(currentLine);

                    // Find connected lines
                    foreach (Element otherLine in lines)
                    {
                        if (processedLines.Contains(otherLine.Id))
                            continue;

                        if (AreConnected(currentLine, otherLine))
                        {
                            queue.Enqueue(otherLine);
                            processedLines.Add(otherLine.Id);
                        }
                    }
                }

                // Create group item
                LineLengthItem groupItem = CreateGroupItem(connectedGroup, groupId);
                groups.Add(groupItem);
                groupId++;
            }

            return groups;
        }

        private bool AreConnected(Element line1, Element line2)
        {
            // Check if lines have the same graphics style (line style)
            GraphicsStyle gs1 = GetGraphicsStyle(line1);
            GraphicsStyle gs2 = GetGraphicsStyle(line2);

            if (gs1 == null || gs2 == null || gs1.Id != gs2.Id)
                return false;

            // Get curve endpoints
            Curve curve1 = GetCurve(line1);
            Curve curve2 = GetCurve(line2);

            if (curve1 == null || curve2 == null)
                return false;

            XYZ p1_start = curve1.GetEndPoint(0);
            XYZ p1_end = curve1.GetEndPoint(1);
            XYZ p2_start = curve2.GetEndPoint(0);
            XYZ p2_end = curve2.GetEndPoint(1);

            double tolerance = 0.001; // Approximately 3mm

            // Check if any endpoints coincide
            return IsPointsCoincident(p1_start, p2_start, tolerance) ||
                   IsPointsCoincident(p1_start, p2_end, tolerance) ||
                   IsPointsCoincident(p1_end, p2_start, tolerance) ||
                   IsPointsCoincident(p1_end, p2_end, tolerance);
        }

        private bool IsPointsCoincident(XYZ p1, XYZ p2, double tolerance)
        {
            return p1.DistanceTo(p2) < tolerance;
        }

        private GraphicsStyle GetGraphicsStyle(Element line)
        {
            if (line is CurveElement curveElem)
            {
                return curveElem.LineStyle as GraphicsStyle;
            }
            return null;
        }

        private Curve GetCurve(Element line)
        {
            if (line is CurveElement curveElem)
            {
                return curveElem.GeometryCurve;
            }
            return null;
        }

        private LineLengthItem CreateGroupItem(List<Element> group, int groupId)
        {
            LineLengthItem item = new LineLengthItem
            {
                GroupId = groupId,
                LineCount = group.Count,
                ElementIds = group.Select(e => e.Id).ToList()
            };

            // Calculate total length
            double totalLength = 0;
            foreach (Element line in group)
            {
                Curve curve = GetCurve(line);
                if (curve != null)
                {
#if REVIT2025
                    totalLength += UnitUtils.ConvertFromInternalUnits(curve.Length, UnitTypeId.Meters);
#else
                    totalLength += UnitUtils.ConvertFromInternalUnits(curve.Length, DisplayUnitType.DUT_METERS);
#endif
                }
            }

            item.TotalLength = totalLength;
            item.TotalLengthFormatted = $"{totalLength:F2} m";

            // Get line style
            GraphicsStyle gs = GetGraphicsStyle(group.First());
            item.LineStyle = gs != null ? gs.Name : "Unknown";

            // Get line type
            Element firstLine = group.First();
            if (firstLine is DetailLine || firstLine is DetailCurve)
            {
                item.LineType = "Detail Line";

                // Get view name for detail lines
                if (firstLine.OwnerViewId != ElementId.InvalidElementId)
                {
                    View view = _doc.GetElement(firstLine.OwnerViewId) as View;
                    item.ViewName = view != null ? view.Name : "Unknown";
                }
            }
            else if (firstLine is ModelLine || firstLine is ModelCurve)
            {
                item.LineType = "Model Line";
                item.ViewName = "N/A (Model)";
            }

            // Create element IDs string
#if REVIT2025
            item.ElementIdsString = string.Join(", ", item.ElementIds.Select(id => id.Value.ToString()));
#else
            item.ElementIdsString = string.Join(", ", item.ElementIds.Select(id => id.IntegerValue.ToString()));
#endif

            return item;
        }

        private void AssignColors(List<LineLengthItem> groups)
        {
            int paletteIndex = 0;

            for (int i = 0; i < groups.Count; i++)
            {
                // Try to restore color from extensible storage
                string storedColor = TryGetGroupStoredColor(groups[i]);

                if (!string.IsNullOrEmpty(storedColor))
                {
                    // Use the stored color
                    groups[i].ColorHex = storedColor;
                }
                else
                {
                    // Assign a new color from the palette
                    groups[i].ColorHex = ColorPalette[paletteIndex % ColorPalette.Length];
                    paletteIndex++;
                }

                // Try to restore description from extensible storage
                string storedDescription = TryGetGroupStoredDescription(groups[i]);
                if (!string.IsNullOrEmpty(storedDescription))
                {
                    groups[i].Description = storedDescription;
                }
            }
        }

        /// <summary>
        /// Tries to get a consistent stored color for all lines in a group
        /// </summary>
        private string TryGetGroupStoredColor(LineLengthItem group)
        {
            if (group == null || group.ElementIds == null || group.ElementIds.Count == 0)
                return null;

            string firstColor = null;

            foreach (ElementId id in group.ElementIds)
            {
                Element line = _doc.GetElement(id);
                if (line == null)
                    continue;

                string storedColor = LineColorStorageService.GetStoredColor(line);

                if (firstColor == null)
                {
                    firstColor = storedColor;
                }
                else if (firstColor != storedColor)
                {
                    // Lines in group have different stored colors, don't use any
                    return null;
                }
            }

            return firstColor;
        }

        /// <summary>
        /// Tries to get a consistent stored description for all lines in a group
        /// </summary>
        private string TryGetGroupStoredDescription(LineLengthItem group)
        {
            if (group == null || group.ElementIds == null || group.ElementIds.Count == 0)
                return null;

            string firstDescription = null;

            foreach (ElementId id in group.ElementIds)
            {
                Element line = _doc.GetElement(id);
                if (line == null)
                    continue;

                string storedDescription = LineColorStorageService.GetStoredDescription(line);

                if (firstDescription == null)
                {
                    firstDescription = storedDescription;
                }
                else if (firstDescription != storedDescription)
                {
                    // Lines in group have different stored descriptions, don't use any
                    return null;
                }
            }

            return firstDescription;
        }

        private void UpdateStatistics()
        {
            _statistics.TotalLineGroups = _lineGroups.Count;
            _statistics.TotalLines = _lineGroups.Sum(g => g.LineCount);
            _statistics.TotalLength = _lineGroups.Sum(g => g.TotalLength);
            _statistics.TotalLengthFormatted = $"{_statistics.TotalLength:F2} m";
            _statistics.DetailLineCount = _lineGroups.Where(g => g.LineType == "Detail Line").Sum(g => g.LineCount);
            _statistics.ModelLineCount = _lineGroups.Where(g => g.LineType == "Model Line").Sum(g => g.LineCount);

            // Update UI
            TotalGroupsText.Text = _statistics.TotalLineGroups.ToString();
            TotalLinesText.Text = _statistics.TotalLines.ToString();
            TotalLengthText.Text = _statistics.TotalLengthFormatted;
            DetailLinesText.Text = _statistics.DetailLineCount.ToString();
            ModelLinesText.Text = _statistics.ModelLineCount.ToString();
        }

        private void ApplyColors_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Pass data to the handler and raise the external event
                _applyColorsHandler.SetData(_doc, _lineGroups.ToList());
                _applyColorsEvent.Raise();
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", $"Error applying colors:\n{ex.Message}");
            }
        }

        private void LineGroupsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ZoomButton.IsEnabled = LineGroupsDataGrid.SelectedItems.Count > 0;
        }

        private void ZoomToSelected_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (LineGroupsDataGrid.SelectedItems.Count == 0)
                    return;

                List<ElementId> selectedIds = new List<ElementId>();
                foreach (LineLengthItem item in LineGroupsDataGrid.SelectedItems)
                {
                    selectedIds.AddRange(item.ElementIds);
                }

                _uiDoc.Selection.SetElementIds(selectedIds);
                _uiDoc.ShowElements(selectedIds);

                TaskDialog.Show("Zoom Complete", $"Zoomed to {selectedIds.Count} line(s).");
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", $"Error zooming to elements:\n{ex.Message}");
            }
        }

        private void ExportToExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog saveDialog = new SaveFileDialog
                {
                    Filter = "Excel Files (*.xlsx)|*.xlsx|All Files (*.*)|*.*",
                    DefaultExt = "xlsx",
                    FileName = $"LineLengths_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    // For Excel export, we would need to reference an Excel library
                    // For now, export as CSV with .xlsx extension or use a library like EPPlus
                    ExportToCSVFile(saveDialog.FileName, true);

                    TaskDialog.Show("Export Successful",
                        $"Data exported to:\n{saveDialog.FileName}\n\n" +
                        "Note: File is saved in CSV format. You can open it with Excel.");
                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", $"Error exporting to Excel:\n{ex.Message}");
            }
        }

        private void ExportToCSV_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog saveDialog = new SaveFileDialog
                {
                    Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
                    DefaultExt = "csv",
                    FileName = $"LineLengths_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    ExportToCSVFile(saveDialog.FileName, false);
                    TaskDialog.Show("Export Successful", $"Data exported to:\n{saveDialog.FileName}");
                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", $"Error exporting to CSV:\n{ex.Message}");
            }
        }

        private void ExportToCSVFile(string filePath, bool isExcel)
        {
            StringBuilder csv = new StringBuilder();

            // Header
            csv.AppendLine("Group ID,Line Count,Total Length (m),Line Style,Line Type,View,Color,Element IDs");

            // Data rows
            foreach (var group in _lineGroups)
            {
                csv.AppendLine($"{group.GroupId},{group.LineCount},{group.TotalLength:F2}," +
                              $"\"{group.LineStyle}\",\"{group.LineType}\",\"{group.ViewName}\"," +
                              $"{group.ColorHex},\"{group.ElementIdsString}\"");
            }

            // Summary
            csv.AppendLine();
            csv.AppendLine("SUMMARY");
            csv.AppendLine($"Total Groups,{_statistics.TotalLineGroups}");
            csv.AppendLine($"Total Lines,{_statistics.TotalLines}");
            csv.AppendLine($"Total Length (m),{_statistics.TotalLength:F2}");
            csv.AppendLine($"Detail Lines,{_statistics.DetailLineCount}");
            csv.AppendLine($"Model Lines,{_statistics.ModelLineCount}");

            File.WriteAllText(filePath, csv.ToString(), Encoding.UTF8);
        }

        private void CopyToClipboard_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StringBuilder text = new StringBuilder();

                // Header
                text.AppendLine("Group ID\tLine Count\tTotal Length (m)\tLine Style\tLine Type\tView\tColor\tElement IDs");

                // Data rows
                foreach (var group in _lineGroups)
                {
                    text.AppendLine($"{group.GroupId}\t{group.LineCount}\t{group.TotalLength:F2}\t" +
                                  $"{group.LineStyle}\t{group.LineType}\t{group.ViewName}\t" +
                                  $"{group.ColorHex}\t{group.ElementIdsString}");
                }

                // Summary
                text.AppendLine();
                text.AppendLine("SUMMARY");
                text.AppendLine($"Total Groups\t{_statistics.TotalLineGroups}");
                text.AppendLine($"Total Lines\t{_statistics.TotalLines}");
                text.AppendLine($"Total Length (m)\t{_statistics.TotalLength:F2}");
                text.AppendLine($"Detail Lines\t{_statistics.DetailLineCount}");
                text.AppendLine($"Model Lines\t{_statistics.ModelLineCount}");

                Clipboard.SetText(text.ToString());

                TaskDialog.Show("Copied", "Data copied to clipboard.\nYou can paste it into Excel or any text editor.");
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", $"Error copying to clipboard:\n{ex.Message}");
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Close();
            }
            catch (Exception ex)
            {
                // Log error but don't show to user
                System.Diagnostics.Debug.WriteLine($"Error closing window: {ex.Message}");
            }
        }

        #region Preset Management

        private void LoadPresetsList()
        {
            var presetNames = LineLengthPresetService.GetPresetNames();
            PresetsComboBox.ItemsSource = presetNames;

            if (presetNames.Count > 0)
            {
                PresetsComboBox.SelectedIndex = 0;
            }
        }

        private void PresetsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadPresetButton.IsEnabled = PresetsComboBox.SelectedItem != null;
            DeletePresetButton.IsEnabled = PresetsComboBox.SelectedItem != null;
        }

        private void SavePreset_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Prompt for preset name using simple window
                var inputWindow = new Window
                {
                    Title = "Save Preset",
                    Width = 400,
                    Height = 150,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = this,
                    ResizeMode = ResizeMode.NoResize
                };

                var grid = new System.Windows.Controls.Grid();
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                var stackPanel = new StackPanel { Margin = new Thickness(20) };
                stackPanel.Children.Add(new TextBlock { Text = "Enter a name for this preset:", Margin = new Thickness(0, 0, 0, 10) });
                var textBox = new System.Windows.Controls.TextBox { Text = "My Preset", Padding = new Thickness(5) };
                stackPanel.Children.Add(textBox);
                System.Windows.Controls.Grid.SetRow(stackPanel, 0);
                grid.Children.Add(stackPanel);

                var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(20, 10, 20, 10) };
                var okButton = new System.Windows.Controls.Button { Content = "OK", Width = 75, Margin = new Thickness(5, 0, 5, 0), IsDefault = true };
                var cancelButton = new System.Windows.Controls.Button { Content = "Cancel", Width = 75, Margin = new Thickness(5, 0, 5, 0), IsCancel = true };
                okButton.Click += (s, args) => { inputWindow.DialogResult = true; inputWindow.Close(); };
                cancelButton.Click += (s, args) => { inputWindow.DialogResult = false; inputWindow.Close(); };
                buttonPanel.Children.Add(okButton);
                buttonPanel.Children.Add(cancelButton);
                System.Windows.Controls.Grid.SetRow(buttonPanel, 1);
                grid.Children.Add(buttonPanel);

                inputWindow.Content = grid;

                if (inputWindow.ShowDialog() != true || string.IsNullOrWhiteSpace(textBox.Text))
                    return;

                string presetName = textBox.Text.Trim();

                // Check if preset already exists
                if (LineLengthPresetService.PresetExists(presetName))
                {
                    var result = TaskDialog.Show("Overwrite Preset",
                        $"A preset named '{presetName}' already exists.\n\nDo you want to overwrite it?",
                        TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No);

                    if (result != TaskDialogResult.Yes)
                        return;
                }

                // Create preset from current data
                var preset = new LineLengthPreset
                {
                    Name = presetName,
                    CreatedDate = DateTime.Now
                };

                // Save line groups
                foreach (var group in _lineGroups)
                {
                    var presetGroup = new PresetLineGroup
                    {
#if REVIT2025
                        ElementIds = group.ElementIds.Select(id => (int)id.Value).ToList(),
#else
                        ElementIds = group.ElementIds.Select(id => id.IntegerValue).ToList(),
#endif
                        Description = group.Description ?? "",
                        ColorHex = group.ColorHex
                    };
                    preset.LineGroups.Add(presetGroup);
                }

                // Save selected element IDs
#if REVIT2025
                preset.SelectedElementIds = _selectedElementIds.Select(id => (int)id.Value).ToList();
#else
                preset.SelectedElementIds = _selectedElementIds.Select(id => id.IntegerValue).ToList();
#endif

                LineLengthPresetService.SavePreset(preset);
                LoadPresetsList();
                SavePresetButton.IsEnabled = true;

                TaskDialog.Show("Success", $"Preset '{presetName}' saved successfully!");
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", $"Error saving preset:\n{ex.Message}");
            }
        }

        private void LoadPreset_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string presetName = PresetsComboBox.SelectedItem as string;
                if (string.IsNullOrEmpty(presetName))
                    return;

                var preset = LineLengthPresetService.LoadPreset(presetName);
                if (preset == null)
                {
                    TaskDialog.Show("Error", $"Preset '{presetName}' not found.");
                    return;
                }

                // Clear current data
                _lineGroups.Clear();
                _selectedElementIds.Clear();

                // Load selected element IDs
                foreach (int idValue in preset.SelectedElementIds)
                {
#if REVIT2025
                    _selectedElementIds.Add(new ElementId(idValue));
#else
                    _selectedElementIds.Add(new ElementId(idValue));
#endif
                }

                // Load line groups
                int groupId = 1;
                foreach (var presetGroup in preset.LineGroups)
                {
                    var group = new LineLengthItem
                    {
                        GroupId = groupId++,
                        Description = presetGroup.Description,
                        ColorHex = presetGroup.ColorHex
                    };

                    foreach (int idValue in presetGroup.ElementIds)
                    {
#if REVIT2025
                        group.ElementIds.Add(new ElementId(idValue));
#else
                        group.ElementIds.Add(new ElementId(idValue));
#endif
                    }

                    // Recalculate properties for this group
                    RecalculateGroupProperties(group);
                    _lineGroups.Add(group);
                }

                UpdateStatistics();
                UseSelectionCheckBox.IsChecked = _selectedElementIds.Count > 0;
                ExportExcelButton.IsEnabled = true;
                ExportCSVButton.IsEnabled = true;
                CopyButton.IsEnabled = true;
                ApplyColorsButton.IsEnabled = true;
                SavePresetButton.IsEnabled = true;

                TaskDialog.Show("Success", $"Preset '{presetName}' loaded successfully!");
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", $"Error loading preset:\n{ex.Message}");
            }
        }

        private void DeletePreset_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string presetName = PresetsComboBox.SelectedItem as string;
                if (string.IsNullOrEmpty(presetName))
                    return;

                var result = TaskDialog.Show("Delete Preset",
                    $"Are you sure you want to delete the preset '{presetName}'?",
                    TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No);

                if (result != TaskDialogResult.Yes)
                    return;

                LineLengthPresetService.DeletePreset(presetName);
                LoadPresetsList();

                TaskDialog.Show("Success", $"Preset '{presetName}' deleted successfully!");
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", $"Error deleting preset:\n{ex.Message}");
            }
        }

        private void RestoreSelection_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var lastSelection = LineLengthPresetService.LoadLastSelection();
                if (lastSelection == null || lastSelection.Count == 0)
                {
                    TaskDialog.Show("No Selection", "No previous selection found to restore.");
                    return;
                }

                _selectedElementIds.Clear();
                foreach (int idValue in lastSelection)
                {
#if REVIT2025
                    _selectedElementIds.Add(new ElementId(idValue));
#else
                    _selectedElementIds.Add(new ElementId(idValue));
#endif
                }

                UseSelectionCheckBox.IsChecked = true;

                TaskDialog.Show("Selection Restored",
                    $"Restored {_selectedElementIds.Count} previously selected line(s).");
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", $"Error restoring selection:\n{ex.Message}");
            }
        }

        private void RecalculateGroupProperties(LineLengthItem group)
        {
            // Recalculate total length and other properties
            double totalLength = 0;
            group.LineCount = group.ElementIds.Count;

            Element firstLine = null;
            foreach (ElementId id in group.ElementIds)
            {
                Element line = _doc.GetElement(id);
                if (line != null)
                {
                    if (firstLine == null)
                        firstLine = line;

                    Curve curve = GetCurve(line);
                    if (curve != null)
                    {
#if REVIT2025
                        totalLength += UnitUtils.ConvertFromInternalUnits(curve.Length, UnitTypeId.Meters);
#else
                        totalLength += UnitUtils.ConvertFromInternalUnits(curve.Length, DisplayUnitType.DUT_METERS);
#endif
                    }
                }
            }

            group.TotalLength = totalLength;
            group.TotalLengthFormatted = $"{totalLength:F2} m";

            if (firstLine != null)
            {
                GraphicsStyle gs = GetGraphicsStyle(firstLine);
                group.LineStyle = gs != null ? gs.Name : "Unknown";

                if (firstLine is DetailLine || firstLine is DetailCurve)
                {
                    group.LineType = "Detail Line";
#if REVIT2025
                    if (firstLine.OwnerViewId != ElementId.InvalidElementId)
#else
                    if (firstLine.OwnerViewId != ElementId.InvalidElementId)
#endif
                    {
                        View view = _doc.GetElement(firstLine.OwnerViewId) as View;
                        group.ViewName = view != null ? view.Name : "Unknown";
                    }
                }
                else if (firstLine is ModelLine || firstLine is ModelCurve)
                {
                    group.LineType = "Model Line";
                    group.ViewName = "N/A (Model)";
                }
            }

            // Create element IDs string
#if REVIT2025
            group.ElementIdsString = string.Join(", ", group.ElementIds.Select(id => id.Value.ToString()));
#else
            group.ElementIdsString = string.Join(", ", group.ElementIds.Select(id => id.IntegerValue.ToString()));
#endif
        }

        #endregion
    }

    /// <summary>
    /// Selection filter for Detail Lines and Model Lines
    /// </summary>
    public class LineSelectionFilter : Autodesk.Revit.UI.Selection.ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            return elem is DetailLine || elem is DetailCurve || elem is ModelLine || elem is ModelCurve;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }

    /// <summary>
    /// External Event Handler for applying colors to line groups
    /// </summary>
    public class ApplyColorsHandler : IExternalEventHandler
    {
        private Document _doc;
        private List<LineLengthItem> _lineGroups;

        public void SetData(Document doc, List<LineLengthItem> lineGroups)
        {
            _doc = doc;
            _lineGroups = lineGroups;
        }

        public void Execute(UIApplication app)
        {
            try
            {
                if (_doc == null || _lineGroups == null || _lineGroups.Count == 0)
                {
                    TaskDialog.Show("Error", "No data available to apply colors.");
                    return;
                }

                using (Transaction trans = new Transaction(_doc, "Apply Line Colors"))
                {
                    trans.Start();

                    foreach (var group in _lineGroups)
                    {
                        // Parse color
                        System.Windows.Media.Color wpfColor = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(group.ColorHex);
                        Autodesk.Revit.DB.Color revitColor = new Autodesk.Revit.DB.Color(wpfColor.R, wpfColor.G, wpfColor.B);

                        // Apply color override to each line in the group
                        foreach (ElementId id in group.ElementIds)
                        {
                            Element line = _doc.GetElement(id);
                            if (line != null)
                            {
                                View ownerView = null;

                                // For detail lines, use their owner view
#if REVIT2025
                                if (line.OwnerViewId != ElementId.InvalidElementId)
#else
                                if (line.OwnerViewId != ElementId.InvalidElementId)
#endif
                                {
                                    ownerView = _doc.GetElement(line.OwnerViewId) as View;
                                }
                                else
                                {
                                    ownerView = _doc.ActiveView;
                                }

                                if (ownerView != null)
                                {
                                    OverrideGraphicSettings ogs = new OverrideGraphicSettings();
                                    ogs.SetProjectionLineColor(revitColor);
                                    ogs.SetProjectionLineWeight(3); // Make lines slightly thicker
                                    ownerView.SetElementOverrides(id, ogs);
                                }

                                // Store the color and description in extensible storage for persistence
                                LineColorStorageService.StoreColorAndDescription(line, group.ColorHex, group.Description);
                            }
                        }
                    }

                    trans.Commit();
                }

                TaskDialog.Show("Success",
                    "Colors applied to line groups successfully!\n\n" +
                    "The color assignments have been saved and will be restored when you reopen this tool.");
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", $"Error applying colors:\n{ex.Message}");
            }
        }

        public string GetName()
        {
            return "Apply Line Colors";
        }
    }
}
