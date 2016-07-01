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

        private readonly LayerEditorViewModel _conditionModel;

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

        private bool _enumValueIsVisible;
        private bool _preselecting;
        private GeneralHelpers.PropertyCollection _selectedDataModelProp;
        private string _selectedEnum;
        private NamedOperator _selectedOperator;
        private string _userValue;
        private bool _userValueIsVisible;

        public LayerConditionViewModel(LayerEditorViewModel conditionModel, LayerConditionModel layerConditionModel,
            BindableCollection<GeneralHelpers.PropertyCollection> dataModelProps)
        {
            _conditionModel = conditionModel;
            _preselecting = false;

            LayerConditionModel = layerConditionModel;
            DataModelProps = dataModelProps;
            Operators = new BindableCollection<NamedOperator>();
            Enums = new BindableCollection<string>();

            PropertyChanged += UpdateModel;
            PropertyChanged += UpdateForm;

            PreSelect();
        }

        public LayerConditionModel LayerConditionModel { get; set; }

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

        /// <summary>
        ///     Handles updating the form to match the selected data model property
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateForm(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "SelectedDataModelProp")
                return;

            Operators.Clear();
            Enums.Clear();
            UserValueIsVisible = false;
            EnumValueIsVisible = false;

            switch (SelectedDataModelProp.Type)
            {
                case "Int32":
                    Operators.AddRange(_int32Operators);
                    UserValueIsVisible = true;
                    break;
                case "Boolean":
                    Operators.AddRange(_boolOperators);
                    Enums.Add("True");
                    Enums.Add("False");
                    EnumValueIsVisible = true;
                    break;
                default:
                    Operators.AddRange(_operators);
                    UserValueIsVisible = true;
                    break;
            }

            // Setup Enum selection if needed
            if (SelectedDataModelProp.EnumValues != null)
            {
                Enums.AddRange(SelectedDataModelProp.EnumValues);
                EnumValueIsVisible = true;
            }

            SelectedOperator = Operators.First();
        }


        /// <summary>
        ///     Handles saving user input to the model
        ///     TODO: Data validation?
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateModel(object sender, PropertyChangedEventArgs e)
        {
            // Don't mess with model during preselect
            if (_preselecting)
                return;

            // Only care about these fields
            if (e.PropertyName != "UserValue" &&
                e.PropertyName != "SelectedOperator" &&
                e.PropertyName != "SelectedDataModelProp" &&
                e.PropertyName != "SelectedEnum")
                return;

            LayerConditionModel.Field = SelectedDataModelProp.Path;
            LayerConditionModel.Operator = SelectedOperator.Value;
            LayerConditionModel.Type = SelectedDataModelProp.Type;

            if (SelectedDataModelProp.Type == "Enum" || SelectedDataModelProp.Type == "Boolean")
                LayerConditionModel.Value = SelectedEnum;
            else
                LayerConditionModel.Value = UserValue;

            UpdateForm(sender, e);
        }

        /// <summary>
        ///     Setup the current UI elements to show the backing model
        /// </summary>
        private void PreSelect()
        {
            _preselecting = true;
            SelectedDataModelProp = DataModelProps.FirstOrDefault(m => m.Path == LayerConditionModel.Field);
            SelectedOperator = Operators.FirstOrDefault(o => o.Value == LayerConditionModel.Operator);
            LayerConditionModel.Type = SelectedDataModelProp.Type;
            if (LayerConditionModel.Type == "Enum")
                SelectedEnum = LayerConditionModel.Value;
            else
                UserValue = LayerConditionModel.Value;

            _preselecting = false;
        }

        /// <summary>
        ///     Delete the current model from the parent
        /// </summary>
        public void Delete()
        {
            _conditionModel.DeleteCondition(this, LayerConditionModel);
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