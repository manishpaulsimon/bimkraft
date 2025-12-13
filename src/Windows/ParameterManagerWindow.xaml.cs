using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Autodesk.Revit.DB;
using BIMKraft.Models;
using Microsoft.Win32;
using Newtonsoft.Json;
using Color = System.Windows.Media.Color;

namespace BIMKraft.Windows
{
    public partial class ParameterManagerWindow : Window
    {
        private readonly Document _doc;
        private readonly Dictionary<string, ParameterConfig> _selectedParams;
        private readonly List<string> _selectionOrder;
        private readonly Dictionary<string, BuiltInCategory> _availableCategories;
#if REVIT2025
        private readonly Dictionary<string, ForgeTypeId> _parameterGroups;
#else
        private readonly Dictionary<string, BuiltInParameterGroup> _parameterGroups;
#endif
        private readonly Dictionary<string, CheckBox> _checkboxLookup;
        private readonly string _presetsDirectory;
        private List<GroupData> _allParametersData;
        private bool _isUpdatingUI;
        private string _lastClickedParameter; // Track last clicked parameter for shift-select

        public ParameterManagerWindow(Document doc)
        {
            InitializeComponent();

            _doc = doc;
            _selectedParams = new Dictionary<string, ParameterConfig>();
            _selectionOrder = new List<string>();
            _checkboxLookup = new Dictionary<string, CheckBox>();
            _allParametersData = new List<GroupData>();
            _isUpdatingUI = false;

            // Set up presets directory
            string assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string assemblyDir = Path.GetDirectoryName(assemblyLocation);
            _presetsDirectory = Path.Combine(assemblyDir, "ParameterPresets");

            if (!Directory.Exists(_presetsDirectory))
            {
                Directory.CreateDirectory(_presetsDirectory);
            }

            // Initialize category mappings
            _availableCategories = InitializeCategories();
            _parameterGroups = InitializeParameterGroups();

            // Load data
            LoadSharedParameters();
            LoadPresets();
            UpdateSelectionSummary();
        }

        #region Initialization

