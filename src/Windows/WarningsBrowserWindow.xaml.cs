using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BIMKraft.Models;
using Microsoft.Win32;

namespace BIMKraft.Windows
{
    public partial class WarningsBrowserWindow : Window
    {
        private readonly Document _doc;
        private readonly UIDocument _uiDoc;
        private ObservableCollection<WarningItem> _warningItems;
        private ObservableCollection<WarningItem> _filteredWarningItems;
        private List<ElementId> _highlightedElements;
        private List<ElementId> _savedHighlightedElements;
        private OverrideGraphicSettings _highlightOverride;
        private List<ElementId> _created3DViews;
        private Dictionary<ViewType, List<View>> _cachedViews;
        private WarningStatistics _statistics;

        public WarningsBrowserWindow(Document doc, UIDocument uiDoc)
        {
            // Initialize fields BEFORE InitializeComponent to avoid null reference errors
            // when XAML event handlers are triggered during initialization
            _doc = doc;
            _uiDoc = uiDoc;
            _highlightedElements = new List<ElementId>();
            _savedHighlightedElements = new List<ElementId>();
            _created3DViews = new List<ElementId>();
            _cachedViews = new Dictionary<ViewType, List<View>>();

            InitializeComponent();

            InitializeHighlightSettings();
            CacheViews();
            LoadWarnings();
            UpdateStatistics();
            PopulateCategoryFilter();
        }

        private void InitializeHighlightSettings()
        {
            _highlightOverride = new OverrideGraphicSettings();

            // Set red color for highlighting
            var redColor = new Color(255, 0, 0);
            _highlightOverride.SetProjectionLineColor(redColor);
            _highlightOverride.SetCutLineColor(redColor);

            // Try to set solid fill pattern
            try
            {
                var solidPattern = new FilteredElementCollector(_doc)
                    .OfClass(typeof(FillPatternElement))
                    .Cast<FillPatternElement>()
                    .FirstOrDefault(fp => fp.GetFillPattern().IsSolidFill);

                if (solidPattern != null)
                {
                    _highlightOverride.SetSurfaceForegroundPatternId(solidPattern.Id);
                    _highlightOverride.SetSurfaceForegroundPatternColor(redColor);
                    _highlightOverride.SetCutForegroundPatternId(solidPattern.Id);
                    _highlightOverride.SetCutForegroundPatternColor(redColor);
                }
            }
            catch { }

            _highlightOverride.SetSurfaceTransparency(30);
        }

        private void CacheViews()
        {
            try
            {
                var allViews = new FilteredElementCollector(_doc)
                    .OfClass(typeof(View))
                    .Cast<View>()
                    .Where(v => !v.IsTemplate)
                    .ToList();

                _cachedViews[ViewType.FloorPlan] = allViews.Where(v => v.ViewType == ViewType.FloorPlan).ToList();
                _cachedViews[ViewType.ThreeD] = allViews.Where(v => v.ViewType == ViewType.ThreeD).ToList();
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Warning", $"Error caching views: {ex.Message}");
            }
        }

