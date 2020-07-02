using System.Reflection;
using Artemis.Core.Extensions;
using Artemis.Core.Plugins.Abstract.DataModels.Attributes;
using Artemis.UI.Shared.Services;
using Stylet;

namespace Artemis.UI.Shared.DataModelVisualization.Shared
{
    public class DataModelViewModel : DataModelVisualizationViewModel
    {
        private readonly IDataModelVisualizationService _dataModelVisualizationService;
        private BindableCollection<DataModelVisualizationViewModel> _children;

        internal DataModelViewModel()
        {
            Children = new BindableCollection<DataModelVisualizationViewModel>();
        }

        internal DataModelViewModel(PropertyInfo propertyInfo, object model, DataModelPropertyAttribute propertyDescription, DataModelVisualizationViewModel parent,
            IDataModelVisualizationService dataModelVisualizationService)
        {
            _dataModelVisualizationService = dataModelVisualizationService;
            PropertyInfo = propertyInfo;
            Model = model;
            PropertyDescription = propertyDescription;
            Parent = parent;
            Children = new BindableCollection<DataModelVisualizationViewModel>();

            PopulateProperties();
        }

        public BindableCollection<DataModelVisualizationViewModel> Children
        {
            get => _children;
            set => SetAndNotify(ref _children, value);
        }

        public void PopulateProperties()
        {
            Children.Clear();
            foreach (var propertyInfo in Model.GetType().GetProperties())
            {
                var child = CreateChild(_dataModelVisualizationService, propertyInfo);
                if (child != null)
                    Children.Add(child);
            }
        }

        public override void Update()
        {
            if (PropertyInfo != null && PropertyInfo.PropertyType.IsStruct())
                Model = PropertyInfo.GetValue(Parent.Model);

            foreach (var dataModelVisualizationViewModel in Children)
                dataModelVisualizationViewModel.Update();

            UpdateListStatus();
        }
    }
}