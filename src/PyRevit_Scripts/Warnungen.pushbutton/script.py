# -*- coding: utf-8 -*-
"""
Enhanced Warnings Browser for Revit
A modal dialog for browsing warnings with enhanced information and element highlighting
"""

import clr
clr.AddReference('PresentationCore')
clr.AddReference('PresentationFramework')
clr.AddReference('WindowsBase')

import System
from System.Windows import Window, Application, RoutedEventHandler
from System.Windows.Input import MouseButtonEventHandler
from System.Windows.Controls import (
    Grid, DataGrid, DataGridTextColumn, Button, StackPanel,
    CheckBox, TextBlock, ScrollViewer, DataGridLength
)
from System.Windows.Data import Binding
from System.Windows.Media import SolidColorBrush, Colors
from System.Collections.ObjectModel import ObservableCollection
from System import EventArgs, Array

from Autodesk.Revit.DB import (
    Transaction, OverrideGraphicSettings, ElementId,
    Color as RevitColor, View, ViewType, View3D, ViewFamilyType,
    FilteredElementCollector, BuiltInCategory, TransactionStatus,
    BoundingBoxXYZ, XYZ, FillPatternElement, FillPattern
)

from pyrevit import revit, DB, UI, forms, script
import System.Windows.Forms as WinForms
from System.Threading import Thread, ThreadStart
from System.Windows.Threading import Dispatcher

class WarningItem:
    """Data class for warnings display"""
    def __init__(self, message, element_ids, element_info, occurrence_count=1, is_group=True, parent_item=None):
        self.Message = message
        self.ElementCount = len(element_ids)
        self.OccurrenceCount = occurrence_count if is_group else ""  # Only show for group rows
        self.ElementIds = "; ".join([str(eid.IntegerValue) for eid in element_ids])
        self.ElementNames = "; ".join([info['name'] for info in element_info])
        self.Levels = "; ".join(list(set([info['level'] for info in element_info])))
        self.Views = "; ".join(list(set([view for info in element_info for view in info['views']])))
        self.Categories = "; ".join(list(set([info['category'] for info in element_info])))
        self._element_ids = element_ids  # Store for highlighting
        self.IsGroup = is_group
        self.IsExpanded = False
        self.Children = []  # Child warning items
        self.Parent = parent_item  # Reference to parent group
        self.ExpandSymbol = "[+]" if is_group and occurrence_count > 1 else ""

