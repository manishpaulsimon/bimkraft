using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace BIMKraft.Windows
{
    public partial class FamilyRenamerWindow : Window
    {
        private readonly Document _doc;
        private ObservableCollection<RenameItem> _systemItems;
        private ObservableCollection<RenameItem> _loadableItems;
        private List<FamilyData> _allLoadableFamilies;
        private Dictionary<string, BuiltInCategory> _systemCategories;

        public FamilyRenamerWindow(Document doc)
        {
            InitializeComponent();
            _doc = doc;
            _systemItems = new ObservableCollection<RenameItem>();
            _loadableItems = new ObservableCollection<RenameItem>();
            _allLoadableFamilies = new List<FamilyData>();

            InitializeSystemCategories();
            InitializeBauteilComboBox();
            InitializeLageComboBox();

            SystemDataGrid.ItemsSource = _systemItems;
            LoadableDataGrid.ItemsSource = _loadableItems;
        }

        #region Initialization

        private void InitializeSystemCategories()
        {
            _systemCategories = new Dictionary<string, BuiltInCategory>
            {
                { "Walls", BuiltInCategory.OST_Walls },
                { "Floors", BuiltInCategory.OST_Floors },
                { "Ceilings", BuiltInCategory.OST_Ceilings },
                { "Roofs", BuiltInCategory.OST_Roofs },
                { "Stairs", BuiltInCategory.OST_Stairs },
                { "Railings", BuiltInCategory.OST_StairsRailing }
            };

            foreach (var category in _systemCategories.Keys.OrderBy(k => k))
            {
                SystemCategoryComboBox.Items.Add(category);
            }
        }

        private void InitializeBauteilComboBox()
        {
            string[] bauteile = { "TR", "OZ", "UZ", "BA", "FB", "PL" };
            foreach (var bauteil in bauteile)
            {
                BauteilComboBox.Items.Add(bauteil);
            }
            BauteilComboBox.SelectedIndex = 0;
        }

        private void InitializeLageComboBox()
        {
            string[] lagen = { "I", "A", "X" };
            foreach (var lage in lagen)
            {
                LageComboBox.Items.Add(lage);
            }
            LageComboBox.SelectedIndex = 2; // Default to X
        }

        #endregion

        #region System Families

        private void SystemCategoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SystemCategoryComboBox.SelectedItem == null) return;

            string selectedCategory = SystemCategoryComboBox.SelectedItem.ToString();
            if (_systemCategories.ContainsKey(selectedCategory))
            {
                LoadSystemTypes(_systemCategories[selectedCategory]);
            }
        }

        private void LoadSystemTypes(BuiltInCategory category)
        {
            _systemItems.Clear();

            try
            {
                List<ElementType> types = new List<ElementType>();

                if (category == BuiltInCategory.OST_Walls)
                {
                    types = new FilteredElementCollector(_doc).OfClass(typeof(WallType)).Cast<ElementType>().ToList();
                }
                else if (category == BuiltInCategory.OST_Floors)
                {
                    types = new FilteredElementCollector(_doc).OfClass(typeof(FloorType)).Cast<ElementType>().ToList();
                }
                else if (category == BuiltInCategory.OST_Ceilings)
                {
                    types = new FilteredElementCollector(_doc).OfClass(typeof(CeilingType)).Cast<ElementType>().ToList();
                }
                else if (category == BuiltInCategory.OST_Roofs)
                {
                    types = new FilteredElementCollector(_doc).OfClass(typeof(RoofType)).Cast<ElementType>().ToList();
                }
                else if (category == BuiltInCategory.OST_Stairs || category == BuiltInCategory.OST_StairsRailing)
                {
                    types = new FilteredElementCollector(_doc)
                        .OfCategory(category)
                        .WhereElementIsElementType()
                        .Cast<ElementType>()
                        .ToList();
                }

                foreach (var type in types)
                {
                    string typeName = type.Name;
                    _systemItems.Add(new RenameItem
                    {
                        Element = type,
                        OldName = typeName,
                        NewName = typeName,
                        Selected = true
                    });
                }

                TaskDialog.Show("Types Loaded", $"Loaded {_systemItems.Count} types.");
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", $"Failed to load system types:\n{ex.Message}");
            }
        }

        private void SystemApplyNamingButton_Click(object sender, RoutedEventArgs e)
        {
            string prefix = SystemPrefixTextBox.Text;
            string suffix = SystemSuffixTextBox.Text;
            string find = SystemFindTextBox.Text;
            string replace = SystemReplaceTextBox.Text;

            foreach (var item in _systemItems)
            {
                string newName = item.OldName;

                if (!string.IsNullOrEmpty(find))
                {
                    newName = newName.Replace(find, replace ?? "");
                }

                if (!string.IsNullOrEmpty(prefix))
                {
                    newName = prefix + newName;
                }

                if (!string.IsNullOrEmpty(suffix))
                {
                    newName = newName + suffix;
                }

                item.NewName = newName;
            }
        }

        private void SystemRenameButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = _systemItems.Where(i => i.Selected).ToList();

            if (!selectedItems.Any())
            {
                TaskDialog.Show("No Selection", "No types selected for renaming.");
                return;
            }

            var itemsToProcess = selectedItems.Where(i => i.OldName != i.NewName).ToList();
            if (!itemsToProcess.Any())
            {
                TaskDialog.Show("No Changes", "No types have new names different from their current names.");
                return;
            }

            // Check for duplicates
            var newNames = itemsToProcess.Select(i => i.NewName).ToList();
            if (newNames.Count != newNames.Distinct().Count())
            {
                TaskDialog.Show("Duplicate Names", "Duplicate names detected. Please ensure all new names are unique.");
                return;
            }

            // Confirm
            string message = $"This will duplicate {itemsToProcess.Count} types with new names.";
            if (DeleteOriginalCheckBox.IsChecked == true)
            {
                message += "\n\nOriginal types will be deleted if they have no instances.";
            }

            var result = TaskDialog.Show("Confirm", message, TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No);
            if (result != TaskDialogResult.Yes) return;

            // Process
            int successCount = 0;
            int deletedCount = 0;
            List<string> failedItems = new List<string>();

            using (Transaction trans = new Transaction(_doc, "Batch Rename System Types"))
            {
                trans.Start();

                foreach (var item in itemsToProcess)
                {
                    try
                    {
                        ElementType elemType = item.Element as ElementType;
                        if (elemType == null) continue;

                        // Duplicate with new name
                        ElementType newType = elemType.Duplicate(item.NewName) as ElementType;

                        if (newType != null)
                        {
                            successCount++;

                            // Try to delete original if requested
                            if (DeleteOriginalCheckBox.IsChecked == true)
                            {
                                if (!TypeHasInstances(elemType))
                                {
                                    try
                                    {
                                        _doc.Delete(elemType.Id);
                                        deletedCount++;
                                    }
                                    catch
                                    {
                                        // Could not delete
                                    }
                                }
                            }
                        }
                        else
                        {
                            failedItems.Add($"{item.OldName}: Could not duplicate");
                        }
                    }
                    catch (Exception ex)
                    {
                        failedItems.Add($"{item.OldName}: {ex.Message}");
                    }
                }

                trans.Commit();
            }

            // Show results
            string resultMessage = $"Process Complete:\n\n✓ Successfully created {successCount} new types";
            if (DeleteOriginalCheckBox.IsChecked == true)
            {
                resultMessage += $"\n✓ Deleted {deletedCount} original types";
            }

            if (failedItems.Any())
            {
                resultMessage += $"\n\n✗ Failed to process {failedItems.Count} types";
            }

            TaskDialog.Show("Results", resultMessage);

            // Reload
            if (SystemCategoryComboBox.SelectedItem != null)
            {
                string selectedCategory = SystemCategoryComboBox.SelectedItem.ToString();
                if (_systemCategories.ContainsKey(selectedCategory))
                {
                    LoadSystemTypes(_systemCategories[selectedCategory]);
                }
            }
        }

        private bool TypeHasInstances(ElementType elemType)
        {
            try
            {
                var collector = new FilteredElementCollector(_doc)
                    .OfCategoryId(elemType.Category.Id)
                    .WhereElementIsNotElementType();

                foreach (Element elem in collector)
                {
                    try
                    {
                        ElementId typeId = elem.GetTypeId();
                        if (typeId != null && typeId == elemType.Id)
                        {
                            return true;
                        }
                    }
                    catch { }
                }

                return false;
            }
            catch
            {
                return true; // Assume it has instances if we can't check
            }
        }

        #endregion

        #region Loadable Families

        private void MainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MainTabControl.SelectedItem == LoadableFamiliesTab && _loadableItems.Count == 0)
            {
                LoadLoadableFamilies();
            }
        }

        private void LoadLoadableFamilies()
        {
            _loadableItems.Clear();
            _allLoadableFamilies.Clear();

            try
            {
                var families = new FilteredElementCollector(_doc)
                    .OfClass(typeof(Family))
                    .Cast<Family>()
                    .Where(f => !f.IsEditable || !f.FamilyCategory.Name.Contains("System"))
                    .ToList();

                HashSet<string> addedFamilies = new HashSet<string>();
                HashSet<string> categories = new HashSet<string>();

                foreach (var family in families)
                {
                    try
                    {
                        string familyName = family.Name;
                        string categoryName = family.FamilyCategory?.Name ?? "";

                        if (addedFamilies.Contains(familyName)) continue;

                        _allLoadableFamilies.Add(new FamilyData
                        {
                            Family = family,
                            Name = familyName,
                            Category = categoryName
                        });

                        _loadableItems.Add(new RenameItem
                        {
                            Element = family,
                            OldName = familyName,
                            NewName = familyName,
                            Category = categoryName,
                            Selected = true
                        });

                        addedFamilies.Add(familyName);
                        if (!string.IsNullOrEmpty(categoryName))
                        {
                            categories.Add(categoryName);
                        }
                    }
                    catch { }
                }

                // Populate category filter
                LoadableCategoryComboBox.Items.Clear();
                LoadableCategoryComboBox.Items.Add("All Categories");
                foreach (var cat in categories.OrderBy(c => c))
                {
                    LoadableCategoryComboBox.Items.Add(cat);
                }
                LoadableCategoryComboBox.SelectedIndex = 0;

                TaskDialog.Show("Families Loaded", $"Loaded {_loadableItems.Count} loadable families.");
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", $"Failed to load loadable families:\n{ex.Message}");
            }
        }

        private void ApplySkelettbauButton_Click(object sender, RoutedEventArgs e)
        {
            string bauteil = BauteilComboBox.SelectedItem?.ToString() ?? "TR";
            string lage = LageComboBox.SelectedItem?.ToString() ?? "X";

            int changedCount = 0;
            foreach (var item in _loadableItems.Where(i => i.Selected))
            {
                string materialCode = ExtractMaterialCode(item.OldName);
                string newName = GenerateSkelettbauName(item.OldName, bauteil, lage, materialCode);

                if (newName != item.NewName)
                {
                    item.NewName = newName;
                    changedCount++;
                }
            }

            TaskDialog.Show("Convention Applied", $"Updated {changedCount} family names with the Skelettbau convention.");
        }

        private void AutoDetectButton_Click(object sender, RoutedEventArgs e)
        {
            int changedCount = 0;
            foreach (var item in _loadableItems)
            {
                string materialCode = ExtractMaterialCode(item.OldName);

                if (!string.IsNullOrEmpty(materialCode))
                {
                    string newName = GenerateSkelettbauName(item.OldName, "TR", "X", materialCode);

                    if (newName != item.NewName)
                    {
                        item.NewName = newName;
                        changedCount++;
                    }
                }
            }

            TaskDialog.Show("Auto-Detect Complete", $"Auto-detected material codes for {changedCount} families.");
        }

        private void ApplyCategoryFilterButton_Click(object sender, RoutedEventArgs e)
        {
            if (LoadableCategoryComboBox.SelectedItem == null) return;

            string selectedCategory = LoadableCategoryComboBox.SelectedItem.ToString();
            _loadableItems.Clear();

            if (selectedCategory == "All Categories")
            {
                foreach (var famData in _allLoadableFamilies)
                {
                    _loadableItems.Add(new RenameItem
                    {
                        Element = famData.Family,
                        OldName = famData.Name,
                        NewName = famData.Name,
                        Category = famData.Category,
                        Selected = true
                    });
                }
            }
            else
            {
                foreach (var famData in _allLoadableFamilies.Where(f => f.Category == selectedCategory))
                {
                    _loadableItems.Add(new RenameItem
                    {
                        Element = famData.Family,
                        OldName = famData.Name,
                        NewName = famData.Name,
                        Category = famData.Category,
                        Selected = true
                    });
                }
            }

            TaskDialog.Show("Filter Applied", $"Showing {_loadableItems.Count} families.");
        }

        private void ClearFilterButton_Click(object sender, RoutedEventArgs e)
        {
            LoadableCategoryComboBox.SelectedIndex = 0;
            LoadLoadableFamilies();
        }

        private void LoadableRenameButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = _loadableItems.Where(i => i.Selected).ToList();

            if (!selectedItems.Any())
            {
                TaskDialog.Show("No Selection", "No families selected for renaming.");
                return;
            }

            var itemsToProcess = selectedItems.Where(i => i.OldName != i.NewName).ToList();
            if (!itemsToProcess.Any())
            {
                TaskDialog.Show("No Changes", "No families have new names different from their current names.");
                return;
            }

            // Check for duplicates
            var newNames = itemsToProcess.Select(i => i.NewName).ToList();
            if (newNames.Count != newNames.Distinct().Count())
            {
                TaskDialog.Show("Duplicate Names", "Duplicate names detected. Please ensure all new names are unique.");
                return;
            }

            // Confirm
            var result = TaskDialog.Show("Confirm", $"This will rename {itemsToProcess.Count} families.",
                TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No);
            if (result != TaskDialogResult.Yes) return;

            // Process
            int successCount = 0;
            List<string> failedItems = new List<string>();

            using (Transaction trans = new Transaction(_doc, "Rename Loadable Families"))
            {
                trans.Start();

                foreach (var item in itemsToProcess)
                {
                    try
                    {
                        Family family = item.Element as Family;
                        if (family == null) continue;

                        family.Name = item.NewName;
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        failedItems.Add($"{item.OldName}: {ex.Message}");
                    }
                }

                trans.Commit();
            }

            // Show results
            string resultMessage = $"Process Complete:\n\n✓ Successfully renamed {successCount} families";

            if (failedItems.Any())
            {
                resultMessage += $"\n\n✗ Failed to rename {failedItems.Count} families";
            }

            TaskDialog.Show("Results", resultMessage);

            // Reload
            LoadLoadableFamilies();
        }

        #endregion

        #region Helper Methods

        private Dictionary<string, string> GetMaterialCodeMappings()
        {
            return new Dictionary<string, string>
            {
                { "HFT", "Stahlbeton - Halbfertigteil" },
                { "VFT", "Stahlbeton - Vollfertigteil" },
                { "STB", "Stahlbeton - Ortbeton" },
                { "FLS", "Flachstahl" },
                { "HEA", "Stahl - HEA" },
                { "HEB", "Stahl - HEB" },
                { "HEM", "Stahl - HEM" },
                { "IPE", "Stahl - IPE" },
                { "IPN", "Stahl - IPN" },
                { "KHP", "Stahl - Kreishohlprofil" },
                { "L", "Stahl - L-Winkel" },
                { "RHS", "Stahl - RHS" },
                { "RO", "Stahl - RO" },
                { "SHS", "Stahl - SHS" },
                { "T", "Stahl - T Profile" },
                { "UPE", "Stahl - UPE" },
                { "UPN", "Stahl - UPN" },
                { "ZGL", "Ziegel" },
                { "BSH", "Brettschichtholz" },
                { "KVH", "Konstruktionsvollholz" },
                { "XXX", "Ohne Zuordnung" }
            };
        }

        private string ExtractMaterialCode(string familyName)
        {
            var materialCodes = GetMaterialCodeMappings().Keys;

            foreach (var code in materialCodes)
            {
                if (familyName.ToUpper().Contains(code))
                {
                    return code;
                }
            }

            return null;
        }

        private string GenerateSkelettbauName(string oldName, string bauteil, string lage, string materialCode)
        {
            if (string.IsNullOrEmpty(materialCode))
            {
                materialCode = ExtractMaterialCode(oldName) ?? "XXX";
            }

            return $"{bauteil}_{lage}_{materialCode}.rfa";
        }

        #endregion

        #region Common Event Handlers

        private void SelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            if (MainTabControl.SelectedItem == SystemFamiliesTab)
            {
                foreach (var item in _systemItems)
                {
                    item.Selected = true;
                }
            }
            else
            {
                foreach (var item in _loadableItems)
                {
                    item.Selected = true;
                }
            }
        }

        private void SelectNoneButton_Click(object sender, RoutedEventArgs e)
        {
            if (MainTabControl.SelectedItem == SystemFamiliesTab)
            {
                foreach (var item in _systemItems)
                {
                    item.Selected = false;
                }
            }
            else
            {
                foreach (var item in _loadableItems)
                {
                    item.Selected = false;
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #endregion
    }

    #region Data Models

    public class RenameItem : INotifyPropertyChanged
    {
        private bool _selected;
        private string _newName;

        public Element Element { get; set; }
        public string OldName { get; set; }

        public string NewName
        {
            get => _newName;
            set
            {
                _newName = value;
                OnPropertyChanged(nameof(NewName));
            }
        }

        public string Category { get; set; }

        public bool Selected
        {
            get => _selected;
            set
            {
                _selected = value;
                OnPropertyChanged(nameof(Selected));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class FamilyData
    {
        public Family Family { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
    }

    #endregion
}