        private void LoadWarnings()
        {
            _warningItems = new ObservableCollection<WarningItem>();
            var warnings = _doc.GetWarnings();

            // Group warnings by message
            var warningGroups = warnings
                .Where(w => w.GetFailingElements().Count > 0)
                .GroupBy(w => w.GetDescriptionText())
                .ToList();

            foreach (var group in warningGroups)
            {
                var allElementIds = new List<ElementId>();
                var allElementInfo = new List<ElementInfo>();

                foreach (var warning in group)
                {
                    var elemIds = warning.GetFailingElements().ToList();
                    allElementIds.AddRange(elemIds);

                    foreach (var elemId in elemIds)
                    {
                        allElementInfo.Add(GetElementInfo(elemId));
                    }
                }

                var warningItem = new WarningItem
                {
                    Message = group.Key,
                    ElementCount = allElementIds.Count,
                    OccurrenceCount = group.Count(),
                    ElementIdList = allElementIds,
#if REVIT2025
                    ElementIds = string.Join("; ", allElementIds.Select(id => id.Value)),
#else
                    ElementIds = string.Join("; ", allElementIds.Select(id => id.IntegerValue)),
#endif
                    ElementNames = string.Join("; ", allElementInfo.Select(info => info.Name).Distinct().Take(5)) +
                                  (allElementInfo.Select(info => info.Name).Distinct().Count() > 5 ? "..." : ""),
                    Levels = string.Join("; ", allElementInfo.Select(info => info.Level).Distinct().Take(3)) +
                            (allElementInfo.Select(info => info.Level).Distinct().Count() > 3 ? "..." : ""),
                    Views = string.Join("; ", allElementInfo.SelectMany(info => info.Views).Distinct().Take(3)) +
                           (allElementInfo.SelectMany(info => info.Views).Distinct().Count() > 3 ? "..." : ""),
                    Categories = string.Join("; ", allElementInfo.Select(info => info.Category).Distinct().Take(3)) +
                                (allElementInfo.Select(info => info.Category).Distinct().Count() > 3 ? "..." : ""),
                    IsGroup = true
                };

                // Create child items for each occurrence if more than one
                if (group.Count() > 1)
                {
                    int occurrenceNum = 1;
                    foreach (var warning in group)
                    {
                        var elemIds = warning.GetFailingElements().ToList();
                        var childInfo = new List<ElementInfo>();

                        foreach (var elemId in elemIds)
                        {
                            childInfo.Add(GetElementInfo(elemId));
                        }

                        var childItem = new WarningItem
                        {
                            Message = $"  → Occurrence {occurrenceNum}",
                            ElementCount = elemIds.Count,
                            OccurrenceCount = 0,
                            ElementIdList = elemIds,
#if REVIT2025
                            ElementIds = string.Join("; ", elemIds.Select(id => id.Value)),
#else
                            ElementIds = string.Join("; ", elemIds.Select(id => id.IntegerValue)),
#endif
                            ElementNames = string.Join("; ", childInfo.Select(info => info.Name)),
                            Levels = string.Join("; ", childInfo.Select(info => info.Level).Distinct()),
                            Views = string.Join("; ", childInfo.SelectMany(info => info.Views).Distinct()),
                            Categories = string.Join("; ", childInfo.Select(info => info.Category).Distinct()),
                            IsGroup = false,
                            Parent = warningItem
                        };

                        warningItem.Children.Add(childItem);
                        occurrenceNum++;
                    }
                }

                _warningItems.Add(warningItem);
            }

            _filteredWarningItems = new ObservableCollection<WarningItem>(_warningItems);
            WarningsDataGrid.ItemsSource = _filteredWarningItems;
        }

        private ElementInfo GetElementInfo(ElementId elementId)
        {
            var info = new ElementInfo();

            try
            {
                var element = _doc.GetElement(elementId);
                if (element == null)
                    return info;

                // Element name
#if REVIT2025
                info.Name = element.Name ?? $"ID: {elementId.Value}";
#else
                info.Name = element.Name ?? $"ID: {elementId.IntegerValue}";
#endif

                // Level
                if (element.LevelId != null && element.LevelId != ElementId.InvalidElementId)
                {
                    var level = _doc.GetElement(element.LevelId);
                    info.Level = level?.Name ?? "N/A";
                }

                // Category
                info.Category = element.Category?.Name ?? "N/A";

                // Find a suitable view
                var viewName = GetElementView(element);
                if (!string.IsNullOrEmpty(viewName))
                {
                    info.Views.Add(viewName);
                }
            }
            catch { }

            return info;
        }

        private string GetElementView(Element element)
        {
            try
            {
                // Try floor plan views first
                if (_cachedViews.ContainsKey(ViewType.FloorPlan))
                {
                    foreach (var view in _cachedViews[ViewType.FloorPlan])
                    {
                        try
                        {
                            if (!element.IsHidden(view) &&
                                (element.Category == null || !view.GetCategoryHidden(element.Category.Id)))
                            {
                                return view.Name;
                            }
                        }
                        catch { }
                    }
                }

                // Try 3D views
                if (_cachedViews.ContainsKey(ViewType.ThreeD))
                {
                    foreach (var view in _cachedViews[ViewType.ThreeD])
                    {
                        try
                        {
                            if (!element.IsHidden(view) &&
                                (element.Category == null || !view.GetCategoryHidden(element.Category.Id)))
                            {
                                return view.Name;
                            }
                        }
                        catch { }
                    }
                }
            }
            catch { }

            return null;
        }