class WarningsBrowserWindow(Window):
    """Modal window for enhanced warnings browsing"""
    
    def __init__(self):
        self.doc = revit.doc
        self.uidoc = revit.uidoc
        self.highlighted_elements = []
        self.saved_highlights = []  # Persistent highlights when "Highlight speichern" is enabled
        self.current_override = None
        self.created_3d_views = []  # Track 3D views created by this tool

        # Cache views for performance
        self.cached_floor_plans = []
        self.cached_3d_views = []
        self._cache_views()

        self.InitializeComponent()
        self.LoadWarnings()
        self.SetupHighlighting()

    def _cache_views(self):
        """Cache floor plans and 3D views once for performance"""
        try:
            all_views = FilteredElementCollector(self.doc)\
                       .OfClass(View)\
                       .WhereElementIsNotElementType()\
                       .ToElements()

            for view in all_views:
                if view.IsTemplate:
                    continue
                if view.ViewType == ViewType.FloorPlan:
                    self.cached_floor_plans.append(view)
                elif view.ViewType == ViewType.ThreeD:
                    self.cached_3d_views.append(view)
        except Exception as ex:
            script.get_logger().error("Error caching views: {}".format(str(ex)))
    
    def InitializeComponent(self):
        """Initialize the WPF interface"""
        # Set window properties
        self.Title = "Enhanced Warnings Browser - ICL"
        self.Width = 1400
        self.Height = 700
        self.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen

        # Add closing event handler to clear overrides
        self.Closing += self.OnWindowClosing
        
        # Create main grid
        main_grid = Grid()
        
        # Row definitions
        from System.Windows import GridLength, GridUnitType
        from System.Windows.Controls import RowDefinition
        
        header_row = RowDefinition()
        header_row.Height = GridLength(40)
        main_grid.RowDefinitions.Add(header_row)
        
        content_row = RowDefinition()
        content_row.Height = GridLength(1, GridUnitType.Star)
        main_grid.RowDefinitions.Add(content_row)
        
        button_row = RowDefinition()
        button_row.Height = GridLength(60)
        main_grid.RowDefinitions.Add(button_row)
        
        # Header
        header = TextBlock()
        header.Text = "Enhanced Warnings Browser - ICL Ingenieur Consult GmbH"
        header.FontSize = 16
        header.FontWeight = System.Windows.FontWeights.Bold
        header.HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        header.VerticalAlignment = System.Windows.VerticalAlignment.Center
        header.Foreground = SolidColorBrush(Colors.DarkBlue)
        Grid.SetRow(header, 0)
        main_grid.Children.Add(header)
        
        # DataGrid - has built-in scrolling
        self.dataGrid = DataGrid()
        self.dataGrid.AutoGenerateColumns = False
        self.dataGrid.CanUserAddRows = False
        self.dataGrid.CanUserDeleteRows = False
        self.dataGrid.IsReadOnly = True
        self.dataGrid.SelectionMode = System.Windows.Controls.DataGridSelectionMode.Extended  # Enable multi-select
        self.dataGrid.GridLinesVisibility = System.Windows.Controls.DataGridGridLinesVisibility.All
        self.dataGrid.HeadersVisibility = System.Windows.Controls.DataGridHeadersVisibility.Column

        # Enable mouse wheel scrolling
        self.dataGrid.CanUserResizeRows = False
        self.dataGrid.EnableRowVirtualization = True
        self.dataGrid.EnableColumnVirtualization = True
        
        # Create columns
        self.CreateColumns()
        
        # Add selection changed event
        self.dataGrid.SelectionChanged += self.OnSelectionChanged

        # Add mouse double-click event for expand/collapse
        self.dataGrid.MouseDoubleClick += MouseButtonEventHandler(self.OnRowDoubleClick)

        # Add DataGrid directly to grid
        Grid.SetRow(self.dataGrid, 1)
        main_grid.Children.Add(self.dataGrid)
        
        # Button panel
        button_panel = StackPanel()
        button_panel.Orientation = System.Windows.Controls.Orientation.Horizontal
        button_panel.HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        button_panel.Margin = System.Windows.Thickness(10)
        
        # Auto-highlight checkbox
        self.autoHighlightCb = CheckBox()
        self.autoHighlightCb.Content = "Auto-Highlight bei Auswahl"
        self.autoHighlightCb.IsChecked = True
        self.autoHighlightCb.Margin = System.Windows.Thickness(5)
        self.autoHighlightCb.VerticalAlignment = System.Windows.VerticalAlignment.Center
        button_panel.Children.Add(self.autoHighlightCb)

        # Save highlights checkbox
        self.saveHighlightsCb = CheckBox()
        self.saveHighlightsCb.Content = "Highlight speichern"
        self.saveHighlightsCb.IsChecked = False
        self.saveHighlightsCb.Margin = System.Windows.Thickness(5)
        self.saveHighlightsCb.VerticalAlignment = System.Windows.VerticalAlignment.Center
        button_panel.Children.Add(self.saveHighlightsCb)

        # Highlight button
        self.highlight_btn = Button()
        self.highlight_btn.Content = "Markieren"
        self.highlight_btn.Width = 100
        self.highlight_btn.Height = 30
        self.highlight_btn.Margin = System.Windows.Thickness(5)
        self.highlight_btn.Click += RoutedEventHandler(self.HighlightElements)
        button_panel.Children.Add(self.highlight_btn)

        # Clear button
        clear_btn = Button()
        clear_btn.Content = "Löschen"
        clear_btn.Width = 100
        clear_btn.Height = 30
        clear_btn.Margin = System.Windows.Thickness(5)
        clear_btn.Click += RoutedEventHandler(self.ClearHighlights)
        button_panel.Children.Add(clear_btn)

        # Clear saved highlights button
        clear_saved_btn = Button()
        clear_saved_btn.Content = "Alle Löschen"
        clear_saved_btn.Width = 100
        clear_saved_btn.Height = 30
        clear_saved_btn.Margin = System.Windows.Thickness(5)
        clear_saved_btn.Click += RoutedEventHandler(self.ClearAllHighlights)
        button_panel.Children.Add(clear_saved_btn)

        # Zoom button (current view only)
        zoom_current_btn = Button()
        zoom_current_btn.Content = "Zoom (Aktuelle Ansicht)"
        zoom_current_btn.Width = 150
        zoom_current_btn.Height = 30
        zoom_current_btn.Margin = System.Windows.Thickness(5)
        zoom_current_btn.Click += RoutedEventHandler(self.ZoomToElementsCurrentView)
        button_panel.Children.Add(zoom_current_btn)

        # Zoom button (all views)
        zoom_btn = Button()
        zoom_btn.Content = "Zoom (Alle Ansichten)"
        zoom_btn.Width = 150
        zoom_btn.Height = 30
        zoom_btn.Margin = System.Windows.Thickness(5)
        zoom_btn.Click += RoutedEventHandler(self.ZoomToElements)
        button_panel.Children.Add(zoom_btn)

        # Show in 3D view button
        view_3d_btn = Button()
        view_3d_btn.Content = "In 3D Ansicht zeigen"
        view_3d_btn.Width = 150
        view_3d_btn.Height = 30
        view_3d_btn.Margin = System.Windows.Thickness(5)
        view_3d_btn.Click += RoutedEventHandler(self.ShowIn3DView)
        button_panel.Children.Add(view_3d_btn)

        # Export button
        export_btn = Button()
        export_btn.Content = "Export HTML"
        export_btn.Width = 100
        export_btn.Height = 30
        export_btn.Margin = System.Windows.Thickness(5)
        export_btn.Click += RoutedEventHandler(self.ExportToHtml)
        button_panel.Children.Add(export_btn)
        
        Grid.SetRow(button_panel, 2)
        main_grid.Children.Add(button_panel)
        
        self.Content = main_grid
    
    def CreateColumns(self):
        """Create DataGrid columns"""

        # Expand/Collapse column
        col_expand = DataGridTextColumn()
        col_expand.Header = ""
        col_expand.Binding = Binding("ExpandSymbol")
        col_expand.Width = DataGridLength(40)
        col_expand.IsReadOnly = True
        self.dataGrid.Columns.Add(col_expand)

        # Warning Message
        col1 = DataGridTextColumn()
        col1.Header = "Fehlermeldung"
        col1.Binding = Binding("Message")
        col1.Width = DataGridLength(280)
        self.dataGrid.Columns.Add(col1)

        # Occurrence Count
        col2 = DataGridTextColumn()
        col2.Header = "Vorkommen"
        col2.Binding = Binding("OccurrenceCount")
        col2.Width = DataGridLength(80)
        self.dataGrid.Columns.Add(col2)

        # Element Count
        col3 = DataGridTextColumn()
        col3.Header = "Elemente"
        col3.Binding = Binding("ElementCount")
        col3.Width = DataGridLength(70)
        self.dataGrid.Columns.Add(col3)

        # Element IDs
        col4 = DataGridTextColumn()
        col4.Header = "Element IDs"
        col4.Binding = Binding("ElementIds")
        col4.Width = DataGridLength(150)
        self.dataGrid.Columns.Add(col4)

        # Element Names
        col5 = DataGridTextColumn()
        col5.Header = "Elementnamen"
        col5.Binding = Binding("ElementNames")
        col5.Width = DataGridLength(200)
        self.dataGrid.Columns.Add(col5)

        # Levels
        col6 = DataGridTextColumn()
        col6.Header = "Ebenen"
        col6.Binding = Binding("Levels")
        col6.Width = DataGridLength(120)
        self.dataGrid.Columns.Add(col6)

        # Views
        col7 = DataGridTextColumn()
        col7.Header = "Ansichten"
        col7.Binding = Binding("Views")
        col7.Width = DataGridLength(150)
        self.dataGrid.Columns.Add(col7)

        # Categories
        col8 = DataGridTextColumn()
        col8.Header = "Kategorien"
        col8.Binding = Binding("Categories")
        col8.Width = DataGridLength(120)
        self.dataGrid.Columns.Add(col8)
    
    def LoadWarnings(self):
        """Load warnings from the document and group identical warnings"""
        warnings_collection = ObservableCollection[object]()

        try:
            warnings = self.doc.GetWarnings()

            # Group warnings by message
            warning_groups = {}

            for warning in warnings:
                message = warning.GetDescriptionText()
                element_ids = list(warning.GetFailingElements())

                if not element_ids:
                    continue

                # Group by message
                if message not in warning_groups:
                    warning_groups[message] = []

                warning_groups[message].append({
                    'element_ids': element_ids,
                    'warning': warning
                })

            # Create warning items with occurrence count and children
            for message, occurrences in warning_groups.items():
                # Collect all element IDs and info for this warning type
                all_element_ids = []
                all_element_info = []

                for occurrence in occurrences:
                    all_element_ids.extend(occurrence['element_ids'])
                    for elem_id in occurrence['element_ids']:
                        info = self.GetElementInfo(elem_id)
                        all_element_info.append(info)

                # Create grouped warning item (parent)
                warning_item = WarningItem(
                    message,
                    all_element_ids,
                    all_element_info,
                    occurrence_count=len(occurrences),
                    is_group=True
                )

                # Create child items for each occurrence (only if more than 1)
                if len(occurrences) > 1:
                    for i, occurrence in enumerate(occurrences):
                        child_element_info = []
                        for elem_id in occurrence['element_ids']:
                            info = self.GetElementInfo(elem_id)
                            child_element_info.append(info)

                        child_item = WarningItem(
                            "  → Occurrence {}".format(i + 1),
                            occurrence['element_ids'],
                            child_element_info,
                            occurrence_count=1,
                            is_group=False,
                            parent_item=warning_item
                        )
                        warning_item.Children.append(child_item)

                warnings_collection.Add(warning_item)

            self.dataGrid.ItemsSource = warnings_collection

            script.get_logger().debug("Loaded {} unique warning types from {} total warnings".format(
                len(warning_groups), len(warnings)))

        except Exception as ex:
            script.get_logger().error("Error loading warnings: {}".format(str(ex)))
    
    def GetElementInfo(self, element_id):
        """Get detailed information about an element"""
        info = {
            'name': 'Unknown',
            'level': 'N/A',
            'category': 'N/A',
            'views': []
        }

        try:
            element = self.doc.GetElement(element_id)
            if not element:
                return info

            # Element name
            if hasattr(element, 'Name') and element.Name:
                info['name'] = element.Name
            else:
                info['name'] = "ID: {}".format(element_id.IntegerValue)

            # Level
            if hasattr(element, 'LevelId') and element.LevelId != ElementId.InvalidElementId:
                level_elem = self.doc.GetElement(element.LevelId)
                if level_elem:
                    info['level'] = level_elem.Name
            elif hasattr(element, 'Level') and element.Level:
                info['level'] = element.Level.Name

            # Category
            if element.Category:
                info['category'] = element.Category.Name

            # Get one suitable view - optimized
            view_name = self.GetElementView(element)
            if view_name:
                info['views'] = [view_name]

        except Exception as ex:
            script.get_logger().error("Error getting element info for {}: {}".format(element_id.IntegerValue, str(ex)))

        return info
    
    def GetElementView(self, element):
        """Get one suitable view where element is visible - optimized with cached views"""
        try:
            # First, try floor plan views from cache
            for view in self.cached_floor_plans:
                try:
                    if not element.IsHidden(view):
                        if not element.Category or not view.GetCategoryHidden(element.Category.Id):
                            return view.Name
                except:
                    continue

            # If not found in floor plans, try 3D views
            for view in self.cached_3d_views:
                try:
                    if not element.IsHidden(view):
                        if not element.Category or not view.GetCategoryHidden(element.Category.Id):
                            return view.Name
                except:
                    continue

        except Exception as ex:
            script.get_logger().error("Error getting element view: {}".format(str(ex)))

        return None
    
    def SetupHighlighting(self):
        """Setup element highlighting graphics with solid red fill"""
        self.current_override = OverrideGraphicSettings()

        # Red highlight color
        red_color = RevitColor(255, 0, 0)
        self.current_override.SetProjectionLineColor(red_color)
        self.current_override.SetCutLineColor(red_color)

        # Set solid red fill pattern for surfaces
        try:
            # Get solid fill pattern
            solid_pattern = None
            fill_patterns = FilteredElementCollector(self.doc)\
                .OfClass(FillPatternElement)\
                .ToElements()

            for pattern in fill_patterns:
                if pattern.GetFillPattern().IsSolidFill:
                    solid_pattern = pattern.Id
                    break

            if solid_pattern:
                self.current_override.SetSurfaceForegroundPatternId(solid_pattern)
                self.current_override.SetSurfaceForegroundPatternColor(red_color)
                self.current_override.SetCutForegroundPatternId(solid_pattern)
                self.current_override.SetCutForegroundPatternColor(red_color)

        except Exception as ex:
            script.get_logger().error("Error setting fill pattern: {}".format(str(ex)))

        # Keep some transparency
        self.current_override.SetSurfaceTransparency(30)
    
    def OnSelectionChanged(self, sender, e):
        """Handle selection change in the grid"""
        if self.autoHighlightCb.IsChecked:
            self.HighlightElements(sender, e)

    def OnRowDoubleClick(self, sender, e):
        """Handle double-click to expand/collapse groups"""
        selected_item = self.dataGrid.SelectedItem
        if not selected_item or not hasattr(selected_item, 'IsGroup'):
            return

        if selected_item.IsGroup and len(selected_item.Children) > 0:
            # Toggle expansion
            selected_item.IsExpanded = not selected_item.IsExpanded

            if selected_item.IsExpanded:
                # Expand: insert children after parent
                selected_item.ExpandSymbol = "[-]"
                items = list(self.dataGrid.ItemsSource)
                parent_index = items.index(selected_item)

                # Insert children
                for i, child in enumerate(selected_item.Children):
                    items.insert(parent_index + 1 + i, child)

                # Update ItemsSource
                new_collection = ObservableCollection[object]()
                for item in items:
                    new_collection.Add(item)
                self.dataGrid.ItemsSource = new_collection

            else:
                # Collapse: remove children
                selected_item.ExpandSymbol = "[+]"
                items = list(self.dataGrid.ItemsSource)

                # Remove children
                items = [item for item in items if item not in selected_item.Children]

                # Update ItemsSource
                new_collection = ObservableCollection[object]()
                for item in items:
                    new_collection.Add(item)
                self.dataGrid.ItemsSource = new_collection

            # Reselect the parent item
            self.dataGrid.SelectedItem = selected_item

    def OnWindowClosing(self, sender, e):
        """Clear all overrides when window is closing (except in created 3D views)"""
        script.get_logger().debug("Window closing - clearing overrides in non-3D views")

        # Only clear highlights if not in a created 3D view
        active_view_id = self.uidoc.ActiveView.Id
        is_created_3d_view = any(view_id == active_view_id for view_id in self.created_3d_views)

        if is_created_3d_view:
            script.get_logger().debug("Keeping overrides in created 3D view")
            # Don't clear, but reset the tracked elements so we don't try to clear them
            self.highlighted_elements = []
        else:
            # Clear highlights in regular views
            self.ClearHighlights(None, None)

    def HighlightElements(self, sender, e):
        """Highlight selected warning elements - supports multiple selection"""
        script.get_logger().debug("HighlightElements called")

        selected_items = self.dataGrid.SelectedItems
        if not selected_items or selected_items.Count == 0:
            forms.alert("Bitte wählen Sie eine oder mehrere Warnungen aus der Liste aus.")
            return

        # Collect all element IDs from all selected warnings
        all_element_ids = []
        for item in selected_items:
            all_element_ids.extend(item._element_ids)

        script.get_logger().debug("Total elements from {} selected warnings: {}".format(
            selected_items.Count, len(all_element_ids)))

        # Clear previous highlights first (only clears temporary ones)
        self.ClearHighlights(None, None)

        # Perform highlighting in a transaction
        t = Transaction(self.doc, "Highlight Warning Elements")

        try:
            status = t.Start()
            if status != TransactionStatus.Started:
                forms.alert("Konnte Transaktion nicht starten")
                return

            active_view = self.uidoc.ActiveView
            newly_highlighted = []

            for elem_id in all_element_ids:
                try:
                    element = self.doc.GetElement(elem_id)
                    if element:
                        try:
                            if not element.IsHidden(active_view):
                                active_view.SetElementOverrides(elem_id, self.current_override)
                                newly_highlighted.append(elem_id)
                        except:
                            # Element might not be visible in current view
                            pass
                except Exception as ex:
                    script.get_logger().error("Error highlighting element {}: {}".format(elem_id.IntegerValue, str(ex)))

            # Add new highlights to the current list
            for elem_id in newly_highlighted:
                if elem_id not in self.highlighted_elements:
                    self.highlighted_elements.append(elem_id)

            # If "Highlight speichern" is checked, add to saved highlights
            if self.saveHighlightsCb.IsChecked:
                for elem_id in newly_highlighted:
                    if elem_id not in self.saved_highlights:
                        self.saved_highlights.append(elem_id)
                script.get_logger().debug("Added {} elements to saved highlights (total saved: {})".format(
                    len(newly_highlighted), len(self.saved_highlights)))

            t.Commit()
            script.get_logger().debug("Highlighted {} elements (visible in current view)".format(
                len(newly_highlighted)))

        except Exception as ex:
            if t.HasStarted() and not t.HasEnded():
                t.RollBack()
            forms.alert("Fehler beim Markieren: {}".format(str(ex)))
            script.get_logger().error("Error in highlight operation: {}".format(str(ex)))
    
    def ClearHighlights(self, sender, e):
        """Clear all element highlights (respects saved highlights)"""
        script.get_logger().debug("ClearHighlights called")

        # Determine which elements to clear - only clear temporary highlights, not saved ones
        elements_to_clear = [elem_id for elem_id in self.highlighted_elements if elem_id not in self.saved_highlights]

        if not elements_to_clear:
            script.get_logger().debug("No temporary elements to clear")
            return

        t = Transaction(self.doc, "Clear Highlights")

        try:
            status = t.Start()
            if status != TransactionStatus.Started:
                script.get_logger().error("Could not start clear transaction")
                return

            active_view = self.uidoc.ActiveView
            cleared_count = 0

            for elem_id in elements_to_clear:
                try:
                    # Only clear if not in saved highlights
                    if elem_id not in self.saved_highlights:
                        active_view.SetElementOverrides(elem_id, OverrideGraphicSettings())
                        cleared_count += 1
                except:
                    pass

            # Remove only cleared elements from highlighted_elements
            self.highlighted_elements = [elem_id for elem_id in self.highlighted_elements if elem_id in self.saved_highlights]

            t.Commit()
            script.get_logger().debug("Cleared {} temporary element highlights ({} saved highlights preserved)".format(
                cleared_count, len(self.saved_highlights)))

        except Exception as ex:
            if t.HasStarted() and not t.HasEnded():
                t.RollBack()
            script.get_logger().error("Error clearing highlights: {}".format(str(ex)))

    def ClearAllHighlights(self, sender, e):
        """Clear ALL element highlights including saved ones"""
        script.get_logger().debug("ClearAllHighlights called")

        if not self.highlighted_elements and not self.saved_highlights:
            script.get_logger().debug("No elements to clear")
            return

        t = Transaction(self.doc, "Clear All Highlights")

        try:
            status = t.Start()
            if status != TransactionStatus.Started:
                script.get_logger().error("Could not start clear transaction")
                return

            active_view = self.uidoc.ActiveView
            cleared_count = 0

            # Clear all highlighted elements
            all_to_clear = list(set(self.highlighted_elements + self.saved_highlights))
            for elem_id in all_to_clear:
                try:
                    active_view.SetElementOverrides(elem_id, OverrideGraphicSettings())
                    cleared_count += 1
                except:
                    pass

            # Reset all lists
            self.highlighted_elements = []
            self.saved_highlights = []

            t.Commit()
            script.get_logger().debug("Cleared all {} element highlights (including saved)".format(cleared_count))

        except Exception as ex:
            if t.HasStarted() and not t.HasEnded():
                t.RollBack()
            script.get_logger().error("Error clearing all highlights: {}".format(str(ex)))
    
    def ZoomToElementsCurrentView(self, sender, e):
        """Zoom to selected elements in current view only"""
        script.get_logger().debug("ZoomToElementsCurrentView called")

        selected_items = self.dataGrid.SelectedItems
        if not selected_items or selected_items.Count == 0:
            forms.alert("Bitte wählen Sie eine oder mehrere Warnungen aus der Liste aus.")
            return

        try:
            # Collect all element IDs from all selected warnings
            all_element_ids = []
            for item in selected_items:
                all_element_ids.extend(item._element_ids)

            # Filter to only elements visible in current view
            active_view = self.uidoc.ActiveView
            visible_element_ids = []

            for elem_id in all_element_ids:
                try:
                    element = self.doc.GetElement(elem_id)
                    if element:
                        try:
                            if not element.IsHidden(active_view):
                                # Check if category is visible in view
                                if not element.Category or not active_view.GetCategoryHidden(element.Category.Id):
                                    visible_element_ids.append(elem_id)
                        except:
                            pass
                except:
                    pass

            if not visible_element_ids:
                forms.alert("Keine Elemente in der aktuellen Ansicht gefunden.\n\n"
                           "Tipp: Verwenden Sie 'Zoom (Alle Ansichten)' um zu einer Ansicht zu wechseln wo die Elemente sichtbar sind.")
                return

            if visible_element_ids:
                # Convert to List for SetElementIds
                from System.Collections.Generic import List
                id_list = List[ElementId](visible_element_ids)

                script.get_logger().debug("Setting selection to {} elements (visible in current view) from {} warnings".format(
                    len(visible_element_ids), selected_items.Count))
                self.uidoc.Selection.SetElementIds(id_list)
                self.uidoc.ShowElements(id_list)

                script.get_logger().debug("Zoomed to elements in current view successfully")
        except Exception as ex:
            forms.alert("Fehler beim Zoomen: {}".format(str(ex)))
            script.get_logger().error("Error zooming to elements in current view: {}".format(str(ex)))

    def ZoomToElements(self, sender, e):
        """Zoom to selected elements - supports multiple selection (may switch views intelligently)"""
        script.get_logger().debug("ZoomToElements called")

        selected_items = self.dataGrid.SelectedItems
        if not selected_items or selected_items.Count == 0:
            forms.alert("Bitte wählen Sie eine oder mehrere Warnungen aus der Liste aus.")
            return

        try:
            # Collect all element IDs from all selected warnings
            all_element_ids = []
            for item in selected_items:
                all_element_ids.extend(item._element_ids)

            if all_element_ids:
                # Try to find the best view for these elements
                best_view = self._find_best_view_for_elements(all_element_ids)

                if best_view and best_view.Id != self.uidoc.ActiveView.Id:
                    script.get_logger().debug("Switching to view: {}".format(best_view.Name))
                    self.uidoc.ActiveView = best_view

                # Convert to List for SetElementIds
                from System.Collections.Generic import List
                id_list = List[ElementId](all_element_ids)

                script.get_logger().debug("Setting selection to {} elements from {} warnings".format(
                    len(all_element_ids), selected_items.Count))
                self.uidoc.Selection.SetElementIds(id_list)
                self.uidoc.ShowElements(id_list)

                script.get_logger().debug("Zoomed to elements successfully")
        except Exception as ex:
            forms.alert("Fehler beim Zoomen: {}".format(str(ex)))
            script.get_logger().error("Error zooming to elements: {}".format(str(ex)))

    def _find_best_view_for_elements(self, element_ids):
        """Find the most appropriate view for displaying the given elements"""
        if not element_ids:
            return None

        try:
            # Get the first element to determine level
            first_element = self.doc.GetElement(element_ids[0])
            if not first_element:
                return None

            # Try to get the element's level
            element_level = None
            level_name = None

            if hasattr(first_element, 'LevelId') and first_element.LevelId != ElementId.InvalidElementId:
                level_elem = self.doc.GetElement(first_element.LevelId)
                if level_elem:
                    element_level = level_elem
                    level_name = level_elem.Name
            elif hasattr(first_element, 'Level') and first_element.Level:
                element_level = first_element.Level
                level_name = first_element.Level.Name

            script.get_logger().debug("Element level: {}".format(level_name if level_name else "None"))

            # Get all floor plan views
            floor_plan_views = [v for v in self.cached_floor_plans if not v.IsTemplate]

            if level_name and floor_plan_views:
                # Try exact match first
                for view in floor_plan_views:
                    if view.Name == level_name:
                        script.get_logger().debug("Found exact match view: {}".format(view.Name))
                        return view

                # Try partial match (e.g., "OG 3" in "OG 3 - OK FFB")
                level_parts = level_name.split()
                for view in floor_plan_views:
                    view_name = view.Name
                    # Check if any significant part of level name is in view name
                    for part in level_parts:
                        if len(part) >= 2 and part in view_name:
                            script.get_logger().debug("Found partial match view: {} (matches '{}')".format(
                                view.Name, part))
                            return view

                # Try to match by level object
                if element_level:
                    for view in floor_plan_views:
                        try:
                            if hasattr(view, 'GenLevel') and view.GenLevel and view.GenLevel.Id == element_level.Id:
                                script.get_logger().debug("Found view by level ID: {}".format(view.Name))
                                return view
                        except:
                            pass

            # Check if element is visible in current view
            active_view = self.uidoc.ActiveView
            try:
                if not first_element.IsHidden(active_view):
                    if not first_element.Category or not active_view.GetCategoryHidden(first_element.Category.Id):
                        script.get_logger().debug("Element visible in current view, staying here")
                        return active_view
            except:
                pass

            # Fall back to first available floor plan
            if floor_plan_views:
                script.get_logger().debug("Using first available floor plan: {}".format(floor_plan_views[0].Name))
                return floor_plan_views[0]

            # Last resort: try 3D view
            if self.cached_3d_views:
                script.get_logger().debug("Using 3D view as fallback: {}".format(self.cached_3d_views[0].Name))
                return self.cached_3d_views[0]

        except Exception as ex:
            script.get_logger().error("Error finding best view: {}".format(str(ex)))

        return None

    def ShowIn3DView(self, sender, e):
        """Create a new 3D view with crop box focused on selected warning elements"""
        script.get_logger().debug("ShowIn3DView called")

        selected_items = self.dataGrid.SelectedItems
        if not selected_items or selected_items.Count == 0:
            forms.alert("Bitte wählen Sie eine oder mehrere Warnungen aus der Liste aus.")
            return

        try:
            # Collect all element IDs from all selected warnings
            all_element_ids = []
            for item in selected_items:
                all_element_ids.extend(item._element_ids)

            if not all_element_ids:
                forms.alert("Keine Elemente gefunden.")
                return

            # Calculate bounding box for all elements
            min_x = min_y = min_z = float('inf')
            max_x = max_y = max_z = float('-inf')

            valid_elements = 0
            for elem_id in all_element_ids:
                try:
                    element = self.doc.GetElement(elem_id)
                    if element and hasattr(element, 'get_BoundingBox'):
                        bbox = element.get_BoundingBox(None)
                        if bbox:
                            min_x = min(min_x, bbox.Min.X)
                            min_y = min(min_y, bbox.Min.Y)
                            min_z = min(min_z, bbox.Min.Z)
                            max_x = max(max_x, bbox.Max.X)
                            max_y = max(max_y, bbox.Max.Y)
                            max_z = max(max_z, bbox.Max.Z)
                            valid_elements += 1
                except:
                    pass

            if valid_elements == 0:
                forms.alert("Konnte keine Bounding Box für die Elemente berechnen.")
                return

            # Add some padding (10% on each side)
            padding_x = (max_x - min_x) * 0.1
            padding_y = (max_y - min_y) * 0.1
            padding_z = (max_z - min_z) * 0.1

            min_x -= padding_x
            min_y -= padding_y
            min_z -= padding_z
            max_x += padding_x
            max_y += padding_y
            max_z += padding_z

            # Start transaction
            t = Transaction(self.doc, "Create 3D View for Warnings")
            t.Start()

            try:
                # Get 3D view type
                view_family_types = FilteredElementCollector(self.doc)\
                    .OfClass(ViewFamilyType)\
                    .ToElements()

                view_type_3d = None
                for vft in view_family_types:
                    if vft.ViewFamily == DB.ViewFamily.ThreeDimensional:
                        view_type_3d = vft
                        break

                if not view_type_3d:
                    forms.alert("Konnte keinen 3D View Type finden.")
                    t.RollBack()
                    return

                # Create new 3D view
                new_view = View3D.CreateIsometric(self.doc, view_type_3d.Id)

                # Generate unique name
                from System import DateTime
                timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss")
                new_view.Name = "Warnings_3D_{}".format(timestamp)

                # Create and set crop box
                crop_box = BoundingBoxXYZ()
                crop_box.Min = XYZ(min_x, min_y, min_z)
                crop_box.Max = XYZ(max_x, max_y, max_z)

                new_view.SetSectionBox(crop_box)
                new_view.CropBoxActive = True
                new_view.CropBoxVisible = True

                # Apply red overrides to ALL highlighted elements (saved + current) in this view
                all_highlights = list(set(all_element_ids + self.saved_highlights))
                for elem_id in all_highlights:
                    try:
                        new_view.SetElementOverrides(elem_id, self.current_override)
                    except:
                        pass

                t.Commit()

                # Track this view so we don't clear overrides when closing
                self.created_3d_views.append(new_view.Id)

                # Switch to the new view
                self.uidoc.ActiveView = new_view

                # Select the elements
                from System.Collections.Generic import List
                id_list = List[ElementId](all_element_ids)
                self.uidoc.Selection.SetElementIds(id_list)

                total_highlighted = len(all_highlights)
                script.get_logger().debug("Created 3D view '{}' with {} elements ({} current + {} saved)".format(
                    new_view.Name, total_highlighted, len(all_element_ids), len(self.saved_highlights)))

                msg = "3D Ansicht '{}' wurde erstellt.\n\nDie roten Markierungen bleiben in dieser Ansicht erhalten.".format(new_view.Name)
                if len(self.saved_highlights) > 0:
                    msg += "\n\nGespeicherte Highlights: {} Elemente".format(len(self.saved_highlights))
                forms.alert(msg)

            except Exception as ex:
                if t.HasStarted() and not t.HasEnded():
                    t.RollBack()
                raise

        except Exception as ex:
            forms.alert("Fehler beim Erstellen der 3D Ansicht: {}".format(str(ex)))
            script.get_logger().error("Error creating 3D view: {}".format(str(ex)))

    def ExportToHtml(self, sender, e):
        """Export warnings to HTML file"""
        script.get_logger().debug("ExportToHtml called")

        try:
            save_dialog = WinForms.SaveFileDialog()
            save_dialog.Filter = "HTML files (*.html)|*.html"
            save_dialog.DefaultExt = "html"
            save_dialog.FileName = "Enhanced_Warnings_{}.html".format(
                self.doc.Title.replace(" ", "_")
            )

            result = save_dialog.ShowDialog()
            script.get_logger().debug("SaveDialog result: {}".format(result))

            if result == WinForms.DialogResult.OK:
                self.CreateHtmlReport(save_dialog.FileName)
                forms.alert("Export erfolgreich: {}".format(save_dialog.FileName))

        except Exception as ex:
            forms.alert("Export Fehler: {}".format(str(ex)))
            script.get_logger().error("Error exporting to HTML: {}".format(str(ex)))
    
    def CreateHtmlReport(self, file_path):
        """Create HTML report"""
        html_template = """<!DOCTYPE html>
<html>
<head>
    <meta charset="UTF-8">
    <title>Enhanced Warnings Report - {project}</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 20px; }}
        h1 {{ color: #2c5aa0; text-align: center; }}
        .header {{ text-align: center; margin-bottom: 20px; }}
        table {{ border-collapse: collapse; width: 100%; }}
        th, td {{ border: 1px solid #ddd; padding: 8px; text-align: left; }}
        th {{ background-color: #2c5aa0; color: white; }}
        tr:nth-child(even) {{ background-color: #f9f9f9; }}
        .warning-msg {{ max-width: 300px; word-wrap: break-word; }}
    </style>
</head>
<body>
    <div class="header">
        <h1>Enhanced Warnings Report</h1>
        <p><strong>Projekt:</strong> {project}</p>
        <p><strong>Erstellt:</strong> {date}</p>
        <p><strong>ICL Ingenieur Consult GmbH</strong></p>
    </div>
    <table>
        <thead>
            <tr>
                <th>Nr.</th>
                <th>Fehlermeldung</th>
                <th>Anzahl</th>
                <th>Element IDs</th>
                <th>Elementnamen</th>
                <th>Ebenen</th>
                <th>Ansichten</th>
                <th>Kategorien</th>
            </tr>
        </thead>
        <tbody>
            {rows}
        </tbody>
    </table>
</body>
</html>"""
        
        # Generate table rows
        rows = ""
        for i, item in enumerate(self.dataGrid.ItemsSource, 1):
            rows += "<tr>"
            rows += "<td>{}</td>".format(i)
            rows += "<td class='warning-msg'>{}</td>".format(self.escape_html(item.Message))
            rows += "<td>{}</td>".format(item.ElementCount)
            rows += "<td>{}</td>".format(self.escape_html(item.ElementIds))
            rows += "<td>{}</td>".format(self.escape_html(item.ElementNames))
            rows += "<td>{}</td>".format(self.escape_html(item.Levels))
            rows += "<td>{}</td>".format(self.escape_html(item.Views))
            rows += "<td>{}</td>".format(self.escape_html(item.Categories))
            rows += "</tr>"
        
        from System import DateTime
        current_time = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")
        
        final_html = html_template.format(
            project=self.doc.Title,
            date=current_time,
            rows=rows
        )
        
        with open(file_path, 'w', encoding='utf-8') as f:
            f.write(final_html)
    
    def escape_html(self, text):
        """Escape HTML characters"""
        if not text:
            return ""
        return str(text).replace("&", "&amp;").replace("<", "&lt;").replace(">", "&gt;")

# Main execution
try:
    if not revit.doc:
        forms.alert("Kein aktives Revit Dokument gefunden.")
    else:
        # Show the warnings browser window as modal
        window = WarningsBrowserWindow()
        window.ShowDialog()  # Modal

except Exception as ex:
    forms.alert("Fehler beim Starten: {}".format(str(ex)))
    script.get_logger().error("Error in Enhanced Warnings Browser: {}".format(str(ex)))