        private Dictionary<string, BuiltInCategory> InitializeCategories()
        {
            return new Dictionary<string, BuiltInCategory>
            {
                // Views and Sheets
                { "Views", BuiltInCategory.OST_Views },
                { "Sheets", BuiltInCategory.OST_Sheets },

                // Architectural
                { "Walls", BuiltInCategory.OST_Walls },
                { "Doors", BuiltInCategory.OST_Doors },
                { "Windows", BuiltInCategory.OST_Windows },
                { "Rooms", BuiltInCategory.OST_Rooms },
                { "Floors", BuiltInCategory.OST_Floors },
                { "Roofs", BuiltInCategory.OST_Roofs },
                { "Ceilings", BuiltInCategory.OST_Ceilings },
                { "Curtain Panels", BuiltInCategory.OST_CurtainWallPanels },
                { "Curtain Systems", BuiltInCategory.OST_Curtain_Systems },
                { "Curtain Wall Mullions", BuiltInCategory.OST_CurtainWallMullions },
                { "Railings", BuiltInCategory.OST_Railings },
                { "Ramps", BuiltInCategory.OST_Ramps },
                { "Stairs", BuiltInCategory.OST_Stairs },
                { "Columns", BuiltInCategory.OST_Columns },

                // Model Elements
                { "Generic Models", BuiltInCategory.OST_GenericModel },
                { "Mass", BuiltInCategory.OST_Mass },
                { "Casework", BuiltInCategory.OST_Casework },
                { "Furniture", BuiltInCategory.OST_Furniture },
                { "Furniture Systems", BuiltInCategory.OST_FurnitureSystems },
                { "Specialty Equipment", BuiltInCategory.OST_SpecialityEquipment },
                { "Planting", BuiltInCategory.OST_Planting },
                { "Parking", BuiltInCategory.OST_Parking },
                { "Site", BuiltInCategory.OST_Site },
                { "Topography", BuiltInCategory.OST_Topography },

                // Structural
                { "Structural Framing", BuiltInCategory.OST_StructuralFraming },
                { "Structural Columns", BuiltInCategory.OST_StructuralColumns },
                { "Structural Foundations", BuiltInCategory.OST_StructuralFoundation },
                { "Structural Connections", BuiltInCategory.OST_StructConnections },

                // MEP - Mechanical
                { "Spaces", BuiltInCategory.OST_MEPSpaces },
                { "Mechanical Equipment", BuiltInCategory.OST_MechanicalEquipment },
                { "Air Terminals", BuiltInCategory.OST_DuctTerminal },
                { "Ducts", BuiltInCategory.OST_DuctCurves },
                { "Duct Fittings", BuiltInCategory.OST_DuctFitting },
                { "Duct Accessories", BuiltInCategory.OST_DuctAccessory },
                { "Flex Ducts", BuiltInCategory.OST_FlexDuctCurves },

                // MEP - Plumbing
                { "Plumbing Fixtures", BuiltInCategory.OST_PlumbingFixtures },
                { "Pipes", BuiltInCategory.OST_PipeCurves },
                { "Pipe Fittings", BuiltInCategory.OST_PipeFitting },
                { "Pipe Accessories", BuiltInCategory.OST_PipeAccessory },
                { "Flex Pipes", BuiltInCategory.OST_FlexPipeCurves },
                { "Sprinklers", BuiltInCategory.OST_Sprinklers },

                // MEP - Electrical
                { "Electrical Equipment", BuiltInCategory.OST_ElectricalEquipment },
                { "Electrical Fixtures", BuiltInCategory.OST_ElectricalFixtures },
                { "Lighting Fixtures", BuiltInCategory.OST_LightingFixtures },
                { "Lighting Devices", BuiltInCategory.OST_LightingDevices },
                { "Cable Trays", BuiltInCategory.OST_CableTray },
                { "Cable Tray Fittings", BuiltInCategory.OST_CableTrayFitting },
                { "Conduits", BuiltInCategory.OST_Conduit },
                { "Conduit Fittings", BuiltInCategory.OST_ConduitFitting },
                { "Data Devices", BuiltInCategory.OST_DataDevices },
                { "Fire Alarm Devices", BuiltInCategory.OST_FireAlarmDevices },
                { "Communication Devices", BuiltInCategory.OST_CommunicationDevices },
                { "Security Devices", BuiltInCategory.OST_SecurityDevices },
                { "Nurse Call Devices", BuiltInCategory.OST_NurseCallDevices },
                { "Telephone Devices", BuiltInCategory.OST_TelephoneDevices }
            };
        }

#if REVIT2025
        private Dictionary<string, ForgeTypeId> InitializeParameterGroups()
        {
            return new Dictionary<string, ForgeTypeId>
            {
                { "Constraints", GroupTypeId.Constraints },
                { "Construction", GroupTypeId.Construction },
                { "Data", GroupTypeId.Data },
                { "Dimensions", GroupTypeId.Geometry },
                { "Electrical", GroupTypeId.Electrical },
                { "Electrical - Circuiting", GroupTypeId.ElectricalCircuiting },
                { "Electrical - Lighting", GroupTypeId.ElectricalLighting },
                { "Electrical - Loads", GroupTypeId.ElectricalLoads },
                { "Energy Analysis", GroupTypeId.EnergyAnalysis },
                { "Fire Protection", GroupTypeId.FireProtection },
                { "Forces", GroupTypeId.Forces },
                { "General", GroupTypeId.General },
                { "Graphics", GroupTypeId.Graphics },
                { "Green Building", GroupTypeId.GreenBuilding },
                { "Identity Data", GroupTypeId.IdentityData },
                { "IFC Parameters", GroupTypeId.Ifc },
                { "Materials and Finishes", GroupTypeId.Materials },
                { "Mechanical", GroupTypeId.Mechanical },
                { "Mechanical - Airflow", GroupTypeId.MechanicalAirflow },
                { "Mechanical - Loads", GroupTypeId.MechanicalLoads },
                { "Moments", GroupTypeId.Moments },
                { "Phasing", GroupTypeId.Phasing },
                { "Photometrics", GroupTypeId.LightPhotometrics },
                { "Plumbing", GroupTypeId.Plumbing },
                { "Primary End", GroupTypeId.PrimaryEnd },
                { "Rebar Set", GroupTypeId.RebarSystemLayers },
                { "Releases / Member Forces", GroupTypeId.ReleasesMemberForces },
                { "Secondary End", GroupTypeId.SecondaryEnd },
                { "Segments and Fittings", GroupTypeId.SegmentsFittings },
                { "Slab Shape Edit", GroupTypeId.SlabShapeEdit },
                { "Structural", GroupTypeId.Structural },
                { "Structural Analysis", GroupTypeId.StructuralAnalysis },
                { "Text", GroupTypeId.Text },
                { "Title Text", GroupTypeId.Title },
                { "Visibility", GroupTypeId.Visibility }
            };
        }
#else
        private Dictionary<string, BuiltInParameterGroup> InitializeParameterGroups()
        {
            return new Dictionary<string, BuiltInParameterGroup>
            {
                { "Constraints", BuiltInParameterGroup.PG_CONSTRAINTS },
                { "Construction", BuiltInParameterGroup.PG_CONSTRUCTION },
                { "Data", BuiltInParameterGroup.PG_DATA },
                { "Dimensions", BuiltInParameterGroup.PG_GEOMETRY },
                { "Electrical", BuiltInParameterGroup.PG_ELECTRICAL },
                { "Electrical - Circuiting", BuiltInParameterGroup.PG_ELECTRICAL_CIRCUITING },
                { "Electrical - Lighting", BuiltInParameterGroup.PG_ELECTRICAL_LIGHTING },
                { "Electrical - Loads", BuiltInParameterGroup.PG_ELECTRICAL_LOADS },
                { "Energy Analysis", BuiltInParameterGroup.PG_ENERGY_ANALYSIS },
                { "Fire Protection", BuiltInParameterGroup.PG_FIRE_PROTECTION },
                { "Forces", BuiltInParameterGroup.PG_FORCES },
                { "General", BuiltInParameterGroup.PG_GENERAL },
                { "Graphics", BuiltInParameterGroup.PG_GRAPHICS },
                { "Green Building", BuiltInParameterGroup.PG_GREEN_BUILDING },
                { "Identity Data", BuiltInParameterGroup.PG_IDENTITY_DATA },
                { "IFC Parameters", BuiltInParameterGroup.PG_IFC },
                { "Materials and Finishes", BuiltInParameterGroup.PG_MATERIALS },
                { "Mechanical", BuiltInParameterGroup.PG_MECHANICAL },
                { "Mechanical - Airflow", BuiltInParameterGroup.PG_MECHANICAL_AIRFLOW },
                { "Mechanical - Loads", BuiltInParameterGroup.PG_MECHANICAL_LOADS },
                { "Moments", BuiltInParameterGroup.PG_MOMENTS },
                { "Phasing", BuiltInParameterGroup.PG_PHASING },
                { "Photometrics", BuiltInParameterGroup.PG_LIGHT_PHOTOMETRICS },
                { "Plumbing", BuiltInParameterGroup.PG_PLUMBING },
                { "Primary End", BuiltInParameterGroup.PG_PRIMARY_END },
                { "Rebar Set", BuiltInParameterGroup.PG_REBAR_SYSTEM_LAYERS },
                { "Releases / Member Forces", BuiltInParameterGroup.PG_RELEASES_MEMBER_FORCES },
                { "Secondary End", BuiltInParameterGroup.PG_SECONDARY_END },
                { "Segments and Fittings", BuiltInParameterGroup.PG_SEGMENTS_FITTINGS },
                { "Slab Shape Edit", BuiltInParameterGroup.PG_SLAB_SHAPE_EDIT },
                { "Structural", BuiltInParameterGroup.PG_STRUCTURAL },
                { "Structural Analysis", BuiltInParameterGroup.PG_STRUCTURAL_ANALYSIS },
                { "Text", BuiltInParameterGroup.PG_TEXT },
                { "Title Text", BuiltInParameterGroup.PG_TITLE },
                { "Visibility", BuiltInParameterGroup.PG_VISIBILITY }
            };
        }
#endif

