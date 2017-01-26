using System.ComponentModel;
using System.Linq;
using Artemis.Profiles.Layers.Models;
using Artemis.Utilities;
using Caliburn.Micro;

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

        private bool _enumValueIsVisible;
        private GeneralHelpers.PropertyCollection _selectedDataModelProp;
        private string _selectedEnum;
        private NamedOperator _selectedOperator;
        private string _userValue;
        private bool _userValueIsVisible;

        public LayerConditionViewModel(LayerEditorViewModel editorViewModel, LayerConditionModel conditionModel)
        {
            _editorViewModel = editorViewModel;

            ConditionModel = conditionModel;
            DataModelProps = editorViewModel.DataModelProps;
            Operators = new BindableCollection<NamedOperator>();
            Enums = new BindableCollection<string>();

            PropertyChanged += MapViewToModel;
            MapModelToView();
        }

        public LayerConditionModel ConditionModel { get; set; }

        public BindableCollection<GeneralHelpers.PropertyCollection> DataModelProps { get; set; }

        public BindableCollection<NamedOperator> Operators { get; set; }
        public BindableCollection<string> Enums { get; set; }

        public string UserValue
        {
            get { return _userValue; }
            set
            {
                if (value == _userValue) return;
                _userValue = value;
                NotifyOfPropertyChange(() => UserValue);
            }
        }

        public GeneralHelpers.PropertyCollection SelectedDataModelProp
        {
            get { return _selectedDataModelProp; }
            set
            {
                if (value.Equals(_selectedDataModelProp)) return;
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
                if (value == _userValueIsVisible) return;
                _userValueIsVisible = value;
                NotifyOfPropertyChange(() => UserValueIsVisible);
            }
        }

        public bool EnumValueIsVisible
        {
            get { return _enumValueIsVisible; }
            set
            {
                if (value == _enumValueIsVisible) return;
                _enumValueIsVisible = value;
                NotifyOfPropertyChange(() => EnumValueIsVisible);
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

        public string SelectedEnum
        {
            get { return _selectedEnum; }
            set
            {
                if (value == _selectedEnum) return;
                _selectedEnum = value;
                NotifyOfPropertyChange(() => SelectedEnum);
            }
        }

        public void MapModelToView()
        {
            PropertyChanged -= MapViewToModel;

            // Select the right property
            SelectedDataModelProp = DataModelProps.FirstOrDefault(m => m.Path == ConditionModel.Field);
            // Select the operator
            SelectedOperator = Operators.FirstOrDefault(o => o.Value == ConditionModel.Operator);

            if (ConditionModel.Type == "Enum" || ConditionModel.Type == "Boolean")
                SelectedEnum = ConditionModel.Value;
            else
                UserValue = ConditionModel.Value;

            PropertyChanged += MapViewToModel;
        }

        public void MapViewToModel()
        {
            ConditionModel.Field = SelectedDataModelProp.Path;
            ConditionModel.Operator = SelectedOperator.Value;
            ConditionModel.Type = SelectedDataModelProp.Type;

            if (ConditionModel.Type == "Enum" || ConditionModel.Type == "Boolean")
                ConditionModel.Value = SelectedEnum;
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
            Enums.Clear();

            switch (SelectedDataModelProp.Type)
            {
                case "Int32":
                case "Single":
                    Operators.AddRange(_int32Operators);
                    UserValueIsVisible = true;
                    break;
                case "Boolean":
                    Operators.AddRange(_boolOperators);
                    Enums.Add("True");
                    Enums.Add("False");
                    EnumValueIsVisible = true;
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
            EnumValueIsVisible = false;

            if (SelectedOperator.Value == "changed" || SelectedOperator.Value == "decreased" ||
                SelectedOperator.Value == "increased")
                return;

            if (SelectedDataModelProp.Type == "Boolean")
                EnumValueIsVisible = true;
            else if (SelectedDataModelProp.EnumValues != null)
            {
                Enums.Clear();
                Enums.AddRange(SelectedDataModelProp.EnumValues);
                EnumValueIsVisible = true;
            }
            else
                UserValueIsVisible = true;
        }

        /// <summary>
        ///     Delete the current model from the parent
        /// </summary>
        public void Delete()
        {
            _editorViewModel.DeleteCondition(this, ConditionModel);
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