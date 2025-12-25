# -*- coding: utf-8 -*-
__title__ = 'Batch Rename\nSystem Types'
__author__ = 'Your Company'
__doc__ = 'Batch rename system family types by duplicating with new names'

from pyrevit import forms, revit, DB, script
from System.Collections.ObjectModel import ObservableCollection
from System.Windows import Window, Thickness, GridLength, GridUnitType, HorizontalAlignment
from System.Windows.Controls import (
    Grid, StackPanel, Button, TextBox, CheckBox,
    DataGrid, DataGridTextColumn, ComboBox, DataGridCheckBoxColumn,
    RowDefinition, ColumnDefinition, Label, ComboBoxItem,
    DataGridLength, DataGridLengthUnitType, Orientation
)
from System.Windows.Data import Binding
import System.Windows.Media as Media

# Get current document
doc = revit.doc


class TypeRenameItem(forms.Reactive):
    """Reactive item for data binding"""
    def __init__(self, element_type, old_name, new_name):
        self.element_type = element_type
        self._old_name = old_name
        self._new_name = new_name
        self._selected = True
    
    @forms.reactive
    def old_name(self):
        return self._old_name
    
    @old_name.setter
    def old_name(self, value):
        self._old_name = value
    
    @forms.reactive
    def new_name(self):
        return self._new_name
    
    @new_name.setter
    def new_name(self, value):
        self._new_name = value
    
    @forms.reactive
    def selected(self):
        return self._selected
    
    @selected.setter
    def selected(self, value):
        self._selected = value


def get_element_name(element):
    """Get name from element using various methods"""
    try:
        # Try direct Name property
        if hasattr(element, 'Name'):
            return element.Name
    except:
        pass
    
    try:
        # Try getting name parameter
        name_param = element.get_Parameter(DB.BuiltInParameter.SYMBOL_NAME_PARAM)
        if name_param:
            return name_param.AsString()
    except:
        pass
    
    try:
        # Try ALL_MODEL_TYPE_NAME
        name_param = element.get_Parameter(DB.BuiltInParameter.ALL_MODEL_TYPE_NAME)
        if name_param:
            return name_param.AsString()
    except:
        pass
    
    try:
        # Try ELEM_TYPE_PARAM
        name_param = element.get_Parameter(DB.BuiltInParameter.ELEM_TYPE_PARAM)
        if name_param:
            return name_param.AsString()
    except:
        pass
    
    # Last resort - use element Id
    return "Type_{}".format(element.Id)


def test_get_all_system_types():
    """Test function to see what system types exist in the document"""
    print("\n=== TESTING SYSTEM TYPES IN DOCUMENT ===")
    
    # Test Walls
    print("\nWALL TYPES:")
    wall_types = DB.FilteredElementCollector(doc).OfClass(DB.WallType)
    count = 0
    for wt in wall_types:
        try:
            name = get_element_name(wt)
            print("  - {} (ID: {})".format(name, wt.Id))
            count += 1
            if count >= 5:
                break
        except Exception as e:
            print("  - Error: {}".format(e))
    print("  Total wall types: {}".format(wall_types.GetElementCount()))
    
    # Test Floors
    print("\nFLOOR TYPES:")
    floor_types = DB.FilteredElementCollector(doc).OfClass(DB.FloorType)
    count = 0
    for ft in floor_types:
        try:
            name = get_element_name(ft)
            print("  - {} (ID: {})".format(name, ft.Id))
            count += 1
            if count >= 5:
                break
        except Exception as e:
            print("  - Error: {}".format(e))
    print("  Total floor types: {}".format(floor_types.GetElementCount()))
    
    # Test Roofs
    print("\nROOF TYPES:")
    roof_types = DB.FilteredElementCollector(doc).OfClass(DB.RoofType)
    count = 0
    for rt in roof_types:
        try:
            name = get_element_name(rt)
            print("  - {} (ID: {})".format(name, rt.Id))
            count += 1
            if count >= 5:
                break
        except Exception as e:
            print("  - Error: {}".format(e))
    print("  Total roof types: {}".format(roof_types.GetElementCount()))
    
    print("\n=== END TEST ===\n")


