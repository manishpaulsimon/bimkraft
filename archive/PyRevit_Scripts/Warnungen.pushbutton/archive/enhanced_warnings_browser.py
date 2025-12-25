"""
Enhanced Warnings Browser for Revit
Displays warnings with additional columns and provides element highlighting in views
"""

import clr
clr.AddReference('PresentationCore')
clr.AddReference('PresentationFramework')
clr.AddReference('WindowsBase')
clr.AddReference('System.Windows.Forms')

from System.Windows import Window, Application
from System.Windows.Controls import (
    Grid, DataGrid, DataGridTextColumn, Button, StackPanel, 
    CheckBox, TextBlock, ComboBox
)
from System.Windows.Data import Binding
from System.Windows.Markup import XamlReader
from System.Windows.Media import Color, SolidColorBrush
from System.Collections.ObjectModel import ObservableCollection
from System import EventArgs
import System.Windows.Forms as WinForms

from Autodesk.Revit.DB import (
    Transaction, TransactionGroup, FailureMessage, 
    OverrideGraphicSettings, ElementId, Color as RevitColor,
    FillPatternElement, LinePatternElement, View, ViewType
)

from pyrevit import revit, DB, UI
from pyrevit import script
from pyrevit import forms

# Global variables for element highlighting
highlighted_elements = []
current_override = None

class WarningData:
    """Data class to hold warning information"""
    def __init__(self, warning_msg, element_ids, element_names, levels, views, categories):
        self.WarningMessage = warning_msg
        self.ElementCount = len(element_ids)
        self.ElementIds = "; ".join([str(id.IntegerValue) for id in element_ids])
        self.ElementNames = "; ".join(element_names)
        self.Levels = "; ".join(levels)
        self.Views = "; ".join(views)
        self.Categories = "; ".join(categories)
        self.ElementIdList = element_ids  # Keep for highlighting