        private void UpdateStatistics()
        {
            _statistics = new WarningStatistics
            {
                TotalWarnings = _warningItems.Sum(w => w.OccurrenceCount),
                UniqueWarningTypes = _warningItems.Count,
                TotalElements = _warningItems.Sum(w => w.ElementCount)
            };

            TotalWarningsText.Text = _statistics.TotalWarnings.ToString();
            UniqueTypesText.Text = _statistics.UniqueWarningTypes.ToString();
            AffectedElementsText.Text = _statistics.TotalElements.ToString();
        }

        private void PopulateCategoryFilter()
        {
            // Add "All Categories" as the first item
            CategoryFilterComboBox.Items.Add(new ComboBoxItem { Content = "All Categories" });

            var categories = _warningItems
                .SelectMany(w => w.Categories.Split(new[] { "; " }, StringSplitOptions.RemoveEmptyEntries))
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            foreach (var category in categories)
            {
                CategoryFilterComboBox.Items.Add(new ComboBoxItem { Content = category });
            }

            // Set the default selection to "All Categories"
            CategoryFilterComboBox.SelectedIndex = 0;
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void CategoryFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            // Ignore if called during initialization
            if (_warningItems == null || _filteredWarningItems == null)
                return;

            var searchText = SearchTextBox.Text?.ToLower() ?? "";
            var selectedCategory = (CategoryFilterComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();

            var filtered = _warningItems.Where(w =>
            {
                bool matchesSearch = string.IsNullOrEmpty(searchText) ||
                    w.Message.ToLower().Contains(searchText) ||
                    w.ElementNames.ToLower().Contains(searchText) ||
                    w.ElementIds.ToLower().Contains(searchText);

                bool matchesCategory = selectedCategory == null ||
                    selectedCategory == "All Categories" ||
                    w.Categories.Contains(selectedCategory);

                return matchesSearch && matchesCategory;
            }).ToList();

            _filteredWarningItems.Clear();
            foreach (var item in filtered)
            {
                _filteredWarningItems.Add(item);
            }
        }

        private void ClearFilters_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = "";
            CategoryFilterComboBox.SelectedIndex = 0;
        }

        private void GroupWarnings_Changed(object sender, RoutedEventArgs e)
        {
            // Ignore if called during initialization (before LoadWarnings has been called)
            if (_warningItems == null)
                return;

            LoadWarnings();
            UpdateStatistics();
        }

        private void WarningsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedCountText.Text = WarningsDataGrid.SelectedItems.Count.ToString();

            if (AutoHighlightCheckBox.IsChecked == true)
            {
                HighlightButton_Click(sender, e);
            }
        }

