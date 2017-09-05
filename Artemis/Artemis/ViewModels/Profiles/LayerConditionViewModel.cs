using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Artemis.Profiles.Layers.Models;
using Artemis.Utilities;
using Caliburn.Micro;
using MahApps.Metro.Controls;

namespace Artemis.ViewModels.Profiles
{
    public sealed class LayerConditionViewModel : Screen
    {
        private readonly NamedOperator[] _boolOperators =
        {
            new NamedOperator("Equal to", "==")
        };

        private readonly LayerEditorViewModel _editorViewModel;

        private readonly NamedOperator[] _int32Operators =
        {
            new NamedOperator("Lower than", "<"),
            new NamedOperator("Lower or equal to", "<="),
            new NamedOperator("Higher than", ">"),
            new NamedOperator("Higher or equal to", ">="),
            new NamedOperator("Equal to", "=="),
            new NamedOperator("Not equal to", "!=")
        };

        private readonly NamedOperator[] _operators =
        {
            new NamedOperator("Equal to", "=="),
            new NamedOperator("Not equal to", "!=")
        };

        private readonly NamedOperator[] _stringOperators =
        {
            new NamedOperator("Equal to", "=="),
            new NamedOperator("Not equal to", "!="),
            new NamedOperator("Contains", ".Contains"),
            new NamedOperator("Starts with", ".StartsWith"),
            new NamedOperator("Ends with", ".EndsWith")
        };

        private readonly NamedOperator[] _listOperatorsPrefixes =
        {
            new NamedOperator("Any", "any"),
            new NamedOperator("All", "all"),
            new NamedOperator("None", "none")
        };

        private HotKey _hotKey;
        private bool _keybindIsVisible;
        private GeneralHelpers.PropertyCollection _selectedDataModelProp;
        private string _selectedDropdownValue;
        private NamedOperator _selectedOperator;
        private bool _userDropdownValueIsVisible;
        private string _userValue;
        private bool _userValueIsVisible;

        public LayerConditionViewModel(LayerEditorViewModel editorViewModel, LayerConditionModel conditionModel)
        {
            _editorViewModel = editorViewModel;

            ConditionModel = conditionModel;
            Operators = new BindableCollection<NamedOperator>();
            DropdownValues = new BindableCollection<string>();
            DataModelProps = new BindableCollection<GeneralHelpers.PropertyCollection>();
            DataModelProps.AddRange(editorViewModel.DataModelProps);

            PropertyChanged += MapViewToModel;
            MapModelToView();
        }

        public LayerConditionModel ConditionModel { get; set; }
        public BindableCollection<GeneralHelpers.PropertyCollection> DataModelProps { get; set; }
        public BindableCollection<NamedOperator> Operators { get; set; }
        public BindableCollection<string> DropdownValues { get; set; }

        public string UserValue
        {
            get { return _userValue; }
            set
            {
                if (value == _userValue)
                    return;
                _userValue = value;
                NotifyOfPropertyChange(() => UserValue);
            }
        }

        public HotKey HotKey
        {
            get { return _hotKey; }
            set
            {
                if (Equals(value, _hotKey))
                    return;
                _hotKey = value;
                NotifyOfPropertyChange(() => HotKey);
            }
        }

        public GeneralHelpers.PropertyCollection SelectedDataModelProp
        {
            get { return _selectedDataModelProp; }
            set
            {
                if (value.Equals(_selectedDataModelProp))
                    return;
                _selectedDataModelProp = value;
                SetupPropertyInput();
                NotifyOfPropertyChange(() => SelectedDataModelProp);
            }
        }

        public bool UserValueIsVisible
        {
            get { return _userValueIsVisible; }
            set
            {
                if (value == _userValueIsVisible)
                    return;
                _userValueIsVisible = value;
                NotifyOfPropertyChange(() => UserValueIsVisible);
            }
        }

        public bool UserDropdownValueIsVisible
        {
            get { return _userDropdownValueIsVisible; }
            set
            {
                if (value == _userDropdownValueIsVisible)
                    return;
                _userDropdownValueIsVisible = value;
                NotifyOfPropertyChange(() => UserDropdownValueIsVisible);
            }
        }

        public bool KeybindIsVisible
        {
            get { return _keybindIsVisible; }
            set
            {
                if (value == _keybindIsVisible)
                    return;
                _keybindIsVisible = value;
                NotifyOfPropertyChange();
            }
        }

        public NamedOperator SelectedOperator
        {
            get { return _selectedOperator; }
            set
            {
                _selectedOperator = value;
                SetupUserValueInput();
                NotifyOfPropertyChange(() => SelectedOperator);
            }
        }

