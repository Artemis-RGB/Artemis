using System.Linq;
using Artemis.Models.Profiles;
using Artemis.Utilities;
using Caliburn.Micro;

namespace Artemis.ViewModels.LayerEditor
{
    public class LayerConditionViewModel<T> : Screen
    {
        private readonly LayerEditorViewModel<T> _conditionModel;
        private GeneralHelpers.PropertyCollection _selectedDataModelProp;
        private string _selectedOperator;
        private bool _userValueIsVisible;

        public LayerConditionViewModel(LayerEditorViewModel<T> conditionModel, LayerConditionModel layerConditionModel,
            BindableCollection<GeneralHelpers.PropertyCollection> dataModelProps)
        {
            _conditionModel = conditionModel;

            LayerConditionModel = layerConditionModel;
            DataModelProps = dataModelProps;
            Operators = new BindableCollection<string>();
        }

        public LayerConditionModel LayerConditionModel { get; set; }
        public BindableCollection<GeneralHelpers.PropertyCollection> DataModelProps { get; set; }
        public BindableCollection<string> Operators { get; set; }

        public GeneralHelpers.PropertyCollection SelectedDataModelProp
        {
            get { return _selectedDataModelProp; }
            set
            {
                if (value.Equals(_selectedDataModelProp)) return;
                _selectedDataModelProp = value;
                OnSelectedItemChangedAction(_selectedDataModelProp);
                NotifyOfPropertyChange(() => SelectedDataModelProp);
            }
        }

        public string SelectedOperator
        {
            get { return _selectedOperator; }
            set
            {
                if (value == _selectedOperator) return;
                _selectedOperator = value;
                NotifyOfPropertyChange(() => SelectedOperator);
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

        public void OnSelectedItemChangedAction(GeneralHelpers.PropertyCollection prop)
        {
            Operators.Clear();
            if (prop.EnumValues != null)
            {
                Operators.AddRange(prop.EnumValues);
                UserValueIsVisible = false;
            }
            else
                switch (prop.Type)
                {
                    case "Int32":
                        Operators.AddRange(new[]
                        {
                            "Lower than", "Lower or equal to", "Higher than", "Higher or equal to", "Equal to",
                            "Not equal to"
                        });
                        UserValueIsVisible = true;
                        break;
                    case "Boolean":
                        Operators.AddRange(new[] {"False", "True"});
                        UserValueIsVisible = false;
                        break;
                    default:
                        Operators.AddRange(new[] {"Equal to", "Not equal to"});
                        UserValueIsVisible = true;
                        break;
                }

            SelectedOperator = Operators.First();
        }

        public void Delete()
        {
            _conditionModel.DeleteCondition(this, LayerConditionModel);
        }
    }
}