        #endregion

        #region Parameter Loading

        private void LoadSharedParameters()
        {
            try
            {
                DefinitionFile sharedParamFile = _doc.Application.OpenSharedParameterFile();
                if (sharedParamFile == null)
                {
                    MessageBox.Show("No shared parameter file is loaded.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _allParametersData.Clear();
                _checkboxLookup.Clear();

                foreach (DefinitionGroup group in sharedParamFile.Groups)
                {
                    var groupData = new GroupData
                    {
                        Name = group.Name,
                        Parameters = new List<ParameterData>()
                    };

                    foreach (ExternalDefinition paramDef in group.Definitions)
                    {
                        // Note: Revit 2023 ExternalDefinition doesn't expose ParameterType directly
                        string paramTypeName = "Text"; // Default
                        try
                        {
                            // Try to get parameter type if available through other means
#if REVIT2025
                            ForgeTypeId groupId = paramDef.GetGroupTypeId();
                            if (groupId != null && !groupId.Empty())
                            {
                                paramTypeName = groupId.TypeId;
                            }
#else
                            if (paramDef.ParameterGroup != BuiltInParameterGroup.INVALID)
                            {
                                paramTypeName = paramDef.ParameterGroup.ToString();
                            }
#endif
                        }
                        catch { }

                        var paramData = new ParameterData
                        {
                            Name = paramDef.Name,
                            Definition = paramDef,
                            GroupName = group.Name,
                            ParameterType = paramTypeName
                        };

                        groupData.Parameters.Add(paramData);
                    }

                    if (groupData.Parameters.Count > 0)
                    {
                        _allParametersData.Add(groupData);
                    }
                }

                // Sort groups and parameters alphabetically
                _allParametersData = _allParametersData.OrderBy(g => g.Name).ToList();
                foreach (var group in _allParametersData)
                {
                    group.Parameters = group.Parameters.OrderBy(p => p.Name).ToList();
                }

                PopulateParameterTree();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading shared parameters: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PopulateParameterTree()
        {
            _isUpdatingUI = true;

            ParametersTree.Items.Clear();

            foreach (var groupData in _allParametersData)
            {
                var groupItem = new TreeViewItem
                {
                    Header = $"{groupData.Name} ({groupData.Parameters.Count} parameters)",
                    IsExpanded = true
                };

                foreach (var paramData in groupData.Parameters)
                {
                    var checkbox = new CheckBox
                    {
                        Content = paramData.Name,
                        Tag = paramData
                    };

                    checkbox.Checked += ParameterCheckBox_Checked;
                    checkbox.Unchecked += ParameterCheckBox_Unchecked;
                    checkbox.PreviewMouseDown += ParameterCheckBox_MouseDown;

                    // Check if previously selected
                    checkbox.IsChecked = _selectedParams.ContainsKey(paramData.Name);

                    _checkboxLookup[paramData.Name] = checkbox;

                    var paramItem = new TreeViewItem
                    {
                        Header = checkbox
                    };

                    groupItem.Items.Add(paramItem);
                }

                ParametersTree.Items.Add(groupItem);
            }

            _isUpdatingUI = false;
        }

        #endregion

        #region Search Functionality

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdatingUI) return;

            ApplySearchFilter();
        }

        private void ApplySearchFilter()
        {
            _isUpdatingUI = true;

            string searchText = SearchTextBox.Text.ToLower().Trim();

            ParametersTree.Items.Clear();
            _checkboxLookup.Clear();

            var filteredData = _allParametersData;

            if (!string.IsNullOrEmpty(searchText))
            {
                filteredData = _allParametersData
                    .Select(g => new GroupData
                    {
                        Name = g.Name,
                        Parameters = g.Parameters.Where(p => p.Name.ToLower().Contains(searchText)).ToList()
                    })
                    .Where(g => g.Parameters.Count > 0)
                    .ToList();
            }

            foreach (var groupData in filteredData)
            {
                var groupItem = new TreeViewItem
                {
                    Header = $"{groupData.Name} ({groupData.Parameters.Count} parameters)",
                    IsExpanded = true
                };

                foreach (var paramData in groupData.Parameters)
                {
                    var checkbox = new CheckBox
                    {
                        Content = paramData.Name,
                        Tag = paramData
                    };

                    checkbox.Checked += ParameterCheckBox_Checked;
                    checkbox.Unchecked += ParameterCheckBox_Unchecked;
                    checkbox.PreviewMouseDown += ParameterCheckBox_MouseDown;

                    checkbox.IsChecked = _selectedParams.ContainsKey(paramData.Name);

                    _checkboxLookup[paramData.Name] = checkbox;

                    var paramItem = new TreeViewItem
                    {
                        Header = checkbox
                    };

                    groupItem.Items.Add(paramItem);
                }

                ParametersTree.Items.Add(groupItem);
            }

            _isUpdatingUI = false;
            UpdateSelectionSummary();
        }

        #endregion

        #region Parameter Selection

        private void ParameterCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (_isUpdatingUI) return;

            var checkbox = sender as CheckBox;
            var paramData = checkbox.Tag as ParameterData;

            if (!_selectedParams.ContainsKey(paramData.Name))
            {
                _selectedParams[paramData.Name] = new ParameterConfig
                {
                    Group = paramData.GroupName,
                    Categories = new List<string>(),
                    ParameterGroup = "Data",
                    IsInstance = true
                };

                // Add to selection order
                if (_selectionOrder.Contains(paramData.Name))
                    _selectionOrder.Remove(paramData.Name);

                _selectionOrder.Insert(0, paramData.Name);

                UpdateConfigPanel();
                UpdateSelectionSummary();
            }
        }

        private void ParameterCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_isUpdatingUI) return;

