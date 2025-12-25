# -*- coding: utf-8 -*-
__title__ = 'Rename Loadable\nFamilies'
__author__ = 'ICL Tools'
__doc__ = 'Rename loadable families based on a naming convention (e.g., Skelettbau families)'

from pyrevit import forms, revit, DB, script
import System
from System.Collections.ObjectModel import ObservableCollection
from System.Windows import Window, Thickness, GridLength, GridUnitType, HorizontalAlignment
from System.Windows.Controls import (
    Grid, StackPanel, Button, TextBox, CheckBox,
    DataGrid, DataGridTextColumn, ComboBox, DataGridCheckBoxColumn,
    RowDefinition, ColumnDefinition, Label, ComboBoxItem,
    DataGridLength, DataGridLengthUnitType, Orientation, ScrollViewer
)
from System.Windows.Data import Binding
import System.Windows.Media as Media

# Get current document
doc = revit.doc


class FamilyRenameItem(forms.Reactive):
    """Reactive item for data binding"""
    def __init__(self, family_symbol, old_name, new_name, category_name=""):
        self.family_symbol = family_symbol
        self._old_name = old_name
        self._new_name = new_name
        self._selected = True
        self._category_name = category_name

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

    @forms.reactive
    def category_name(self):
        return self._category_name

    @category_name.setter
    def category_name(self, value):
        self._category_name = value


def get_all_loadable_families():
    """Get all loadable families in the project"""
    families_dict = {}

    # Collect all families using FilteredElementCollector
    collector = DB.FilteredElementCollector(doc)\
                  .OfClass(DB.Family)

    for family in collector:
        try:
            # Skip system families (we only want loadable families)
            if hasattr(family, 'IsSystemFamily') and family.IsSystemFamily:
                continue

            # Get family name
            family_name = family.Name

            # Get category name
            category_name = ""
            if hasattr(family, 'FamilyCategory') and family.FamilyCategory:
                category_name = family.FamilyCategory.Name

            # Store unique families only
            if family_name not in families_dict:
                families_dict[family_name] = {
                    'family': family,
                    'name': family_name,
                    'category': category_name
                }

        except Exception as e:
            print("Error processing family {}: {}".format(family.Id, str(e)))
            continue

    return list(families_dict.values())


def get_naming_convention_mappings():
    """
    Define the naming convention mappings based on the provided image.
    This maps the Block 4 codes to their full descriptions.
    """
    return {
        # Structural steel types
        'HFT': 'Stahlbeton - Halbfertigteil',
        'VFT': 'Stahlbeton - Vollfertigteil',
        'STB': 'Stahlbeton - Ortbeton',
        'FLS': 'Flachstahl',
        'HEA': 'Stahl - HEA',
        'HEB': 'Stahl - HEB',
        'HEM': 'Stahl - HEM',
        'IPE': 'Stahl - IPE',
        'IPN': 'Stahl - IPN',
        'KHP': 'Stahl - Kreishohlprofil',
        'L': 'Stahl - L-Winkel',
        'RHS': 'Stahl - RHS',
        'RO': 'Stahl - RO',
        'SHS': 'Stahl - SHS',
        'T': 'Stahl - T Profile',
        'UPE': 'Stahl - UPE',
        'UPN': 'Stahl - UPN',
        'ZGL': 'Ziegel',
        'BSH': 'Brettschichtholz',
        'KVH': 'Konstruktionsvollholz',
        'XXX': 'Ohne Zuordnung',
    }


def extract_material_code(family_name):
    """
    Try to extract material code from family name.
    Examples:
    - "HEA Träger" -> "HEA"
    - "IPE750 Träger" -> "IPE"
    - "CC Träger doppelt" -> None (no standard code)
    """
    material_codes = get_naming_convention_mappings().keys()

    for code in material_codes:
        if code in family_name.upper():
            return code

    return None


def generate_skelettbau_name(old_name, bauteil='TR', lage='X', material_code=None):
    """
    Generate a new name based on Skelettbau naming convention.
    Format: [Bauteil]_[Lage]_[Material/Art]

    Examples:
    - TR_I_HEA (Träger, Innen, HEA)
    - TR_X_XXX (Träger, Ohne Zuordnung, Ohne Zuordnung)
    """
    # If material code not provided, try to extract from old name
    if not material_code:
        material_code = extract_material_code(old_name)
        if not material_code:
            material_code = 'XXX'

    # Build the new name with underscores
    new_name = "{}_{}_{}.rfa".format(bauteil, lage, material_code)

    return new_name