        private void WarningsDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (WarningsDataGrid.SelectedItem is WarningItem item && item.IsGroup && item.Children.Count > 0)
            {
                item.IsExpanded = !item.IsExpanded;

                var allItems = _filteredWarningItems.ToList();
                var itemIndex = allItems.IndexOf(item);

                if (item.IsExpanded)
                {
                    // Insert children
                    for (int i = 0; i < item.Children.Count; i++)
                    {
                        _filteredWarningItems.Insert(itemIndex + 1 + i, item.Children[i]);
                    }
                }
                else
                {
                    // Remove children
                    foreach (var child in item.Children)
                    {
                        _filteredWarningItems.Remove(child);
                    }
                }
            }
        }

        private void HighlightButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = WarningsDataGrid.SelectedItems.Cast<WarningItem>().ToList();
            if (selectedItems.Count == 0)
            {
                MessageBox.Show("Please select one or more warnings from the list.",
                    "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var allElementIds = selectedItems.SelectMany(item => item.ElementIdList).ToList();

            // Clear previous temporary highlights
            ClearTemporaryHighlights();

            using (Transaction trans = new Transaction(_doc, "Highlight Warning Elements"))
            {
                trans.Start();

                try
                {
                    var activeView = _uiDoc.ActiveView;
                    var newlyHighlighted = new List<ElementId>();

                    foreach (var elemId in allElementIds)
                    {
                        try
                        {
                            var element = _doc.GetElement(elemId);
                            if (element != null && !element.IsHidden(activeView))
                            {
                                activeView.SetElementOverrides(elemId, _highlightOverride);
                                newlyHighlighted.Add(elemId);
                            }
                        }
                        catch { }
                    }

                    _highlightedElements.AddRange(newlyHighlighted);

                    if (SaveHighlightsCheckBox.IsChecked == true)
                    {
                        foreach (var elemId in newlyHighlighted)
                        {
                            if (!_savedHighlightedElements.Contains(elemId))
                            {
                                _savedHighlightedElements.Add(elemId);
                            }
                        }
                    }

                    trans.Commit();
                }
                catch (Exception ex)
                {
                    trans.RollBack();
                    MessageBox.Show($"Error highlighting elements: {ex.Message}",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ClearHighlightsButton_Click(object sender, RoutedEventArgs e)
        {
            ClearTemporaryHighlights();
        }

        private void ClearAllHighlightsButton_Click(object sender, RoutedEventArgs e)
        {
            ClearAllHighlights();
        }

        private void ClearTemporaryHighlights()
        {
            var elementsToClear = _highlightedElements
                .Where(id => !_savedHighlightedElements.Contains(id))
                .ToList();

            if (elementsToClear.Count == 0)
                return;

            using (Transaction trans = new Transaction(_doc, "Clear Temporary Highlights"))
            {
                trans.Start();

                try
                {
                    var activeView = _uiDoc.ActiveView;
                    foreach (var elemId in elementsToClear)
                    {
                        try
                        {
                            activeView.SetElementOverrides(elemId, new OverrideGraphicSettings());
                        }
                        catch { }
                    }

                    _highlightedElements = _highlightedElements
                        .Where(id => _savedHighlightedElements.Contains(id))
                        .ToList();

                    trans.Commit();
                }
                catch (Exception ex)
                {
                    trans.RollBack();
                }
            }
        }

        private void ClearAllHighlights()
        {
            var allToClear = _highlightedElements.Union(_savedHighlightedElements).Distinct().ToList();

            if (allToClear.Count == 0)
                return;

            using (Transaction trans = new Transaction(_doc, "Clear All Highlights"))
            {
                trans.Start();

                try
                {
                    var activeView = _uiDoc.ActiveView;
                    foreach (var elemId in allToClear)
                    {
                        try
                        {
                            activeView.SetElementOverrides(elemId, new OverrideGraphicSettings());
                        }
                        catch { }
                    }

                    _highlightedElements.Clear();
                    _savedHighlightedElements.Clear();

                    trans.Commit();
                }
                catch (Exception ex)
                {
                    trans.RollBack();
                }
            }
        }

        private void ZoomCurrentViewButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = WarningsDataGrid.SelectedItems.Cast<WarningItem>().ToList();
            if (selectedItems.Count == 0)
            {
                MessageBox.Show("Please select one or more warnings from the list.",
                    "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var allElementIds = selectedItems.SelectMany(item => item.ElementIdList).ToList();
            var activeView = _uiDoc.ActiveView;

            var visibleIds = allElementIds.Where(id =>
            {
                try
                {
                    var element = _doc.GetElement(id);
                    return element != null && !element.IsHidden(activeView) &&
                           (element.Category == null || !activeView.GetCategoryHidden(element.Category.Id));
                }
                catch { return false; }
            }).ToList();

            if (visibleIds.Count == 0)
            {
                MessageBox.Show("No elements are visible in the current view.\n\nTip: Use 'Zoom (All Views)' to switch to a view where elements are visible.",
                    "No Visible Elements", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            _uiDoc.Selection.SetElementIds(visibleIds);
            _uiDoc.ShowElements(visibleIds);
        }

        private void ZoomAllViewsButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = WarningsDataGrid.SelectedItems.Cast<WarningItem>().ToList();
            if (selectedItems.Count == 0)
            {
                MessageBox.Show("Please select one or more warnings from the list.",
                    "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var allElementIds = selectedItems.SelectMany(item => item.ElementIdList).ToList();
            var bestView = FindBestViewForElements(allElementIds);

            if (bestView != null && bestView.Id != _uiDoc.ActiveView.Id)
            {
                _uiDoc.ActiveView = bestView;
            }

            _uiDoc.Selection.SetElementIds(allElementIds);
            _uiDoc.ShowElements(allElementIds);
        }

        private View FindBestViewForElements(List<ElementId> elementIds)
        {
            if (elementIds.Count == 0)
                return null;

            try
            {
                var firstElement = _doc.GetElement(elementIds[0]);
                if (firstElement == null)
                    return null;

                // Try to match floor plan by level
                if (firstElement.LevelId != null && firstElement.LevelId != ElementId.InvalidElementId)
                {
                    var level = _doc.GetElement(firstElement.LevelId);
                    if (level != null && _cachedViews.ContainsKey(ViewType.FloorPlan))
                    {
                        var matchingView = _cachedViews[ViewType.FloorPlan]
                            .FirstOrDefault(v => v.Name.Contains(level.Name));

                        if (matchingView != null)
                            return matchingView;
                    }
                }

                // Check if visible in current view
                var activeView = _uiDoc.ActiveView;
                if (!firstElement.IsHidden(activeView))
                    return activeView;

                // Fallback to first floor plan
                if (_cachedViews.ContainsKey(ViewType.FloorPlan) && _cachedViews[ViewType.FloorPlan].Count > 0)
                    return _cachedViews[ViewType.FloorPlan][0];

                // Last resort: 3D view
                if (_cachedViews.ContainsKey(ViewType.ThreeD) && _cachedViews[ViewType.ThreeD].Count > 0)
                    return _cachedViews[ViewType.ThreeD][0];
            }
            catch { }

            return null;
        }

        private void ShowIn3DButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = WarningsDataGrid.SelectedItems.Cast<WarningItem>().ToList();
            if (selectedItems.Count == 0)
            {
                MessageBox.Show("Please select one or more warnings from the list.",
                    "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var allElementIds = selectedItems.SelectMany(item => item.ElementIdList).ToList();

            // Calculate bounding box
            double minX = double.MaxValue, minY = double.MaxValue, minZ = double.MaxValue;
            double maxX = double.MinValue, maxY = double.MinValue, maxZ = double.MinValue;
            int validElements = 0;

            foreach (var elemId in allElementIds)
            {
                try
                {
                    var element = _doc.GetElement(elemId);
                    var bbox = element?.get_BoundingBox(null);

                    if (bbox != null)
                    {
                        minX = Math.Min(minX, bbox.Min.X);
                        minY = Math.Min(minY, bbox.Min.Y);
                        minZ = Math.Min(minZ, bbox.Min.Z);
                        maxX = Math.Max(maxX, bbox.Max.X);
                        maxY = Math.Max(maxY, bbox.Max.Y);
                        maxZ = Math.Max(maxZ, bbox.Max.Z);
                        validElements++;
                    }
                }
                catch { }
            }

            if (validElements == 0)
            {
                MessageBox.Show("Could not calculate bounding box for selected elements.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Add padding
            double paddingX = (maxX - minX) * 0.1;
            double paddingY = (maxY - minY) * 0.1;
            double paddingZ = (maxZ - minZ) * 0.1;

            minX -= paddingX; minY -= paddingY; minZ -= paddingZ;
            maxX += paddingX; maxY += paddingY; maxZ += paddingZ;

            using (Transaction trans = new Transaction(_doc, "Create 3D View for Warnings"))
            {
                trans.Start();

                try
                {
                    var viewFamilyType = new FilteredElementCollector(_doc)
                        .OfClass(typeof(ViewFamilyType))
                        .Cast<ViewFamilyType>()
                        .FirstOrDefault(vft => vft.ViewFamily == ViewFamily.ThreeDimensional);

                    if (viewFamilyType == null)
                    {
                        MessageBox.Show("Could not find 3D view type.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        trans.RollBack();
                        return;
                    }

                    var newView = View3D.CreateIsometric(_doc, viewFamilyType.Id);
                    newView.Name = $"Warnings_3D_{DateTime.Now:yyyyMMdd_HHmmss}";

                    var cropBox = new BoundingBoxXYZ
                    {
                        Min = new XYZ(minX, minY, minZ),
                        Max = new XYZ(maxX, maxY, maxZ)
                    };

                    newView.SetSectionBox(cropBox);
                    newView.CropBoxActive = true;
                    newView.CropBoxVisible = true;

                    // Apply highlights to all elements
                    var allHighlights = allElementIds.Union(_savedHighlightedElements).Distinct();
                    foreach (var elemId in allHighlights)
                    {
                        try
                        {
                            newView.SetElementOverrides(elemId, _highlightOverride);
                        }
                        catch { }
                    }

                    trans.Commit();

                    _created3DViews.Add(newView.Id);
                    _uiDoc.ActiveView = newView;
                    _uiDoc.Selection.SetElementIds(allElementIds);

                    MessageBox.Show($"3D view '{newView.Name}' created successfully.\n\nHighlights will persist in this view.",
                        "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    trans.RollBack();
                    MessageBox.Show($"Error creating 3D view: {ex.Message}",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExportHtmlButton_Click(object sender, RoutedEventArgs e)
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = "HTML files (*.html)|*.html",
                DefaultExt = "html",
                FileName = $"BIMKraft_Warnings_{_doc.Title.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd_HHmmss}.html"
            };

            if (saveDialog.ShowDialog() == true)
            {
                try
                {
                    ExportToHtml(saveDialog.FileName);
                    MessageBox.Show($"Export successful!\n\n{saveDialog.FileName}",
                        "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    System.Diagnostics.Process.Start(saveDialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Export failed: {ex.Message}",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExportToHtml(string filePath)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine("    <meta charset=\"UTF-8\">");
            sb.AppendLine($"    <title>BIM Kraft Warnings Report - {_doc.Title}</title>");
            sb.AppendLine("    <style>");
            sb.AppendLine("        body { font-family: Arial, sans-serif; margin: 20px; background: #f5f5f5; }");
            sb.AppendLine("        .container { max-width: 1400px; margin: 0 auto; background: white; padding: 30px; box-shadow: 0 0 10px rgba(0,0,0,0.1); }");
            sb.AppendLine("        h1 { color: #2C5AA0; text-align: center; }");
            sb.AppendLine("        .header { text-align: center; margin-bottom: 30px; }");
            sb.AppendLine("        .stats { display: flex; justify-content: space-around; margin: 20px 0; }");
            sb.AppendLine("        .stat-card { background: #f9f9f9; padding: 20px; border-radius: 8px; text-align: center; flex: 1; margin: 0 10px; }");
            sb.AppendLine("        .stat-value { font-size: 32px; font-weight: bold; color: #DC3545; }");
            sb.AppendLine("        .stat-label { color: #666; margin-top: 5px; }");
            sb.AppendLine("        table { border-collapse: collapse; width: 100%; margin-top: 20px; }");
            sb.AppendLine("        th, td { border: 1px solid #ddd; padding: 12px; text-align: left; }");
            sb.AppendLine("        th { background-color: #2C5AA0; color: white; font-weight: bold; }");
            sb.AppendLine("        tr:nth-child(even) { background-color: #f9f9f9; }");
            sb.AppendLine("        tr:hover { background-color: #e8f4f8; }");
            sb.AppendLine("        .footer { margin-top: 30px; text-align: center; color: #666; font-size: 12px; }");
            sb.AppendLine("    </style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("    <div class=\"container\">");
            sb.AppendLine("        <div class=\"header\">");
            sb.AppendLine("            <h1>BIM Kraft Warnings Report</h1>");
            sb.AppendLine($"            <p><strong>Project:</strong> {_doc.Title}</p>");
            sb.AppendLine($"            <p><strong>Generated:</strong> {DateTime.Now:dd.MM.yyyy HH:mm:ss}</p>");
            sb.AppendLine("        </div>");
            sb.AppendLine("        <div class=\"stats\">");
            sb.AppendLine($"            <div class=\"stat-card\"><div class=\"stat-value\">{_statistics.TotalWarnings}</div><div class=\"stat-label\">Total Warnings</div></div>");
            sb.AppendLine($"            <div class=\"stat-card\"><div class=\"stat-value\">{_statistics.UniqueWarningTypes}</div><div class=\"stat-label\">Unique Types</div></div>");
            sb.AppendLine($"            <div class=\"stat-card\"><div class=\"stat-value\">{_statistics.TotalElements}</div><div class=\"stat-label\">Affected Elements</div></div>");
            sb.AppendLine("        </div>");
            sb.AppendLine("        <table>");
            sb.AppendLine("            <thead>");
            sb.AppendLine("                <tr>");
            sb.AppendLine("                    <th>#</th>");
            sb.AppendLine("                    <th>Warning Message</th>");
            sb.AppendLine("                    <th>Occurrences</th>");
            sb.AppendLine("                    <th>Elements</th>");
            sb.AppendLine("                    <th>Element IDs</th>");
            sb.AppendLine("                    <th>Element Names</th>");
            sb.AppendLine("                    <th>Levels</th>");
            sb.AppendLine("                    <th>Categories</th>");
            sb.AppendLine("                </tr>");
            sb.AppendLine("            </thead>");
            sb.AppendLine("            <tbody>");

            int rowNum = 1;
            foreach (var item in _warningItems)
            {
                sb.AppendLine("                <tr>");
                sb.AppendLine($"                    <td>{rowNum}</td>");
                sb.AppendLine($"                    <td>{System.Web.HttpUtility.HtmlEncode(item.Message)}</td>");
                sb.AppendLine($"                    <td>{item.OccurrenceCount}</td>");
                sb.AppendLine($"                    <td>{item.ElementCount}</td>");
                sb.AppendLine($"                    <td>{System.Web.HttpUtility.HtmlEncode(item.ElementIds)}</td>");
                sb.AppendLine($"                    <td>{System.Web.HttpUtility.HtmlEncode(item.ElementNames)}</td>");
                sb.AppendLine($"                    <td>{System.Web.HttpUtility.HtmlEncode(item.Levels)}</td>");
                sb.AppendLine($"                    <td>{System.Web.HttpUtility.HtmlEncode(item.Categories)}</td>");
                sb.AppendLine("                </tr>");
                rowNum++;
            }

            sb.AppendLine("            </tbody>");
            sb.AppendLine("        </table>");
            sb.AppendLine("        <div class=\"footer\">");
            sb.AppendLine("            <p>Generated by BIM Kraft Warnings Browser Pro</p>");
            sb.AppendLine("        </div>");
            sb.AppendLine("    </div>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }

        private void ExportExcelButton_Click(object sender, RoutedEventArgs e)
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv",
                DefaultExt = "csv",
                FileName = $"BIMKraft_Warnings_{_doc.Title.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            };

            if (saveDialog.ShowDialog() == true)
            {
                try
                {
                    ExportToCsv(saveDialog.FileName);
                    MessageBox.Show($"Export successful!\n\n{saveDialog.FileName}\n\nOpen with Excel or any spreadsheet application.",
                        "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Export failed: {ex.Message}",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExportToCsv(string filePath)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Warning Message,Occurrences,Elements,Element IDs,Element Names,Levels,Views,Categories");

            foreach (var item in _warningItems)
            {
                sb.AppendLine($"\"{EscapeCsv(item.Message)}\",{item.OccurrenceCount},{item.ElementCount}," +
                             $"\"{EscapeCsv(item.ElementIds)}\",\"{EscapeCsv(item.ElementNames)}\"," +
                             $"\"{EscapeCsv(item.Levels)}\",\"{EscapeCsv(item.Views)}\",\"{EscapeCsv(item.Categories)}\"");
            }

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }

        private string EscapeCsv(string value)
        {
            return value?.Replace("\"", "\"\"") ?? "";
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Only clear highlights if not in a created 3D view
            if (!_created3DViews.Contains(_uiDoc.ActiveView.Id))
            {
                ClearAllHighlights();
            }
        }
    }
}
