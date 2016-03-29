using System.ComponentModel;
using System.Linq;
using Artemis.Models.Profiles;
using Artemis.Utilities;
using Caliburn.Micro;

namespace Artemis.ViewModels.LayerEditor
{
    public class LayerConditionViewModel<T> : Screen
    {
        private readonly NamedOperator[] _boolOperators =
        {
            new NamedOperator("True", "== True"),
            new NamedOperator("False", "== False")
        };

        private readonly LayerEditorViewModel<T> _conditionModel;

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

        private bool _preselecting;

        private GeneralHelpers.PropertyCollection _selectedDataModelProp;
        private NamedOperator _selectedOperator;
        private string _userValue;
        private bool _userValueIsVisible;

        public LayerConditionViewModel(LayerEditorViewModel<T> conditionModel, LayerConditionModel layerConditionModel,
            BindableCollection<GeneralHelpers.PropertyCollection> dataModelProps)
        {
            _conditionModel = conditionModel;
            _preselecting = false;

            LayerConditionModel = layerConditionModel;
            DataModelProps = dataModelProps;
            Operators = new BindableCollection<NamedOperator>();

            PropertyChanged += UpdateModel;
            PropertyChanged += UpdateForm;

            PreSelect();
        }

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

        public LayerConditionModel LayerConditionModel { get; set; }
        public BindableCollection<GeneralHelpers.PropertyCollection> DataModelProps { get; set; }
        public BindableCollection<NamedOperator> Operators { get; set; }

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

        public NamedOperator SelectedOperator
        {
            get { return _selectedOperator; }
            set
            {
                if (value.Equals(_selectedOperator)) return;
                _selectedOperator = value;
                NotifyOfPropertyChange(() => SelectedOperator);
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
            if (SelectedDataModelProp.EnumValues != null)
            {
                Operators.AddRange(
                    SelectedDataModelProp.EnumValues.Select(val => new NamedOperator(val.SplitCamelCase(), "== " + val)));
                UserValueIsVisible = false;
            }
            else
                switch (SelectedDataModelProp.Type)
                {
                    case "Int32":
                        Operators.AddRange(_int32Operators);
                        UserValueIsVisible = true;
                        break;
                    case "Boolean":
                        Operators.AddRange(_boolOperators);
                        UserValueIsVisible = false;
                        break;
                    default:
                        Operators.AddRange(_operators);
                        UserValueIsVisible = true;
                        break;
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
                e.PropertyName != "SelectedDataModelProp")
                return;

            LayerConditionModel.Field = SelectedDataModelProp.Path;
            LayerConditionModel.Operator = SelectedOperator.Value;
            LayerConditionModel.Value = UserValue;
        }

        /// <summary>
        ///     Setup the current UI elements to show the backing model
        /// </summary>
        private void PreSelect()
        {
            _preselecting = true;
            SelectedDataModelProp = DataModelProps.FirstOrDefault(m => m.Path == LayerConditionModel.Field);
            SelectedOperator = Operators.FirstOrDefault(o => o.Value == LayerConditionModel.Operator);
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