        public string SelectedDropdownValue
        {
            get { return _selectedDropdownValue; }
            set
            {
                if (value == _selectedDropdownValue)
                    return;
                _selectedDropdownValue = value;
                NotifyOfPropertyChange(() => SelectedDropdownValue);
            }
        }

        public void MapModelToView()
        {
            PropertyChanged -= MapViewToModel;

            // Select the right property
            SelectedDataModelProp = DataModelProps.FirstOrDefault(m => m.Path == ConditionModel.Field);
            // Select the operator
            SelectedOperator = Operators.FirstOrDefault(o => o.Value == ConditionModel.Operator);
            HotKey = ConditionModel.HotKey;

            if (ConditionModel.Type == "Enum" || ConditionModel.Type == "Boolean")
                SelectedDropdownValue = ConditionModel.Value;
            else
                UserValue = ConditionModel.Value;

            PropertyChanged += MapViewToModel;
        }

        public void MapViewToModel()
        {
            ConditionModel.Field = SelectedDataModelProp.Path;
            ConditionModel.Operator = SelectedOperator.Value;
            ConditionModel.Type = SelectedDataModelProp.Type;
            ConditionModel.HotKey = HotKey;

            if (ConditionModel.Type == "Enum" || ConditionModel.Type == "Boolean")
                ConditionModel.Value = SelectedDropdownValue;
            else
                ConditionModel.Value = UserValue;
        }

        private void MapViewToModel(object sender, PropertyChangedEventArgs e)
        {
            MapViewToModel();
        }

        public void SetupPropertyInput()
        {
            Operators.Clear();
            DropdownValues.Clear();
            switch (SelectedDataModelProp.Type)
            {
                case "Int32":
                case "Single":
                    Operators.AddRange(_int32Operators);
                    UserValueIsVisible = true;
                    break;
                case "Boolean":
                    Operators.AddRange(_boolOperators);
                    DropdownValues.Add("True");
                    DropdownValues.Add("False");
                    UserDropdownValueIsVisible = true;
                    break;
                case "String":
                    Operators.AddRange(_stringOperators);
                    UserValueIsVisible = true;
                    break;
                default:
                    Operators.AddRange(_operators);
                    UserValueIsVisible = true;
                    break;
            }
            // If the selected value is in a list, prefix all choices with list choices
            if (SelectedDataModelProp.Path != null && SelectedDataModelProp.Path.Contains("("))
            {
                var listOperators = new List<NamedOperator>();
                foreach (var o in Operators)
                    listOperators.AddRange(_listOperatorsPrefixes.Select(p => new NamedOperator(p.Display + " " + o.Display.ToLower(), p.Value + "|" + o.Value)));
                
                Operators.Clear();
                Operators.AddRange(listOperators);
            }
            // Add Changed operator is the type is event
            if (_editorViewModel.ProposedLayer.IsEvent)
            {
                Operators.Add(new NamedOperator("Changed", "changed"));
                // Also add decreased and increased operator on numbers
                if (SelectedDataModelProp.Type == "Int32" || SelectedDataModelProp.Type == "Single")
                {
                    Operators.Add(new NamedOperator("Decreased", "decreased"));
                    Operators.Add(new NamedOperator("Increased", "increased"));
                }
            }

            SetupUserValueInput();
        }

        private void SetupUserValueInput()
        {
            UserValueIsVisible = false;
            UserDropdownValueIsVisible = false;
            KeybindIsVisible = false;

            // Event operators don't have any form of input
            if (SelectedOperator.Value == "changed" || SelectedOperator.Value == "decreased" || SelectedOperator.Value == "increased")
                return;

            if (SelectedDataModelProp.Type != null && SelectedDataModelProp.Type.Contains("hotkey"))
            {
                KeybindIsVisible = true;
            }
            else if (SelectedDataModelProp.Type == "Boolean")
            {
                UserDropdownValueIsVisible = true;
            }
            else if (SelectedDataModelProp.EnumValues != null)
            {
                DropdownValues.Clear();
                DropdownValues.AddRange(SelectedDataModelProp.EnumValues);
                UserDropdownValueIsVisible = true;
            }
            else
            {
                UserValueIsVisible = true;
            }
        }

        /// <summary>
        ///     Delete the current model from the parent
        /// </summary>
        public void Delete()
        {
            _editorViewModel.LayerConditionVms.Remove(this);
        }

        public struct NamedOperator
        {
            public string Display { get; set; }
            public string Value { get; set; }

            public NamedOperator(string display, string value)
            {
                Display = display;
                Value = value;
            }
        }
    }
}