def get_system_family_categories():
    """Get list of system family categories"""
    categories = {
        'Floors': DB.BuiltInCategory.OST_Floors,
        'Walls': DB.BuiltInCategory.OST_Walls,
        'Ceilings': DB.BuiltInCategory.OST_Ceilings,
        'Roofs': DB.BuiltInCategory.OST_Roofs,
        'Stairs': DB.BuiltInCategory.OST_Stairs,
        'Railings': DB.BuiltInCategory.OST_StairsRailing,
    }
    return categories


def get_types_for_category(category_bic):
    """Get all types for a given category"""
    types = []
    
    # Use ElementType class for system families
    if category_bic == DB.BuiltInCategory.OST_Walls:
        # Get all WallType elements
        collector = DB.FilteredElementCollector(doc).OfClass(DB.WallType)
        types = list(collector)
    
    elif category_bic == DB.BuiltInCategory.OST_Floors:
        # Get all FloorType elements  
        collector = DB.FilteredElementCollector(doc).OfClass(DB.FloorType)
        types = list(collector)
    
    elif category_bic == DB.BuiltInCategory.OST_Ceilings:
        # Get all CeilingType elements
        collector = DB.FilteredElementCollector(doc).OfClass(DB.CeilingType)
        types = list(collector)
    
    elif category_bic == DB.BuiltInCategory.OST_Roofs:
        # Get all RoofType elements
        collector = DB.FilteredElementCollector(doc).OfClass(DB.RoofType)
        types = list(collector)
    
    elif category_bic == DB.BuiltInCategory.OST_Stairs:
        # Stairs might be different - use category approach
        collector = DB.FilteredElementCollector(doc)\
                      .OfCategory(category_bic)\
                      .WhereElementIsElementType()
        types = list(collector)
    
    elif category_bic == DB.BuiltInCategory.OST_StairsRailing:
        # Get railing types
        collector = DB.FilteredElementCollector(doc)\
                      .OfCategory(category_bic)\
                      .WhereElementIsElementType()
        types = list(collector)
    
    # Debug output
    print("Found {} types for category".format(len(types)))
    for t in types[:5]:  # Print first 5 types for debugging
        try:
            name = get_element_name(t)
            print("  - Type: {} (ID: {})".format(name, t.Id))
        except:
            print("  - Type with no retrievable name")
    
    return types


def generate_new_name(old_name, prefix="", suffix="", find="", replace=""):
    """Generate new name based on naming convention"""
    new_name = old_name
    
    # Find and replace
    if find and replace is not None:
        new_name = new_name.replace(find, replace)
    
    # Add prefix
    if prefix:
        new_name = prefix + new_name
    
    # Add suffix
    if suffix:
        new_name = new_name + suffix
    
    return new_name


def duplicate_and_rename_type(elem_type, new_name):
    """
    Duplicate a system type with a new name.
    Returns the new type if successful, None otherwise.
    """
    try:
        # Check if the element type has a Duplicate method
        if hasattr(elem_type, 'Duplicate'):
            # Use Duplicate method which accepts a name parameter
            new_type = elem_type.Duplicate(new_name)
            if new_type:
                print("Successfully duplicated: {} -> {}".format(get_element_name(elem_type), new_name))
            return new_type
        else:
            print("Type does not have Duplicate method: {}".format(get_element_name(elem_type)))
            return None
    except Exception as e:
        print("Failed to duplicate {}: {}".format(get_element_name(elem_type), str(e)))
        return None


def type_has_instances(elem_type):
    """Check if a type has any instances in the model"""
    try:
        # Get the category from the type
        category_id = elem_type.Category.Id
        
        # Create a collector for instances of this category
        collector = DB.FilteredElementCollector(doc)\
                      .OfCategoryId(category_id)\
                      .WhereElementIsNotElementType()
        
        # Check each instance to see if it uses this type
        for elem in collector:
            try:
                # Try to get the type ID from the element
                type_id = elem.GetTypeId()
                if type_id and type_id == elem_type.Id:
                    return True
            except:
                pass
        
        return False
    except Exception as e:
        print("Error checking instances for type {}: {}".format(get_element_name(elem_type), str(e)))
        # If we can't check, assume it has instances to be safe
        return True


