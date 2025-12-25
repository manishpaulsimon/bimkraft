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
    Color as RevitColor, View, ViewType,
    FilteredElementCollector, BuiltInCategory, TransactionStatus
)

from pyrevit import revit, DB, UI, forms, script
import System.Windows.Forms as WinForms
from System.Threading import Thread, ThreadStart
from System.Windows.Threading import Dispatcher

class WarningItem:
    """Data class for warnings display"""
    def __init__(self, message, element_ids, element_info):
        self.Message = message
        self.ElementCount = len(element_ids)
        self.ElementIds = "; ".join([str(eid.IntegerValue) for eid in element_ids])
        self.ElementNames = "; ".join([info['name'] for info in element_info])
        self.Levels = "; ".join(list(set([info['level'] for info in element_info])))
        self.Views = "; ".join(list(set([view for info in element_info for view in info['views']])))
        self.Categories = "; ".join(list(set([info['category'] for info in element_info])))
        self._element_ids = element_ids  # Store for highlighting

class WarningsBrowserWindow(Window):
    """Modal window for enhanced warnings browsing"""
    
    def __init__(self):
        self.doc = revit.doc
        self.uidoc = revit.uidoc
        self.highlighted_elements = []
        self.current_override = None

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
        
        # DataGrid with ScrollViewer
        scroll_viewer = ScrollViewer()
        scroll_viewer.VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto
        scroll_viewer.HorizontalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto
        
        self.dataGrid = DataGrid()
        self.dataGrid.AutoGenerateColumns = False
        self.dataGrid.CanUserAddRows = False
        self.dataGrid.CanUserDeleteRows = False
        self.dataGrid.IsReadOnly = True
        self.dataGrid.SelectionMode = System.Windows.Controls.DataGridSelectionMode.Extended  # Enable multi-select
        self.dataGrid.GridLinesVisibility = System.Windows.Controls.DataGridGridLinesVisibility.All
        self.dataGrid.HeadersVisibility = System.Windows.Controls.DataGridHeadersVisibility.Column
        
        # Create columns
        self.CreateColumns()
        
        # Add selection changed event
        self.dataGrid.SelectionChanged += self.OnSelectionChanged
        
        scroll_viewer.Content = self.dataGrid
        Grid.SetRow(scroll_viewer, 1)
        main_grid.Children.Add(scroll_viewer)
        
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

        # Switch to Revit button
        switch_btn = Button()
        switch_btn.Content = "Switch to Revit"
        switch_btn.Width = 120
        switch_btn.Height = 30
        switch_btn.Margin = System.Windows.Thickness(5)
        switch_btn.Click += RoutedEventHandler(self.SwitchToRevit)
        button_panel.Children.Add(switch_btn)

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

        # Warning Message
        col1 = DataGridTextColumn()
        col1.Header = "Fehlermeldung"
        col1.Binding = Binding("Message")
        col1.Width = DataGridLength(350)
        self.dataGrid.Columns.Add(col1)

        # Element Count
        col2 = DataGridTextColumn()
        col2.Header = "Anzahl"
        col2.Binding = Binding("ElementCount")
        col2.Width = DataGridLength(70)
        self.dataGrid.Columns.Add(col2)

        # Element IDs
        col3 = DataGridTextColumn()
        col3.Header = "Element IDs"
        col3.Binding = Binding("ElementIds")
        col3.Width = DataGridLength(200)
        self.dataGrid.Columns.Add(col3)

        # Element Names
        col4 = DataGridTextColumn()
        col4.Header = "Elementnamen"
        col4.Binding = Binding("ElementNames")
        col4.Width = DataGridLength(250)
        self.dataGrid.Columns.Add(col4)

        # Levels
        col5 = DataGridTextColumn()
        col5.Header = "Ebenen"
        col5.Binding = Binding("Levels")
        col5.Width = DataGridLength(150)
        self.dataGrid.Columns.Add(col5)

        # Views
        col6 = DataGridTextColumn()
        col6.Header = "Ansichten"
        col6.Binding = Binding("Views")
        col6.Width = DataGridLength(200)
        self.dataGrid.Columns.Add(col6)

        # Categories
        col7 = DataGridTextColumn()
        col7.Header = "Kategorien"
        col7.Binding = Binding("Categories")
        col7.Width = DataGridLength(150)
        self.dataGrid.Columns.Add(col7)
    
    def LoadWarnings(self):
        """Load warnings from the document"""
        warnings_collection = ObservableCollection[object]()
        
        try:
            warnings = self.doc.GetWarnings()
            
            for warning in warnings:
                message = warning.GetDescriptionText()
                element_ids = list(warning.GetFailingElements())
                
                if not element_ids:
                    continue
                
                # Get element information
                element_info = []
                for elem_id in element_ids:
                    info = self.GetElementInfo(elem_id)
                    element_info.append(info)
                
                warning_item = WarningItem(message, element_ids, element_info)
                warnings_collection.Add(warning_item)
            
            self.dataGrid.ItemsSource = warnings_collection
            
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
        """Setup element highlighting graphics"""
        self.current_override = OverrideGraphicSettings()
        
        # Red highlight color
        red_color = RevitColor(255, 0, 0)
        self.current_override.SetProjectionLineColor(red_color)
        self.current_override.SetCutLineColor(red_color)
        self.current_override.SetSurfaceTransparency(30)
    
    def OnSelectionChanged(self, sender, e):
        """Handle selection change in the grid"""
        if self.autoHighlightCb.IsChecked:
            self.HighlightElements(sender, e)

    def OnWindowClosing(self, sender, e):
        """Clear all overrides when window is closing"""
        script.get_logger().debug("Window closing - clearing all overrides")
        self.ClearHighlights(None, None)

    def SwitchToRevit(self, sender, e):
        """Hide window temporarily to work in Revit, keep highlights active"""
        script.get_logger().debug("Switching to Revit - hiding window")
        self.Hide()

        # Show a task dialog to bring the window back
        from Autodesk.Revit.UI import TaskDialog, TaskDialogCommonButtons, TaskDialogResult

        dialog = TaskDialog("Warnings Browser")
        dialog.MainInstruction = "Warnings Browser ist minimiert"
        dialog.MainContent = "Sie können jetzt in Revit arbeiten. Die markierten Elemente bleiben rot.\n\nKlicken Sie auf 'Zurück zum Browser' um das Warnings Browser Fenster wieder anzuzeigen."
        dialog.CommonButtons = TaskDialogCommonButtons.Close
        dialog.DefaultButton = TaskDialogResult.Close

        # Add command link to restore window
        dialog.AddCommandLink(TaskDialogResult.CommandLink1, "Zurück zum Browser")

        result = dialog.Show()

        if result == TaskDialogResult.CommandLink1:
            # Restore the window
            self.Show()
            self.Activate()
            script.get_logger().debug("Window restored")

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

        # Clear previous highlights first
        self.ClearHighlights(None, None)

        # Perform highlighting in a transaction
        t = Transaction(self.doc, "Highlight Warning Elements")

        try:
            status = t.Start()
            if status != TransactionStatus.Started:
                forms.alert("Konnte Transaktion nicht starten")
                return

            active_view = self.uidoc.ActiveView
            self.highlighted_elements = []

            for elem_id in all_element_ids:
                try:
                    element = self.doc.GetElement(elem_id)
                    if element:
                        try:
                            if not element.IsHidden(active_view):
                                active_view.SetElementOverrides(elem_id, self.current_override)
                                self.highlighted_elements.append(elem_id)
                        except:
                            # Element might not be visible in current view
                            pass
                except Exception as ex:
                    script.get_logger().error("Error highlighting element {}: {}".format(elem_id.IntegerValue, str(ex)))

            t.Commit()
            script.get_logger().debug("Highlighted {} elements (visible in current view)".format(
                len(self.highlighted_elements)))

        except Exception as ex:
            if t.HasStarted() and not t.HasEnded():
                t.RollBack()
            forms.alert("Fehler beim Markieren: {}".format(str(ex)))
            script.get_logger().error("Error in highlight operation: {}".format(str(ex)))
    
    def ClearHighlights(self, sender, e):
        """Clear all element highlights"""
        script.get_logger().debug("ClearHighlights called")

        if not self.highlighted_elements:
            script.get_logger().debug("No elements to clear")
            return

        t = Transaction(self.doc, "Clear Highlights")

        try:
            status = t.Start()
            if status != TransactionStatus.Started:
                script.get_logger().error("Could not start clear transaction")
                return

            active_view = self.uidoc.ActiveView
            cleared_count = 0

            for elem_id in self.highlighted_elements:
                try:
                    active_view.SetElementOverrides(elem_id, OverrideGraphicSettings())
                    cleared_count += 1
                except:
                    pass

            self.highlighted_elements = []
            t.Commit()
            script.get_logger().debug("Cleared {} element highlights".format(cleared_count))

        except Exception as ex:
            if t.HasStarted() and not t.HasEnded():
                t.RollBack()
            script.get_logger().error("Error clearing highlights: {}".format(str(ex)))
    
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
        """Zoom to selected elements - supports multiple selection (may switch views)"""
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
        # Show the warnings browser window as modeless
        window = WarningsBrowserWindow()
        window.Show()  # Modeless - allows switching to Revit

except Exception as ex:
    forms.alert("Fehler beim Starten: {}".format(str(ex)))
    script.get_logger().error("Error in Enhanced Warnings Browser: {}".format(str(ex)))