class EnhancedWarningsBrowser(Window):
    """Main window for the enhanced warnings browser"""
    
    def __init__(self):
        self.doc = revit.doc
        self.uidoc = revit.uidoc
        self.app = revit.app
        
        # Initialize UI
        self.InitializeUI()
        
        # Load warnings data
        self.LoadWarningsData()
        
        # Setup highlighting
        self.setup_override_graphics()
    
    def InitializeUI(self):
        """Initialize the WPF user interface"""
        
        # Create main grid
        main_grid = Grid()
        
        # Define rows
        from System.Windows import GridLength, GridUnitType
        from System.Windows.Controls import RowDefinition, ColumnDefinition
        
        # Row definitions
        row1 = RowDefinition()
        row1.Height = GridLength(30)  # Header row
        row2 = RowDefinition()
        row2.Height = GridLength(1, GridUnitType.Star)  # Data grid
        row3 = RowDefinition()
        row3.Height = GridLength(50)  # Button row
        
        main_grid.RowDefinitions.Add(row1)
        main_grid.RowDefinitions.Add(row2)
        main_grid.RowDefinitions.Add(row3)
        
        # Header panel
        header_panel = StackPanel()
        header_panel.Orientation = System.Windows.Controls.Orientation.Horizontal
        header_panel.Margin = System.Windows.Thickness(10)
        
        header_text = TextBlock()
        header_text.Text = "Enhanced Warnings Browser - ICL Ingenieur Consult"
        header_text.FontWeight = System.Windows.FontWeights.Bold
        header_text.FontSize = 14
        header_panel.Children.Add(header_text)
        
        Grid.SetRow(header_panel, 0)
        main_grid.Children.Add(header_panel)
        
        # Create DataGrid
        self.warnings_grid = DataGrid()
        self.warnings_grid.AutoGenerateColumns = False
        self.warnings_grid.CanUserAddRows = False
        self.warnings_grid.CanUserDeleteRows = False
        self.warnings_grid.IsReadOnly = True
        self.warnings_grid.SelectionMode = System.Windows.Controls.DataGridSelectionMode.Single
        self.warnings_grid.Margin = System.Windows.Thickness(10)
        
        # Define columns
        self.create_columns()
        
        # Add selection changed event
        self.warnings_grid.SelectionChanged += self.on_selection_changed
        
        Grid.SetRow(self.warnings_grid, 1)
        main_grid.Children.Add(self.warnings_grid)
        
        # Bottom button panel
        button_panel = StackPanel()
        button_panel.Orientation = System.Windows.Controls.Orientation.Horizontal
        button_panel.HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        button_panel.Margin = System.Windows.Thickness(10)
        
        # Highlight button
        self.highlight_btn = Button()
        self.highlight_btn.Content = "Highlight Selected"
        self.highlight_btn.Width = 120
        self.highlight_btn.Height = 30
        self.highlight_btn.Margin = System.Windows.Thickness(5)
        self.highlight_btn.Click += self.highlight_elements
        button_panel.Children.Add(self.highlight_btn)
        
        # Clear highlights button
        self.clear_btn = Button()
        self.clear_btn.Content = "Clear Highlights"
        self.clear_btn.Width = 120
        self.clear_btn.Height = 30
        self.clear_btn.Margin = System.Windows.Thickness(5)
        self.clear_btn.Click += self.clear_highlights
        button_panel.Children.Add(self.clear_btn)
        
        # Export button
        self.export_btn = Button()
        self.export_btn.Content = "Export to HTML"
        self.export_btn.Width = 120
        self.export_btn.Height = 30
        self.export_btn.Margin = System.Windows.Thickness(5)
        self.export_btn.Click += self.export_to_html
        button_panel.Children.Add(self.export_btn)
        
        # Auto highlight checkbox
        self.auto_highlight_cb = CheckBox()
        self.auto_highlight_cb.Content = "Auto-highlight on selection"
        self.auto_highlight_cb.IsChecked = True
        self.auto_highlight_cb.Margin = System.Windows.Thickness(10, 5, 5, 5)
        button_panel.Children.Add(self.auto_highlight_cb)
        
        Grid.SetRow(button_panel, 2)
        main_grid.Children.Add(button_panel)
        
        # Set window properties
        self.Content = main_grid
        self.Title = "Enhanced Warnings Browser - ICL"
        self.Width = 1200
        self.Height = 600
        self.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen
    
    def create_columns(self):
        """Create DataGrid columns"""
        
        # Warning Message column
        col1 = DataGridTextColumn()
        col1.Header = "Fehlermeldung"
        col1.Binding = Binding("WarningMessage")
        col1.Width = 300
        self.warnings_grid.Columns.Add(col1)
        
        # Element Count column
        col2 = DataGridTextColumn()
        col2.Header = "Anzahl Elemente"
        col2.Binding = Binding("ElementCount")
        col2.Width = 100
        self.warnings_grid.Columns.Add(col2)
        
        # Element IDs column
        col3 = DataGridTextColumn()
        col3.Header = "Element IDs"
        col3.Binding = Binding("ElementIds")
        col3.Width = 200
        self.warnings_grid.Columns.Add(col3)
        
        # Element Names column
        col4 = DataGridTextColumn()
        col4.Header = "Elementnamen"
        col4.Binding = Binding("ElementNames")
        col4.Width = 200
        self.warnings_grid.Columns.Add(col4)
        
        # Levels column
        col5 = DataGridTextColumn()
        col5.Header = "Ebenen"
        col5.Binding = Binding("Levels")
        col5.Width = 150
        self.warnings_grid.Columns.Add(col5)
        
        # Views column
        col6 = DataGridTextColumn()
        col6.Header = "Ansichten"
        col6.Binding = Binding("Views")
        col6.Width = 150
        self.warnings_grid.Columns.Add(col6)
        
        # Categories column
        col7 = DataGridTextColumn()
        col7.Header = "Kategorien"
        col7.Binding = Binding("Categories")
        col7.Width = 150
        self.warnings_grid.Columns.Add(col7)
    
    def setup_override_graphics(self):
        """Setup override graphics for highlighting"""
        self.current_override = OverrideGraphicSettings()
        
        # Set red color for highlighting
        red_color = RevitColor(255, 0, 0)  # Red
        self.current_override.SetProjectionLineColor(red_color)
        self.current_override.SetSurfaceTransparency(30)  # 30% transparency
        
        # Try to set fill pattern if available
        try:
            solid_fill = FillPatternElement.GetFillPatternElementByName(self.doc, "<Solid fill>")
            if solid_fill:
                self.current_override.SetProjectionFillPatternId(solid_fill.Id)
                self.current_override.SetProjectionFillColor(red_color)
        except:
            pass
    
    def LoadWarningsData(self):
        """Load warnings data from Revit"""
        warnings_data = ObservableCollection[WarningData]()
        
        try:
            # Get all warnings
            warnings = self.doc.GetWarnings()
            
            for warning in warnings:
                warning_msg = warning.GetDescriptionText()
                element_ids = list(warning.GetFailingElements())
                
                if not element_ids:
                    continue
                
                # Get element information
                element_names = []
                levels = []
                views = []
                categories = []
                
                for elem_id in element_ids:
                    try:
                        element = self.doc.GetElement(elem_id)
                        if element:
                            # Element name
                            name = getattr(element, 'Name', str(elem_id.IntegerValue))
                            element_names.append(name if name else "Unnamed")
                            
                            # Level information
                            level_name = "N/A"
                            if hasattr(element, 'LevelId') and element.LevelId != ElementId.InvalidElementId:
                                level_elem = self.doc.GetElement(element.LevelId)
                                if level_elem:
                                    level_name = level_elem.Name
                            elif hasattr(element, 'Level') and element.Level:
                                level_name = element.Level.Name
                            levels.append(level_name)
                            
                            # Category
                            cat_name = "N/A"
                            if element.Category:
                                cat_name = element.Category.Name
                            categories.append(cat_name)
                            
                            # Views where element is visible
                            element_views = self.get_element_views(element)
                            views.extend(element_views)
                        else:
                            element_names.append("Deleted Element")
                            levels.append("N/A")
                            categories.append("N/A")
                    except Exception as e:
                        element_names.append("Error: " + str(e))
                        levels.append("N/A")
                        categories.append("N/A")
                
                # Remove duplicates from views
                unique_views = list(set(views))
                
                # Create warning data object
                warning_data = WarningData(
                    warning_msg, 
                    element_ids, 
                    element_names, 
                    levels, 
                    unique_views, 
                    categories
                )
                warnings_data.Add(warning_data)
            
            self.warnings_grid.ItemsSource = warnings_data
            
        except Exception as e:
            forms.alert("Error loading warnings: " + str(e), exitscript=True)
    
    def get_element_views(self, element):
        """Get views where the element is visible"""
        views = []
        
        try:
            # Get all views in the document
            all_views = DB.FilteredElementCollector(self.doc)\
                         .OfClass(DB.View)\
                         .WhereElementIsNotElementType()\
                         .ToElements()
            
            for view in all_views:
                try:
                    # Skip template views and sheets
                    if view.IsTemplate or view.ViewType == ViewType.DrawingSheet:
                        continue
                    
                    # Check if element is visible in this view
                    if element.IsHidden(view):
                        continue
                    
                    # Check if element's category is visible in view
                    if element.Category and not view.GetCategoryHidden(element.Category.Id):
                        views.append(view.Name)
                        
                except Exception:
                    # Skip views that cause errors
                    continue
                    
        except Exception as e:
            print("Error getting element views: " + str(e))
        
        return views[:5]  # Limit to first 5 views to avoid clutter
    
    def on_selection_changed(self, sender, e):
        """Handle selection changed event"""
        if self.auto_highlight_cb.IsChecked:
            self.highlight_elements(sender, e)
    
    def highlight_elements(self, sender, e):
        """Highlight selected elements in views"""
        selected_item = self.warnings_grid.SelectedItem
        if not selected_item:
            return
        
        try:
            # Clear previous highlights
            self.clear_highlights(None, None)
            
            # Start transaction
            with Transaction(self.doc, "Highlight Warning Elements") as t:
                t.Start()
                
                # Get active view
                active_view = self.uidoc.ActiveView
                
                # Highlight elements
                global highlighted_elements
                highlighted_elements = selected_item.ElementIdList
                
                for elem_id in highlighted_elements:
                    try:
                        element = self.doc.GetElement(elem_id)
                        if element and not element.IsHidden(active_view):
                            active_view.SetElementOverrides(elem_id, self.current_override)
                    except Exception as ex:
                        print("Error highlighting element {}: {}".format(elem_id.IntegerValue, str(ex)))
                
                t.Commit()
            
            # Zoom to elements if possible
            if highlighted_elements:
                try:
                    self.uidoc.Selection.SetElementIds(highlighted_elements)
                    self.uidoc.ShowElements(highlighted_elements)
                except:
                    pass
        
        except Exception as ex:
            forms.alert("Error highlighting elements: " + str(ex))
    
    def clear_highlights(self, sender, e):
        """Clear all element highlights"""
        try:
            global highlighted_elements
            if highlighted_elements:
                with Transaction(self.doc, "Clear Element Highlights") as t:
                    t.Start()
                    
                    active_view = self.uidoc.ActiveView
                    for elem_id in highlighted_elements:
                        try:
                            active_view.SetElementOverrides(elem_id, OverrideGraphicSettings())
                        except:
                            pass
                    
                    highlighted_elements = []
                    t.Commit()
        except Exception as ex:
            print("Error clearing highlights: " + str(ex))
    
    def export_to_html(self, sender, e):
        """Export warnings data to HTML file"""
        try:
            # Get save location
            save_dialog = WinForms.SaveFileDialog()
            save_dialog.Filter = "HTML files (*.html)|*.html"
            save_dialog.DefaultExt = "html"
            save_dialog.FileName = "Enhanced_Warnings_Report_{}.html".format(
                self.doc.Title.replace(" ", "_")
            )
            
            if save_dialog.ShowDialog() == WinForms.DialogResult.OK:
                self.create_html_report(save_dialog.FileName)
                forms.alert("Report exported successfully to:\n" + save_dialog.FileName)
        
        except Exception as ex:
            forms.alert("Error exporting report: " + str(ex))
    
    def create_html_report(self, file_path):
        """Create HTML report with all warning data"""
        html_content = """
<!DOCTYPE html>
<html>
<head>
    <title>Enhanced Warnings Report - {}</title>
    <meta charset="utf-8">
    <style>
        body {{ font-family: Arial, sans-serif; margin: 20px; }}
        h1 {{ color: #2c5aa0; text-align: center; }}
        .header {{ text-align: center; margin-bottom: 20px; }}
        .stats {{ background-color: #f5f5f5; padding: 10px; margin-bottom: 20px; border-radius: 5px; }}
        table {{ border-collapse: collapse; width: 100%; margin-top: 20px; }}
        th, td {{ border: 1px solid #ddd; padding: 8px; text-align: left; }}
        th {{ background-color: #2c5aa0; color: white; }}
        tr:nth-child(even) {{ background-color: #f9f9f9; }}
        tr:hover {{ background-color: #f5f5f5; }}
        .warning-msg {{ max-width: 300px; word-wrap: break-word; }}
        .element-ids {{ font-family: monospace; font-size: 0.9em; }}
        .footer {{ margin-top: 30px; text-align: center; color: #666; font-size: 0.9em; }}
    </style>
</head>
<body>
    <div class="header">
        <h1>Enhanced Warnings Report</h1>
        <p><strong>Project:</strong> {}</p>
        <p><strong>Generated:</strong> {}</p>
        <p><strong>Generated by:</strong> ICL Ingenieur Consult GmbH - Enhanced Warnings Browser</p>
    </div>
    
    <div class="stats">
        <p><strong>Total Warnings:</strong> {}</p>
        <p><strong>Total Affected Elements:</strong> {}</p>
    </div>
    
    <table>
        <thead>
            <tr>
                <th>Nr.</th>
                <th>Fehlermeldung</th>
                <th>Anzahl Elemente</th>
                <th>Element IDs</th>
                <th>Elementnamen</th>
                <th>Ebenen</th>
                <th>Ansichten</th>
                <th>Kategorien</th>
            </tr>
        </thead>
        <tbody>
            {}
        </tbody>
    </table>
    
    <div class="footer">
        <p>Generated by PyRevit Enhanced Warnings Browser</p>
        <p>ICL Ingenieur Consult GmbH, Leipzig</p>
    </div>
</body>
</html>
        """
        
        # Generate table rows
        rows_html = ""
        total_elements = 0
        warning_count = 0
        
        for item in self.warnings_grid.ItemsSource:
            warning_count += 1
            total_elements += item.ElementCount
            
            rows_html += "<tr>"
            rows_html += "<td>{}</td>".format(warning_count)
            rows_html += "<td class='warning-msg'>{}</td>".format(self.escape_html(item.WarningMessage))
            rows_html += "<td>{}</td>".format(item.ElementCount)
            rows_html += "<td class='element-ids'>{}</td>".format(self.escape_html(item.ElementIds))
            rows_html += "<td>{}</td>".format(self.escape_html(item.ElementNames))
            rows_html += "<td>{}</td>".format(self.escape_html(item.Levels))
            rows_html += "<td>{}</td>".format(self.escape_html(item.Views))
            rows_html += "<td>{}</td>".format(self.escape_html(item.Categories))
            rows_html += "</tr>"
        
        # Get current time
        from System import DateTime
        current_time = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")
        
        # Format final HTML
        final_html = html_content.format(
            self.doc.Title,
            self.doc.Title,
            current_time,
            warning_count,
            total_elements,
            rows_html
        )
        
        # Write to file
        with open(file_path, 'w', encoding='utf-8') as f:
            f.write(final_html)
    
    def escape_html(self, text):
        """Escape HTML characters"""
        if not text:
            return ""
        return str(text).replace("&", "&amp;").replace("<", "&lt;").replace(">", "&gt;")

def main():
    """Main function to launch the enhanced warnings browser"""
    try:
        # Check if document is available
        if not revit.doc:
            forms.alert("No active Revit document found.", exitscript=True)
        
        # Create and show the window
        window = EnhancedWarningsBrowser()
        window.ShowDialog()
        
    except Exception as e:
        forms.alert("Error launching Enhanced Warnings Browser: " + str(e))
    finally:
        # Clean up highlights on exit
        try:
            global highlighted_elements
            if highlighted_elements:
                with Transaction(revit.doc, "Cleanup Highlights") as t:
                    t.Start()
                    active_view = revit.uidoc.ActiveView
                    for elem_id in highlighted_elements:
                        try:
                            active_view.SetElementOverrides(elem_id, OverrideGraphicSettings())
                        except:
                            pass
                    t.Commit()
        except:
            pass

if __name__ == "__main__":
    main()