class RenameTypesWindow(Window):
    """Main window for renaming types"""
    
    def __init__(self):
        self.Title = 'Batch Rename System Family Types'
        self.Width = 1000
        self.Height = 700
        
        # Data
        self.items = ObservableCollection[object]()
        self.selected_category = None
        self.categories = get_system_family_categories()
        
        # Build UI
        self.build_ui()
        
    def build_ui(self):
        """Build the user interface"""
        main_grid = Grid()
        
        # Row definitions
        main_grid.RowDefinitions.Add(RowDefinition(Height=GridLength(1, GridUnitType.Auto)))
        main_grid.RowDefinitions.Add(RowDefinition(Height=GridLength(1, GridUnitType.Auto)))
        main_grid.RowDefinitions.Add(RowDefinition(Height=GridLength(1, GridUnitType.Auto)))
        main_grid.RowDefinitions.Add(RowDefinition(Height=GridLength(1, GridUnitType.Star)))
        main_grid.RowDefinitions.Add(RowDefinition(Height=GridLength(1, GridUnitType.Auto)))
        
        # Category selection panel
        category_panel = self.create_category_panel()
        Grid.SetRow(category_panel, 0)
        main_grid.Children.Add(category_panel)
        
        # Naming convention panel
        naming_panel = self.create_naming_panel()
        Grid.SetRow(naming_panel, 1)
        main_grid.Children.Add(naming_panel)
        
        # Options panel
        options_panel = self.create_options_panel()
        Grid.SetRow(options_panel, 2)
        main_grid.Children.Add(options_panel)
        
        # Data grid
        self.data_grid = self.create_data_grid()
        Grid.SetRow(self.data_grid, 3)
        main_grid.Children.Add(self.data_grid)
        
        # Button panel
        button_panel = self.create_button_panel()
        Grid.SetRow(button_panel, 4)
        main_grid.Children.Add(button_panel)
        
        self.Content = main_grid
    
    def create_category_panel(self):
        """Create category selection panel"""
        panel = StackPanel()
        panel.Margin = Thickness(10)
        
        label = Label()
        label.Content = "Select System Family Category:"
        panel.Children.Add(label)
        
        self.category_combo = ComboBox()
        self.category_combo.Width = 300
        self.category_combo.HorizontalAlignment = HorizontalAlignment.Left
        
        for cat_name in sorted(self.categories.keys()):
            item = ComboBoxItem()
            item.Content = cat_name
            item.Tag = cat_name
            self.category_combo.Items.Add(item)
        
        self.category_combo.SelectionChanged += self.on_category_changed
        panel.Children.Add(self.category_combo)
        
        # Info label
        info_label = Label()
        info_label.Content = "Note: System types will be duplicated with new names (API limitation)"
        info_label.Foreground = Media.Brushes.Gray
        info_label.Margin = Thickness(0, 5, 0, 0)
        panel.Children.Add(info_label)
        
        return panel
    
    def create_naming_panel(self):
        """Create naming convention input panel"""
        panel = StackPanel()
        panel.Margin = Thickness(10)
        
        # Grid for inputs
        input_grid = Grid()
        input_grid.ColumnDefinitions.Add(ColumnDefinition(Width=GridLength(1, GridUnitType.Auto)))
        input_grid.ColumnDefinitions.Add(ColumnDefinition(Width=GridLength(200, GridUnitType.Pixel)))
        input_grid.ColumnDefinitions.Add(ColumnDefinition(Width=GridLength(1, GridUnitType.Auto)))
        input_grid.ColumnDefinitions.Add(ColumnDefinition(Width=GridLength(200, GridUnitType.Pixel)))
        
        # Prefix
        prefix_label = Label()
        prefix_label.Content = "Prefix:"
        prefix_label.Margin = Thickness(0, 0, 5, 0)
        Grid.SetColumn(prefix_label, 0)
        Grid.SetRow(prefix_label, 0)
        input_grid.Children.Add(prefix_label)
        
        self.prefix_textbox = TextBox()
        self.prefix_textbox.Margin = Thickness(0, 0, 20, 0)
        Grid.SetColumn(self.prefix_textbox, 1)
        Grid.SetRow(self.prefix_textbox, 0)
        input_grid.Children.Add(self.prefix_textbox)
        
        # Suffix
        suffix_label = Label()
        suffix_label.Content = "Suffix:"
        suffix_label.Margin = Thickness(0, 0, 5, 0)
        Grid.SetColumn(suffix_label, 2)
        Grid.SetRow(suffix_label, 0)
        input_grid.Children.Add(suffix_label)
        
        self.suffix_textbox = TextBox()
        Grid.SetColumn(self.suffix_textbox, 3)
        Grid.SetRow(self.suffix_textbox, 0)
        input_grid.Children.Add(self.suffix_textbox)
        
        # Find/Replace
        input_grid.RowDefinitions.Add(RowDefinition(Height=GridLength(1, GridUnitType.Auto)))
        input_grid.RowDefinitions.Add(RowDefinition(Height=GridLength(1, GridUnitType.Auto)))
        
        find_label = Label()
        find_label.Content = "Find:"
        find_label.Margin = Thickness(0, 5, 5, 0)
        Grid.SetColumn(find_label, 0)
        Grid.SetRow(find_label, 1)
        input_grid.Children.Add(find_label)
        
        self.find_textbox = TextBox()
        self.find_textbox.Margin = Thickness(0, 5, 20, 0)
        Grid.SetColumn(self.find_textbox, 1)
        Grid.SetRow(self.find_textbox, 1)
        input_grid.Children.Add(self.find_textbox)
        
        replace_label = Label()
        replace_label.Content = "Replace:"
        replace_label.Margin = Thickness(0, 5, 5, 0)
        Grid.SetColumn(replace_label, 2)
        Grid.SetRow(replace_label, 1)
        input_grid.Children.Add(replace_label)
        
        self.replace_textbox = TextBox()
        self.replace_textbox.Margin = Thickness(0, 5, 0, 0)
        Grid.SetColumn(self.replace_textbox, 3)
        Grid.SetRow(self.replace_textbox, 1)
        input_grid.Children.Add(self.replace_textbox)
        
        panel.Children.Add(input_grid)
        
        # Apply naming button
        apply_naming_btn = Button()
        apply_naming_btn.Content = "Apply Naming Convention"
        apply_naming_btn.Width = 200
        apply_naming_btn.Margin = Thickness(0, 10, 0, 0)
        apply_naming_btn.HorizontalAlignment = HorizontalAlignment.Left
        apply_naming_btn.Click += self.on_apply_naming
        panel.Children.Add(apply_naming_btn)
        
        return panel
    
    def create_options_panel(self):
        """Create options panel"""
        panel = StackPanel()
        panel.Margin = Thickness(10, 5, 10, 5)
        
        self.delete_original_checkbox = CheckBox()
        self.delete_original_checkbox.Content = "Delete original types after duplication (only if no instances exist)"
        self.delete_original_checkbox.IsChecked = False
        panel.Children.Add(self.delete_original_checkbox)
        
        return panel
    
    def create_data_grid(self):
        """Create data grid for displaying types"""
        grid = DataGrid()
        grid.Margin = Thickness(10)
        grid.AutoGenerateColumns = False
        grid.CanUserAddRows = False
        grid.ItemsSource = self.items
        
        # Checkbox column
        check_col = DataGridCheckBoxColumn()
        check_col.Header = "Include"
        check_col.Width = DataGridLength(60, DataGridLengthUnitType.Pixel)
        check_col.Binding = Binding("selected")
        grid.Columns.Add(check_col)
        
        # Old Name column (read-only)
        old_name_col = DataGridTextColumn()
        old_name_col.Header = "Current Name"
        old_name_col.Width = DataGridLength(1, DataGridLengthUnitType.Star)
        old_name_col.IsReadOnly = True
        old_name_col.Binding = Binding("old_name")
        grid.Columns.Add(old_name_col)
        
        # New Name column (editable)
        new_name_col = DataGridTextColumn()
        new_name_col.Header = "New Name (Editable)"
        new_name_col.Width = DataGridLength(1, DataGridLengthUnitType.Star)
        new_name_col.IsReadOnly = False
        new_name_col.Binding = Binding("new_name")
        grid.Columns.Add(new_name_col)
        
        return grid
    
    def create_button_panel(self):
        """Create button panel"""
        panel = StackPanel()
        panel.Orientation = Orientation.Horizontal
        panel.Margin = Thickness(10)
        panel.HorizontalAlignment = HorizontalAlignment.Right
        
        # Select All button
        select_all_btn = Button()
        select_all_btn.Content = "Select All"
        select_all_btn.Width = 100
        select_all_btn.Margin = Thickness(0, 0, 10, 0)
        select_all_btn.Click += self.on_select_all
        panel.Children.Add(select_all_btn)
        
        # Select None button
        select_none_btn = Button()
        select_none_btn.Content = "Select None"
        select_none_btn.Width = 100
        select_none_btn.Margin = Thickness(0, 0, 10, 0)
        select_none_btn.Click += self.on_select_none
        panel.Children.Add(select_none_btn)
        
        # Apply button
        apply_btn = Button()
        apply_btn.Content = "Apply Rename"
        apply_btn.Width = 120
        apply_btn.Margin = Thickness(0, 0, 10, 0)
        apply_btn.Click += self.on_apply_rename
        panel.Children.Add(apply_btn)
        
        # Cancel button
        cancel_btn = Button()
        cancel_btn.Content = "Cancel"
        cancel_btn.Width = 120
        cancel_btn.Click += self.on_cancel
        panel.Children.Add(cancel_btn)
        
        return panel
    
    def on_category_changed(self, sender, args):
        """Handle category selection change"""
        if self.category_combo.SelectedItem:
            cat_name = self.category_combo.SelectedItem.Tag
            self.selected_category = self.categories[cat_name]
            
            # Debug output
            print("Selected category: {}".format(cat_name))
            print("Category ID: {}".format(self.selected_category))
            
            self.load_types()
    
    def load_types(self):
        """Load types for selected category"""
        self.items.Clear()
        
        if not self.selected_category:
            return
        
        types = get_types_for_category(self.selected_category)
        
        if not types:
            # Show a message if no types found
            forms.alert("No types found for the selected category.", exitscript=False)
            return
        
        for elem_type in types:
            try:
                type_name = get_element_name(elem_type)
                if type_name:  # Only add if name exists
                    item = TypeRenameItem(elem_type, type_name, type_name)
                    self.items.Add(item)
            except Exception as e:
                # Skip types that can't be processed
                print("Error loading type: {}".format(e))
                continue
        
        # If still no items after processing
        if self.items.Count == 0:
            forms.alert("Could not load any types for the selected category.", exitscript=False)
    
    def on_apply_naming(self, sender, args):
        """Apply naming convention to all types"""
        prefix = self.prefix_textbox.Text
        suffix = self.suffix_textbox.Text
        find = self.find_textbox.Text
        replace = self.replace_textbox.Text
        
        for item in self.items:
            new_name = generate_new_name(
                item.old_name, 
                prefix=prefix, 
                suffix=suffix,
                find=find,
                replace=replace
            )
            item.new_name = new_name
    
    def on_select_all(self, sender, args):
        """Select all items"""
        for item in self.items:
            item.selected = True
    
    def on_select_none(self, sender, args):
        """Deselect all items"""
        for item in self.items:
            item.selected = False
    
    def on_apply_rename(self, sender, args):
        """Apply the renaming by duplicating types"""
        # Filter selected items
        selected_items = [item for item in self.items if item.selected]
        
        # Validate
        if not selected_items:
            forms.alert("No types selected for renaming.", exitscript=False)
            return
        
        # Check for items where old name equals new name
        items_to_process = [item for item in selected_items if item.old_name != item.new_name]
        if not items_to_process:
            forms.alert("No types have new names different from their current names.", exitscript=False)
            return
        
        # Check for duplicates in new names
        new_names = [item.new_name for item in items_to_process]
        if len(new_names) != len(set(new_names)):
            forms.alert("Duplicate names detected. Please ensure all new names are unique.", exitscript=False)
            return
        
        # Check for existing type names
        existing_types = get_types_for_category(self.selected_category)
        existing_names = set([get_element_name(t) for t in existing_types])
        conflicts = [name for name in new_names if name in existing_names]
        if conflicts:
            forms.alert(
                "The following names already exist:\n{}\n\nPlease choose different names.".format(
                    "\n".join(conflicts[:10])  # Show max 10 conflicts
                ),
                exitscript=False
            )
            return
        
        # Confirm
        message = "This will duplicate {} types with new names.".format(len(items_to_process))
        if self.delete_original_checkbox.IsChecked:
            message += "\n\nOriginal types will be deleted if they have no instances."
        
        result = forms.alert(message, yes=True, no=True)
        
        if not result:
            return
        
        # Perform rename
        success_count = 0
        failed_items = []
        deleted_count = 0
        cannot_delete = []
        
        print("\n=== STARTING BATCH RENAME PROCESS ===")
        
        with revit.Transaction("Batch Rename System Types"):
            for item in items_to_process:
                print("\nProcessing: {} -> {}".format(item.old_name, item.new_name))
                
                try:
                    # Duplicate the type with new name
                    new_type = duplicate_and_rename_type(item.element_type, item.new_name)
                    
                    if new_type:
                        success_count += 1
                        
                        # Try to delete original if option is checked
                        if self.delete_original_checkbox.IsChecked:
                            has_instances = type_has_instances(item.element_type)
                            
                            if not has_instances:
                                try:
                                    doc.Delete(item.element_type.Id)
                                    deleted_count += 1
                                    print("  - Deleted original type")
                                except Exception as e:
                                    cannot_delete.append("{} (error: {})".format(item.old_name, str(e)))
                                    print("  - Could not delete: {}".format(e))
                            else:
                                cannot_delete.append("{} (has instances)".format(item.old_name))
                                print("  - Type has instances, cannot delete")
                    else:
                        failed_items.append("{}: Could not duplicate".format(item.old_name))
                        print("  - Failed to duplicate")
                        
                except Exception as e:
                    failed_items.append("{}: {}".format(item.old_name, str(e)))
                    print("  - Error: {}".format(e))
        
        print("\n=== BATCH RENAME PROCESS COMPLETE ===\n")
        
        # Show results
        message = "Process Complete:\n\n"
        message += "✓ Successfully created {} new types".format(success_count)
        
        if self.delete_original_checkbox.IsChecked:
            message += "\n✓ Deleted {} original types".format(deleted_count)
            
            if cannot_delete:
                message += "\n\n⚠ Could not delete {} original types:".format(len(cannot_delete))
                for item in cannot_delete[:5]:
                    message += "\n  - {}".format(item)
                if len(cannot_delete) > 5:
                    message += "\n  ... and {} more".format(len(cannot_delete) - 5)
        
        if failed_items:
            message += "\n\n✗ Failed to process {} types:".format(len(failed_items))
            for item in failed_items[:5]:
                message += "\n  - {}".format(item)
            if len(failed_items) > 5:
                message += "\n  ... and {} more".format(len(failed_items) - 5)
        
        forms.alert(message, exitscript=False)
        
        if success_count > 0:
            # Reload the types to show current state
            self.load_types()
    
    def on_cancel(self, sender, args):
        """Cancel and close"""
        self.DialogResult = False
        self.Close()


# Main execution
if __name__ == '__main__':
    # Run test to see what's available
    test_get_all_system_types()
    
    # Show the window
    window = RenameTypesWindow()
    window.ShowDialog()