            var checkbox = sender as CheckBox;
            var paramData = checkbox.Tag as ParameterData;

            if (_selectedParams.ContainsKey(paramData.Name))
            {
                _selectedParams.Remove(paramData.Name);
                _selectionOrder.Remove(paramData.Name);

                UpdateConfigPanel();
                UpdateSelectionSummary();
            }
        }

        private void ParameterCheckBox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var checkbox = sender as CheckBox;
            var paramData = checkbox.Tag as ParameterData;

            // Get all parameters in display order (flattened list from tree)
            var allParams = new List<string>();
            foreach (var group in _allParametersData)
            {
                foreach (var param in group.Parameters)
                {
                    allParams.Add(param.Name);
                }
            }

            bool isShiftPressed = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
            bool isCtrlPressed = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);

            // SHIFT+CLICK: Range selection
            if (isShiftPressed && _lastClickedParameter != null && allParams.Contains(_lastClickedParameter))
            {
                e.Handled = true; // Prevent normal checkbox toggle

                int lastIndex = allParams.IndexOf(_lastClickedParameter);
                int currentIndex = allParams.IndexOf(paramData.Name);

                int startIndex = Math.Min(lastIndex, currentIndex);
                int endIndex = Math.Max(lastIndex, currentIndex);

                // Determine the target state based on the last clicked parameter
                bool targetState = _selectedParams.ContainsKey(_lastClickedParameter);

                _isUpdatingUI = true;

                // Apply the same state to all parameters in range
                for (int i = startIndex; i <= endIndex; i++)
                {
                    string paramName = allParams[i];
                    if (_checkboxLookup.TryGetValue(paramName, out CheckBox cb))
                    {
                        if (targetState)
                        {
                            // Select the parameter
                            if (!_selectedParams.ContainsKey(paramName))
                            {
                                var pd = cb.Tag as ParameterData;
                                _selectedParams[paramName] = new ParameterConfig
                                {
                                    Group = pd.GroupName,
                                    Categories = new List<string>(),
                                    ParameterGroup = "Data",
                                    IsInstance = true
                                };

                                if (_selectionOrder.Contains(paramName))
                                    _selectionOrder.Remove(paramName);

                                _selectionOrder.Insert(0, paramName);
                            }
                            cb.IsChecked = true;
                        }
                        else
                        {
                            // Deselect the parameter
                            if (_selectedParams.ContainsKey(paramName))
                            {
                                _selectedParams.Remove(paramName);
                                _selectionOrder.Remove(paramName);
                            }
                            cb.IsChecked = false;
                        }
                    }
                }

                _isUpdatingUI = false;
                UpdateConfigPanel();
                UpdateSelectionSummary();

                return; // Don't update _lastClickedParameter when shift-clicking
            }
            // NORMAL CLICK: Toggle selection (multi-select mode by default)
            else
            {
                // Let the normal checkbox behavior handle toggle
                // No need to clear other selections
                _lastClickedParameter = paramData.Name;
            }
        }

        private void UpdateSelectionSummary()
        {
            int count = _selectedParams.Count;
            SelectionSummary.Text = count == 0 ? "No parameters selected" :
                                    count == 1 ? "1 parameter selected" :
                                    $"{count} parameters selected";
        }

        #endregion

        #region Configuration Panel

        private void UpdateConfigPanel()
        {
            ConfigStackPanel.Children.Clear();

            if (_selectedParams.Count == 0)
            {
                var label = new TextBlock
                {
                    Text = "Select parameters from the tree to configure them",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontStyle = FontStyles.Italic,
                    Foreground = new SolidColorBrush(Colors.Gray),
                    Margin = new Thickness(0, 50, 0, 0)
                };
                ConfigStackPanel.Children.Add(label);
                return;
            }

            // Create single unified configuration UI for ALL selected parameters
            CreateUnifiedConfigUI();
        }

        private void CreateUnifiedConfigUI()
        {
            // Get the first parameter's config as the template (they should all be the same now)
            ParameterConfig firstConfig = _selectedParams.Values.FirstOrDefault();
            if (firstConfig == null)
            {
                firstConfig = new ParameterConfig
                {
                    IsInstance = true,
                    Categories = new List<string>(),
                    ParameterGroup = "Data"
                };
            }

            // Main container
            var border = new Border
            {
                BorderBrush = new SolidColorBrush(Color.FromRgb(70, 130, 180)), // Steel blue
                BorderThickness = new Thickness(2),
                Margin = new Thickness(5),
                Padding = new Thickness(15)
            };

            var stack = new StackPanel();

            // Header showing count
            var header = new TextBlock
            {
                Text = $"Configure {_selectedParams.Count} selected parameter(s):",
                FontWeight = FontWeights.Bold,
                FontSize = 16,
                Margin = new Thickness(0, 0, 0, 15)
            };
            stack.Children.Add(header);

            // Info text
            var infoText = new TextBlock
            {
                Text = "The configuration below will be applied to ALL selected parameters.",
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(100, 100, 100)),
                Margin = new Thickness(0, 0, 0, 20),
                TextWrapping = TextWrapping.Wrap
            };
            stack.Children.Add(infoText);

            // Binding type
            var bindingLabel = new TextBlock
            {
                Text = "Parameter Binding:",
                FontWeight = FontWeights.SemiBold,
                FontSize = 13,
                Margin = new Thickness(0, 0, 0, 8)
            };
            stack.Children.Add(bindingLabel);

            var bindingStack = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 5) };

            var instanceRadio = new RadioButton
            {
                Content = "Instance",
                GroupName = "unified_binding",
                IsChecked = firstConfig.IsInstance, // Use value from loaded preset
                Margin = new Thickness(0, 0, 20, 0),
                FontSize = 12
            };
            instanceRadio.Checked += UnifiedBindingRadio_Checked;
            bindingStack.Children.Add(instanceRadio);

            var typeRadio = new RadioButton
            {
                Content = "Type",
                GroupName = "unified_binding",
                IsChecked = !firstConfig.IsInstance, // Use value from loaded preset
                FontSize = 12
            };
            typeRadio.Checked += UnifiedBindingRadio_Checked;
            bindingStack.Children.Add(typeRadio);

            stack.Children.Add(bindingStack);

            // Explanation
            var explanation = new TextBlock
            {
                Text = "Instance: Applies to individual elements | Type: Applies to all elements of the same type",
                FontSize = 10,
                Foreground = new SolidColorBrush(Color.FromRgb(100, 100, 100)),
                Margin = new Thickness(0, 0, 0, 20),
                TextWrapping = TextWrapping.Wrap
            };
            stack.Children.Add(explanation);

            // Categories
            var catLabel = new TextBlock
            {
                Text = "Apply to Categories:",
                FontWeight = FontWeights.SemiBold,
                FontSize = 13,
                Margin = new Thickness(0, 0, 0, 8)
            };
            stack.Children.Add(catLabel);

            var catWrap = new WrapPanel();

            foreach (var catName in _availableCategories.Keys.OrderBy(k => k))
            {
                var catCheckbox = new CheckBox
                {
                    Content = catName,
                    Margin = new Thickness(0, 0, 15, 8),
                    FontSize = 11,
                    Tag = catName,
                    IsChecked = firstConfig.Categories.Contains(catName) // Use value from loaded preset
                };
                catCheckbox.Checked += UnifiedCategoryCheckbox_Changed;
                catCheckbox.Unchecked += UnifiedCategoryCheckbox_Changed;
                catWrap.Children.Add(catCheckbox);
            }

            stack.Children.Add(catWrap);

            // Parameter group
            var groupLabel = new TextBlock
            {
                Text = "Parameter Group:",
                FontWeight = FontWeights.SemiBold,
                FontSize = 13,
                Margin = new Thickness(0, 15, 0, 8)
            };
            stack.Children.Add(groupLabel);

            var combo = new ComboBox
            {
                FontSize = 12,
                Padding = new Thickness(5)
            };

            foreach (var groupName in _parameterGroups.Keys.OrderBy(k => k))
            {
                combo.Items.Add(groupName);
            }

            // Use value from loaded preset, or default to "Data"
            combo.SelectedItem = firstConfig.ParameterGroup ?? "Data";
            combo.SelectionChanged += UnifiedParameterGroup_Changed;

            stack.Children.Add(combo);

            border.Child = stack;
            ConfigStackPanel.Children.Add(border);
        }

        private void UnifiedBindingRadio_Checked(object sender, RoutedEventArgs e)
        {
            var radio = sender as RadioButton;
            bool isInstance = radio.Content.ToString() == "Instance";

            // Apply to all selected parameters
            foreach (var param in _selectedParams.Values)
            {
                param.IsInstance = isInstance;
            }
        }

        private void UnifiedCategoryCheckbox_Changed(object sender, RoutedEventArgs e)
        {
            var checkbox = sender as CheckBox;
            string categoryName = checkbox.Tag.ToString();

            // Apply to all selected parameters
            foreach (var param in _selectedParams.Values)
            {
                if (checkbox.IsChecked == true)
                {
                    if (!param.Categories.Contains(categoryName))
                    {
                        param.Categories.Add(categoryName);
                    }
                }
                else
                {
                    param.Categories.Remove(categoryName);
                }
            }
        }

        private void UnifiedParameterGroup_Changed(object sender, SelectionChangedEventArgs e)
        {
            var combo = sender as ComboBox;
            if (combo.SelectedItem != null)
            {
                string groupName = combo.SelectedItem.ToString();

                // Apply to all selected parameters
                foreach (var param in _selectedParams.Values)
                {
                    param.ParameterGroup = groupName;
                }
            }
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
                MessageBox.Show($"Error loading presets: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            PresetsComboBox.SelectedIndex = 0;
        }

        private void LoadPresetButton_Click(object sender, RoutedEventArgs e)
        {
            if (PresetsComboBox.SelectedIndex <= 0)
            {
                MessageBox.Show("Please select a preset to load.", "No Preset Selected", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string presetName = PresetsComboBox.SelectedItem.ToString();
            string presetFile = Path.Combine(_presetsDirectory, $"{presetName}.json");

            try
            {
                string json = File.ReadAllText(presetFile);
                var presetData = JsonConvert.DeserializeObject<PresetData>(json);

                _isUpdatingUI = true;

                // Clear current selection
                _selectedParams.Clear();
                _selectionOrder.Clear();

                // Get shared parameter file and build lookup
                DefinitionFile sharedParamFile = _doc.Application.OpenSharedParameterFile();
                if (sharedParamFile == null)
                {
                    MessageBox.Show("No shared parameter file loaded.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    _isUpdatingUI = false;
                    return;
                }

                var paramLookup = new Dictionary<string, ParameterData>();
                foreach (DefinitionGroup group in sharedParamFile.Groups)
                {
                    foreach (ExternalDefinition paramDef in group.Definitions)
                    {
                        // Note: Revit 2023 ExternalDefinition doesn't expose ParameterType directly
                        string paramTypeName = "Text"; // Default
                        try
                        {
#if REVIT2025
                            ForgeTypeId groupId = paramDef.GetGroupTypeId();
                            if (groupId != null && !groupId.Empty())
                            {
                                paramTypeName = groupId.TypeId;
                            }
#else
                            if (paramDef.ParameterGroup != BuiltInParameterGroup.INVALID)
                            {
                                paramTypeName = paramDef.ParameterGroup.ToString();
                            }
#endif
                        }
                        catch { }

                        paramLookup[paramDef.Name] = new ParameterData
                        {
                            Name = paramDef.Name,
                            Definition = paramDef,
                            GroupName = group.Name,
                            ParameterType = paramTypeName
                        };
                    }
                }

                // Load parameters from preset
                foreach (var kvp in presetData)
                {
                    if (paramLookup.ContainsKey(kvp.Key))
                    {
                        _selectedParams[kvp.Key] = kvp.Value;
                        _selectionOrder.Add(kvp.Key);
                    }
                }

                // Update UI
                BulkUpdateCheckboxes();
                UpdateConfigPanel();
                UpdateSelectionSummary();

                _isUpdatingUI = false;

                MessageBox.Show($"Preset '{presetName}' loaded successfully!\n\nLoaded {_selectedParams.Count} parameter(s).",
                    "Preset Loaded", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                _isUpdatingUI = false;
                MessageBox.Show($"Error loading preset: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SavePresetButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedParams.Count == 0)
            {
                MessageBox.Show("No parameters selected to save as preset.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Create a simple input dialog
            var inputDialog = new Window
            {
                Title = "Save Preset",
                Width = 400,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                ResizeMode = ResizeMode.NoResize
            };

            var stackPanel = new StackPanel { Margin = new Thickness(15) };

            var label = new TextBlock
            {
                Text = "Enter preset name:",
                Margin = new Thickness(0, 0, 0, 10)
            };
            stackPanel.Children.Add(label);

            var textBox = new TextBox
            {
                Margin = new Thickness(0, 0, 0, 15)
            };
            stackPanel.Children.Add(textBox);

            var buttonStack = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            var okButton = new Button
            {
                Content = "OK",
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

            if (inputDialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(textBox.Text))
            {
                string presetName = textBox.Text.Trim();

                try
                {
                    var presetData = new PresetData();
                    foreach (var kvp in _selectedParams)
                    {
                        presetData[kvp.Key] = kvp.Value;
                    }

                    string json = JsonConvert.SerializeObject(presetData, Formatting.Indented);
                    string presetFile = Path.Combine(_presetsDirectory, $"{presetName}.json");

                    File.WriteAllText(presetFile, json);

                    MessageBox.Show($"Preset '{presetName}' saved successfully!\n\nSaved {_selectedParams.Count} parameter(s).",
                        "Preset Saved", MessageBoxButton.OK, MessageBoxImage.Information);

                    LoadPresets();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving preset: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DeletePresetButton_Click(object sender, RoutedEventArgs e)
        {
            if (PresetsComboBox.SelectedIndex <= 0)
            {
                MessageBox.Show("Please select a preset to delete.", "No Preset Selected", MessageBoxButton.OK, MessageBoxImage.Information);
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
                    MessageBox.Show($"Error deleting preset: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BulkUpdateCheckboxes()
        {
            foreach (var kvp in _checkboxLookup)
            {
                kvp.Value.Checked -= ParameterCheckBox_Checked;
                kvp.Value.Unchecked -= ParameterCheckBox_Unchecked;

                kvp.Value.IsChecked = _selectedParams.ContainsKey(kvp.Key);

                kvp.Value.Checked += ParameterCheckBox_Checked;
                kvp.Value.Unchecked += ParameterCheckBox_Unchecked;
            }
        }

        #endregion

        #region Add Parameters

        private void AddParametersButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedParams.Count == 0)
            {
                MessageBox.Show("No parameters selected.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var addedParams = new List<string>();
                var failedParams = new List<string>();
                var skippedParams = new List<string>();
                bool updateExisting = UpdateExistingCheckBox.IsChecked == true;
                bool mergeCategories = MergeCategoriesCheckBox.IsChecked == true;

                using (Transaction trans = new Transaction(_doc, "Add BIM Kraft Project Parameters"))
                {
                    trans.Start();

                    foreach (var kvp in _selectedParams)
                    {
                        string paramName = kvp.Key;
                        ParameterConfig config = kvp.Value;

                        try
                        {
                            // Find the parameter definition
                            ExternalDefinition paramDef = FindParameterDefinition(paramName);
                            if (paramDef == null)
                            {
                                failedParams.Add($"{paramName} (not found in shared parameters)");
                                continue;
                            }

                            // Create category set
                            CategorySet categorySet = _doc.Application.Create.NewCategorySet();

                            foreach (string catName in config.Categories)
                            {
                                if (_availableCategories.ContainsKey(catName))
                                {
                                    try
                                    {
                                        Category category = _doc.Settings.Categories.get_Item(_availableCategories[catName]);
                                        if (category != null)
                                        {
                                            categorySet.Insert(category);
                                        }
                                    }
                                    catch { }
                                }
                            }

                            if (categorySet.Size == 0)
                            {
                                failedParams.Add($"{paramName} (no valid categories)");
                                continue;
                            }

                            // Create binding
                            Binding binding;
                            string bindingType;

                            if (config.IsInstance)
                            {
                                binding = _doc.Application.Create.NewInstanceBinding(categorySet);
                                bindingType = "Instance";
                            }
                            else
                            {
                                binding = _doc.Application.Create.NewTypeBinding(categorySet);
                                bindingType = "Type";
                            }

                            // Get parameter group
#if REVIT2025
                            ForgeTypeId paramGroup = _parameterGroups.ContainsKey(config.ParameterGroup)
                                ? _parameterGroups[config.ParameterGroup]
                                : GroupTypeId.Data;

                            // Add parameter
                            bool success = _doc.ParameterBindings.Insert(paramDef, binding, paramGroup);
#else
                            BuiltInParameterGroup paramGroup = _parameterGroups.ContainsKey(config.ParameterGroup)
                                ? _parameterGroups[config.ParameterGroup]
                                : BuiltInParameterGroup.PG_DATA;

                            // Add parameter
                            bool success = _doc.ParameterBindings.Insert(paramDef, binding, paramGroup);
#endif

                            if (success)
                            {
                                addedParams.Add($"{paramName} ({bindingType})");
                            }
                            else
                            {
                                // Parameter already exists
                                if (updateExisting)
                                {
                                    // Check if we should merge categories
                                    if (mergeCategories)
                                    {
                                        try
                                        {
                                            // Get existing binding
                                            Binding existingBinding = _doc.ParameterBindings.get_Item(paramDef);

                                            if (existingBinding != null)
                                            {
                                                // Get existing categories
                                                CategorySet existingCategories = null;
                                                if (existingBinding is InstanceBinding)
                                                {
                                                    existingCategories = ((InstanceBinding)existingBinding).Categories;
                                                }
                                                else if (existingBinding is TypeBinding)
                                                {
                                                    existingCategories = ((TypeBinding)existingBinding).Categories;
                                                }

                                                if (existingCategories != null)
                                                {
                                                    // Merge: Add existing categories to our new category set
                                                    foreach (Category existingCat in existingCategories)
                                                    {
                                                        if (!categorySet.Contains(existingCat))
                                                        {
                                                            categorySet.Insert(existingCat);
                                                        }
                                                    }

                                                    // Create new binding with merged categories
                                                    Binding mergedBinding;
                                                    if (config.IsInstance)
                                                    {
                                                        mergedBinding = _doc.Application.Create.NewInstanceBinding(categorySet);
                                                    }
                                                    else
                                                    {
                                                        mergedBinding = _doc.Application.Create.NewTypeBinding(categorySet);
                                                    }

                                                    // Update with merged categories
                                                    bool updateSuccess = _doc.ParameterBindings.ReInsert(paramDef, mergedBinding, paramGroup);
                                                    if (updateSuccess)
                                                    {
                                                        addedParams.Add($"{paramName} (categories merged, {bindingType})");
                                                    }
                                                    else
                                                    {
                                                        failedParams.Add($"{paramName} (failed to merge categories)");
                                                    }
                                                }
                                                else
                                                {
                                                    failedParams.Add($"{paramName} (could not get existing categories)");
                                                }
                                            }
                                            else
                                            {
                                                failedParams.Add($"{paramName} (could not get existing binding)");
                                            }
                                        }
                                        catch (Exception mergeEx)
                                        {
                                            failedParams.Add($"{paramName} (merge error: {mergeEx.Message})");
                                        }
                                    }
                                    else
                                    {
                                        // Replace categories (original behavior)
                                        bool updateSuccess = _doc.ParameterBindings.ReInsert(paramDef, binding, paramGroup);
                                        if (updateSuccess)
                                        {
                                            addedParams.Add($"{paramName} (updated, {bindingType})");
                                        }
                                        else
                                        {
                                            failedParams.Add($"{paramName} (failed to update)");
                                        }
                                    }
                                }
                                else
                                {
                                    // Skip existing parameter
                                    skippedParams.Add($"{paramName} (already exists)");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            failedParams.Add($"{paramName} (error: {ex.Message})");
                        }
                    }

                    trans.Commit();
                }

                // Show results
                string resultMsg = "";

                if (addedParams.Count > 0)
                {
                    resultMsg += "Successfully added/updated parameters:\n";
                    foreach (string param in addedParams)
                    {
                        resultMsg += $"• {param}\n";
                    }
                }

                if (skippedParams.Count > 0)
                {
                    resultMsg += "\nSkipped existing parameters:\n";
                    foreach (string param in skippedParams)
                    {
                        resultMsg += $"• {param}\n";
                    }
                }

                if (failedParams.Count > 0)
                {
                    resultMsg += "\nFailed parameters:\n";
                    foreach (string param in failedParams)
                    {
                        resultMsg += $"• {param}\n";
                    }
                }

                if (addedParams.Count > 0 || skippedParams.Count > 0)
                {
                    MessageBox.Show(resultMsg, "Parameters Processed", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show(resultMsg, "No Parameters Added", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding parameters: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private ExternalDefinition FindParameterDefinition(string paramName)
        {
            DefinitionFile sharedParamFile = _doc.Application.OpenSharedParameterFile();
            if (sharedParamFile == null) return null;

            foreach (DefinitionGroup group in sharedParamFile.Groups)
            {
                foreach (ExternalDefinition def in group.Definitions)
                {
                    if (def.Name == paramName)
                    {
                        return def;
                    }
                }
            }

            return null;
        }

        #endregion

        #region Cancel

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        #endregion

        #region Helper Classes

        private class GroupData
        {
            public string Name { get; set; }
            public List<ParameterData> Parameters { get; set; }
        }

        private class ParameterData
        {
            public string Name { get; set; }
            public ExternalDefinition Definition { get; set; }
            public string GroupName { get; set; }
            public string ParameterType { get; set; }
        }

        #endregion
    }
}
