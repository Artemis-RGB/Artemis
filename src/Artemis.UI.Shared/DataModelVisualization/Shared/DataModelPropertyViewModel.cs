using System.Reflection;
using Artemis.Core.Plugins.Abstract.DataModels.Attributes;

namespace Artemis.UI.Shared.DataModelVisualization.Shared
{
    public class DataModelPropertyViewModel : DataModelVisualizationViewModel
    {
        private DataModelDisplayViewModel _displayViewModel;
        private bool _showNull;
        private bool _showToString;
        private bool _showViewModel;

        internal DataModelPropertyViewModel(PropertyInfo propertyInfo, DataModelPropertyAttribute propertyDescription, DataModelVisualizationViewModel parent)
        {
            PropertyInfo = propertyInfo;
            Parent = parent;
            PropertyDescription = propertyDescription;
        }

        public DataModelDisplayViewModel DisplayViewModel
        {
            get => _displayViewModel;
            set => SetAndNotify(ref _displayViewModel, value);
        }

        public bool ShowToString
        {
            get => _showToString;
            set => SetAndNotify(ref _showToString, value);
        }

        public bool ShowNull
        {
            get => _showNull;
            set => SetAndNotify(ref _showNull, value);
        }

        public bool ShowViewModel
        {
            get => _showViewModel;
            set => SetAndNotify(ref _showViewModel, value);
        }

        public override void Update()
        {
            if (PropertyInfo != null && Parent?.Model != null)
            {
                Model = PropertyInfo.GetValue(Parent.Model);
                DisplayViewModel?.UpdateValue(Model);
            }

            ShowToString = Model != null && DisplayViewModel == null;
            ShowNull = Model == null;
            ShowViewModel = Model != null && DisplayViewModel != null;

            UpdateListStatus();
        }
    }
}