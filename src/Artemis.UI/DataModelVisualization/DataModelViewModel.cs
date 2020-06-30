using System.Reflection;
using Artemis.Core.Extensions;
using Artemis.Core.Plugins.Abstract.DataModels.Attributes;
using Stylet;

namespace Artemis.UI.DataModelVisualization
{
    public class DataModelViewModel : DataModelVisualizationViewModel
    {
        private BindableCollection<DataModelVisualizationViewModel> _children;

        public DataModelViewModel()
        {
            Children = new BindableCollection<DataModelVisualizationViewModel>();
        }

        public DataModelViewModel(PropertyInfo propertyInfo, object model, DataModelPropertyAttribute propertyDescription, DataModelVisualizationViewModel parent)
        {
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
                var child = CreateChild(propertyInfo);
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