def rename_family(family, new_name):
    """
    Rename a family.
    Returns True if successful, False otherwise.
    """
    try:
        # Check if name is different
        if family.Name == new_name:
            print("Family already has the target name: {}".format(new_name))
            return True

        # Try to rename
        family.Name = new_name
        print("Successfully renamed: {} -> {}".format(family.Name, new_name))
        return True

    except Exception as e:
        print("Failed to rename {}: {}".format(family.Name, str(e)))
        return False


class RenameFamiliesWindow(Window):
    """Main window for renaming loadable families"""

    def __init__(self):
        self.Title = 'Rename Loadable Families - Skelettbau Convention'
        self.Width = 1200
        self.Height = 700

        # Data
        self.items = ObservableCollection[object]()
        self.all_families = []

        # Build UI
        self.build_ui()

        # Load families
        self.load_families()

    def build_ui(self):
        """Build the user interface"""
        main_grid = Grid()

        # Row definitions
        main_grid.RowDefinitions.Add(RowDefinition(Height=GridLength(1, GridUnitType.Auto)))
        main_grid.RowDefinitions.Add(RowDefinition(Height=GridLength(1, GridUnitType.Auto)))
        main_grid.RowDefinitions.Add(RowDefinition(Height=GridLength(1, GridUnitType.Star)))
        main_grid.RowDefinitions.Add(RowDefinition(Height=GridLength(1, GridUnitType.Auto)))

        # Info panel
        info_panel = self.create_info_panel()
        Grid.SetRow(info_panel, 0)
        main_grid.Children.Add(info_panel)

        # Naming convention panel
        naming_panel = self.create_naming_panel()
        Grid.SetRow(naming_panel, 1)
        main_grid.Children.Add(naming_panel)

        # Data grid
        self.data_grid = self.create_data_grid()
        Grid.SetRow(self.data_grid, 2)
        main_grid.Children.Add(self.data_grid)

        # Button panel
        button_panel = self.create_button_panel()
        Grid.SetRow(button_panel, 3)
        main_grid.Children.Add(button_panel)

        self.Content = main_grid

    def create_info_panel(self):
        """Create info panel"""
        panel = StackPanel()
        panel.Margin = Thickness(10)

        title_label = Label()
        title_label.Content = "Skelettbau Family Naming Convention"
        title_label.FontSize = 14
        title_label.FontWeight = System.Windows.FontWeights.Bold
        panel.Children.Add(title_label)

        info_label = Label()
        info_label.Content = "Format: [Bauteil]_[Lage]_[Hauptmaterial/Art].rfa"
        info_label.Foreground = Media.Brushes.Gray
        panel.Children.Add(info_label)

        # Examples
        example_panel = StackPanel()
        example_panel.Orientation = Orientation.Horizontal
        example_panel.Margin = Thickness(0, 5, 0, 0)

        example_label = Label()
        example_label.Content = "Examples:"
        example_label.FontWeight = System.Windows.FontWeights.Bold
        example_panel.Children.Add(example_label)

        examples_text = Label()
        examples_text.Content = "TR_I_HEA.rfa (Träger, Innen, HEA) | TR_X_XXX.rfa (Träger, Ohne Zuordnung)"
        examples_text.Foreground = Media.Brushes.DarkBlue
        example_panel.Children.Add(examples_text)

        panel.Children.Add(example_panel)

        return panel

    def create_naming_panel(self):
        """Create naming convention input panel"""
        panel = StackPanel()
        panel.Margin = Thickness(10, 5, 10, 10)

        # Grid for inputs
        input_grid = Grid()
        input_grid.ColumnDefinitions.Add(ColumnDefinition(Width=GridLength(1, GridUnitType.Auto)))
        input_grid.ColumnDefinitions.Add(ColumnDefinition(Width=GridLength(150, GridUnitType.Pixel)))
        input_grid.ColumnDefinitions.Add(ColumnDefinition(Width=GridLength(1, GridUnitType.Auto)))
        input_grid.ColumnDefinitions.Add(ColumnDefinition(Width=GridLength(150, GridUnitType.Pixel)))
        input_grid.ColumnDefinitions.Add(ColumnDefinition(Width=GridLength(1, GridUnitType.Star)))

        # Bauteil (Component type)
        bauteil_label = Label()
        bauteil_label.Content = "Bauteil:"
        bauteil_label.Margin = Thickness(0, 0, 5, 0)
        Grid.SetColumn(bauteil_label, 0)
        input_grid.Children.Add(bauteil_label)

        self.bauteil_combo = ComboBox()
        self.bauteil_combo.Width = 150
        self.bauteil_combo.Margin = Thickness(0, 0, 20, 0)
        Grid.SetColumn(self.bauteil_combo, 1)

        # Add common component types
        for code in ['TR', 'OZ', 'UZ', 'BA', 'FB', 'PL']:
            item = ComboBoxItem()
            item.Content = code
            self.bauteil_combo.Items.Add(item)

        self.bauteil_combo.SelectedIndex = 0
        input_grid.Children.Add(self.bauteil_combo)

        # Lage (Location)
        lage_label = Label()
        lage_label.Content = "Lage:"
        lage_label.Margin = Thickness(0, 0, 5, 0)
        Grid.SetColumn(lage_label, 2)
        input_grid.Children.Add(lage_label)

        self.lage_combo = ComboBox()
        self.lage_combo.Width = 150
        Grid.SetColumn(self.lage_combo, 3)

        # Add location options
        for code in ['I', 'A', 'X']:
            item = ComboBoxItem()
            item.Content = code
            self.lage_combo.Items.Add(item)

        self.lage_combo.SelectedIndex = 2  # Default to X (Ohne Zuordnung)
        input_grid.Children.Add(self.lage_combo)

        panel.Children.Add(input_grid)

        # Buttons panel
        buttons_panel = StackPanel()
        buttons_panel.Orientation = Orientation.Horizontal
        buttons_panel.Margin = Thickness(0, 10, 0, 0)

        # Apply naming button
        apply_naming_btn = Button()
        apply_naming_btn.Content = "Apply Convention to Selected"
        apply_naming_btn.Width = 200
        apply_naming_btn.Margin = Thickness(0, 0, 10, 0)
        apply_naming_btn.Click += self.on_apply_naming
        buttons_panel.Children.Add(apply_naming_btn)

        # Auto-detect button
        auto_detect_btn = Button()
        auto_detect_btn.Content = "Auto-Detect Material Codes"
        auto_detect_btn.Width = 200
        auto_detect_btn.Click += self.on_auto_detect
        buttons_panel.Children.Add(auto_detect_btn)

        panel.Children.Add(buttons_panel)

        # Filter panel - Category filter
        category_filter_panel = StackPanel()
        category_filter_panel.Orientation = Orientation.Horizontal
        category_filter_panel.Margin = Thickness(0, 10, 0, 0)

        category_filter_label = Label()
        category_filter_label.Content = "Filter by category:"
        category_filter_panel.Children.Add(category_filter_label)

        self.category_filter_combo = ComboBox()
        self.category_filter_combo.Width = 200
        self.category_filter_combo.Margin = Thickness(5, 0, 10, 0)

        # Add "All Categories" option
        all_item = ComboBoxItem()
        all_item.Content = "All Categories"
        all_item.Tag = None
        self.category_filter_combo.Items.Add(all_item)
        self.category_filter_combo.SelectedIndex = 0

        category_filter_panel.Children.Add(self.category_filter_combo)

        category_filter_btn = Button()
        category_filter_btn.Content = "Apply Category Filter"
        category_filter_btn.Width = 150
        category_filter_btn.Click += self.on_apply_category_filter
        category_filter_panel.Children.Add(category_filter_btn)

        panel.Children.Add(category_filter_panel)

        # Filter panel - Name filter
        filter_panel = StackPanel()
        filter_panel.Orientation = Orientation.Horizontal
        filter_panel.Margin = Thickness(0, 10, 0, 0)

        filter_label = Label()
        filter_label.Content = "Filter by name:"
        filter_panel.Children.Add(filter_label)

        self.filter_textbox = TextBox()
        self.filter_textbox.Width = 200
        self.filter_textbox.Margin = Thickness(5, 0, 10, 0)
        filter_panel.Children.Add(self.filter_textbox)

        filter_btn = Button()
        filter_btn.Content = "Apply Name Filter"
        filter_btn.Width = 130
        filter_btn.Click += self.on_apply_filter
        filter_panel.Children.Add(filter_btn)

        clear_filter_btn = Button()
        clear_filter_btn.Content = "Clear All Filters"
        clear_filter_btn.Width = 120
        clear_filter_btn.Margin = Thickness(5, 0, 0, 0)
        clear_filter_btn.Click += self.on_clear_filter
        filter_panel.Children.Add(clear_filter_btn)

        panel.Children.Add(filter_panel)

        return panel

    def create_data_grid(self):
        """Create data grid for displaying families"""
        grid = DataGrid()
        grid.Margin = Thickness(10, 0, 10, 0)
        grid.AutoGenerateColumns = False
        grid.CanUserAddRows = False
        grid.ItemsSource = self.items

        # Checkbox column
        check_col = DataGridCheckBoxColumn()
        check_col.Header = "Include"
        check_col.Width = DataGridLength(60, DataGridLengthUnitType.Pixel)
        check_col.Binding = Binding("selected")
        grid.Columns.Add(check_col)

        # Category column
        category_col = DataGridTextColumn()
        category_col.Header = "Category"
        category_col.Width = DataGridLength(150, DataGridLengthUnitType.Pixel)
        category_col.IsReadOnly = True
        category_col.Binding = Binding("category_name")
        grid.Columns.Add(category_col)

        # Old Name column (read-only)
        old_name_col = DataGridTextColumn()
        old_name_col.Header = "Current Family Name"
        old_name_col.Width = DataGridLength(1, DataGridLengthUnitType.Star)
        old_name_col.IsReadOnly = True
        old_name_col.Binding = Binding("old_name")
        grid.Columns.Add(old_name_col)

        # New Name column (editable)
        new_name_col = DataGridTextColumn()
        new_name_col.Header = "New Family Name (Editable)"
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
        apply_btn.Content = "Rename Families"
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

    def load_families(self):
        """Load all loadable families"""
        self.items.Clear()

        print("\n=== LOADING LOADABLE FAMILIES ===")
        self.all_families = get_all_loadable_families()
        print("Found {} loadable families".format(len(self.all_families)))

        # Collect unique categories
        categories = set()
        for fam_data in self.all_families:
            if fam_data['category']:
                categories.add(fam_data['category'])

        # Populate category filter dropdown (keep "All Categories" at index 0)
        # Clear existing items except the first one
        while self.category_filter_combo.Items.Count > 1:
            self.category_filter_combo.Items.RemoveAt(1)

        for category in sorted(categories):
            item = ComboBoxItem()
            item.Content = category
            item.Tag = category
            self.category_filter_combo.Items.Add(item)

        for fam_data in self.all_families:
            try:
                item = FamilyRenameItem(
                    fam_data['family'],
                    fam_data['name'],
                    fam_data['name'],
                    fam_data['category']
                )
                self.items.Add(item)
            except Exception as e:
                print("Error adding family {}: {}".format(fam_data['name'], str(e)))
                continue

        print("Loaded {} families into grid".format(self.items.Count))

    def on_apply_naming(self, sender, args):
        """Apply naming convention to selected families"""
        bauteil = self.bauteil_combo.SelectedItem.Content if self.bauteil_combo.SelectedItem else 'TR'
        lage = self.lage_combo.SelectedItem.Content if self.lage_combo.SelectedItem else 'X'

        changed_count = 0
        for item in self.items:
            if item.selected:
                # Extract material code from old name
                material_code = extract_material_code(item.old_name)

                # Generate new name
                new_name = generate_skelettbau_name(
                    item.old_name,
                    bauteil=bauteil,
                    lage=lage,
                    material_code=material_code
                )

                if new_name != item.new_name:
                    item.new_name = new_name
                    changed_count += 1

        if changed_count > 0:
            forms.alert("Updated {} family names with the convention.".format(changed_count), exitscript=False)
        else:
            forms.alert("No changes made. All names already match the convention.", exitscript=False)

    def on_auto_detect(self, sender, args):
        """Auto-detect material codes and suggest names"""
        changed_count = 0
        for item in self.items:
            # Try to extract material code
            material_code = extract_material_code(item.old_name)

            if material_code:
                # Use default bauteil and lage
                new_name = generate_skelettbau_name(
                    item.old_name,
                    bauteil='TR',
                    lage='X',
                    material_code=material_code
                )

                if new_name != item.new_name:
                    item.new_name = new_name
                    changed_count += 1

        if changed_count > 0:
            forms.alert("Auto-detected material codes for {} families.".format(changed_count), exitscript=False)
        else:
            forms.alert("No material codes detected in family names.", exitscript=False)

    def on_apply_category_filter(self, sender, args):
        """Apply category filter to families"""
        if not self.category_filter_combo.SelectedItem:
            return

        selected_category = self.category_filter_combo.SelectedItem.Tag

        self.items.Clear()

        # If "All Categories" is selected, show all families
        if selected_category is None:
            for fam_data in self.all_families:
                item = FamilyRenameItem(
                    fam_data['family'],
                    fam_data['name'],
                    fam_data['name'],
                    fam_data['category']
                )
                self.items.Add(item)
        else:
            # Filter by selected category
            for fam_data in self.all_families:
                if fam_data['category'] == selected_category:
                    item = FamilyRenameItem(
                        fam_data['family'],
                        fam_data['name'],
                        fam_data['name'],
                        fam_data['category']
                    )
                    self.items.Add(item)

        forms.alert("Category filter applied. Showing {} families.".format(self.items.Count), exitscript=False)

    def on_apply_filter(self, sender, args):
        """Apply filter to families"""
        filter_text = self.filter_textbox.Text.strip().lower()

        if not filter_text:
            forms.alert("Please enter a filter text.", exitscript=False)
            return

        self.items.Clear()

        for fam_data in self.all_families:
            if filter_text in fam_data['name'].lower():
                item = FamilyRenameItem(
                    fam_data['family'],
                    fam_data['name'],
                    fam_data['name'],
                    fam_data['category']
                )
                self.items.Add(item)

        forms.alert("Filter applied. Showing {} families.".format(self.items.Count), exitscript=False)

    def on_clear_filter(self, sender, args):
        """Clear all filters and show all families"""
        self.filter_textbox.Text = ""
        self.category_filter_combo.SelectedIndex = 0  # Reset to "All Categories"
        self.load_families()

    def on_select_all(self, sender, args):
        """Select all items"""
        for item in self.items:
            item.selected = True

    def on_select_none(self, sender, args):
        """Deselect all items"""
        for item in self.items:
            item.selected = False

    def on_apply_rename(self, sender, args):
        """Apply the renaming"""
        # Filter selected items
        selected_items = [item for item in self.items if item.selected]

        # Validate
        if not selected_items:
            forms.alert("No families selected for renaming.", exitscript=False)
            return

        # Check for items where old name equals new name
        items_to_process = [item for item in selected_items if item.old_name != item.new_name]
        if not items_to_process:
            forms.alert("No families have new names different from their current names.", exitscript=False)
            return

        # Check for duplicates in new names
        new_names = [item.new_name for item in items_to_process]
        if len(new_names) != len(set(new_names)):
            forms.alert("Duplicate names detected. Please ensure all new names are unique.", exitscript=False)
            return

        # Check for existing family names
        all_family_names = [fam['name'] for fam in self.all_families]
        # Remove the names we're about to change
        for item in items_to_process:
            if item.old_name in all_family_names:
                all_family_names.remove(item.old_name)

        conflicts = [name for name in new_names if name in all_family_names]
        if conflicts:
            forms.alert(
                "The following names already exist:\n{}\n\nPlease choose different names.".format(
                    "\n".join(conflicts[:10])
                ),
                exitscript=False
            )
            return

        # Confirm
        message = "This will rename {} families.".format(len(items_to_process))
        result = forms.alert(message, yes=True, no=True)

        if not result:
            return

        # Perform rename
        success_count = 0
        failed_items = []

        print("\n=== STARTING FAMILY RENAME PROCESS ===")

        with revit.Transaction("Rename Loadable Families"):
            for item in items_to_process:
                print("\nRenaming: {} -> {}".format(item.old_name, item.new_name))

                try:
                    # Get the family (it's already stored in the item)
                    family = item.family_symbol

                    # Rename the family
                    if rename_family(family, item.new_name):
                        success_count += 1
                    else:
                        failed_items.append(item.old_name)

                except Exception as e:
                    failed_items.append("{}: {}".format(item.old_name, str(e)))
                    print("  - Error: {}".format(e))

        print("\n=== FAMILY RENAME PROCESS COMPLETE ===\n")

        # Show results
        message = "Process Complete:\n\n"
        message += "✓ Successfully renamed {} families".format(success_count)

        if failed_items:
            message += "\n\n✗ Failed to rename {} families:".format(len(failed_items))
            for item in failed_items[:5]:
                message += "\n  - {}".format(item)
            if len(failed_items) > 5:
                message += "\n  ... and {} more".format(len(failed_items) - 5)

        forms.alert(message, exitscript=False)

        if success_count > 0:
            # Reload the families to show current state
            self.load_families()

    def on_cancel(self, sender, args):
        """Cancel and close"""
        self.DialogResult = False
        self.Close()


# Main execution
if __name__ == '__main__':
    # Show the window
    window = RenameFamiliesWindow()
    window.ShowDialog()
