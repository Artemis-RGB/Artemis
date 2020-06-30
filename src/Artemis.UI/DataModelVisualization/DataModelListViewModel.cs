using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Artemis.Core.Plugins.Abstract.DataModels.Attributes;
using Stylet;

namespace Artemis.UI.DataModelVisualization
{
    public class DataModelListViewModel : DataModelVisualizationViewModel
    {
        private BindableCollection<DataModelVisualizationViewModel> _children;
        private IList _list;
        private string _count;

        public DataModelListViewModel(PropertyInfo propertyInfo, DataModelPropertyAttribute propertyDescription, DataModelVisualizationViewModel parent)
        {
            PropertyInfo = propertyInfo;
            Parent = parent;
            PropertyDescription = propertyDescription;
            Children = new BindableCollection<DataModelVisualizationViewModel>();
        }

        public BindableCollection<DataModelVisualizationViewModel> Children
        {
            get => _children;
            set => SetAndNotify(ref _children, value);
        }

        public IList List
        {
            get => _list;
            set => SetAndNotify(ref _list, value);
        }

        public string Count
        {
            get => _count;
            set => SetAndNotify(ref _count, value);
        }

        public override void Update()
        {
            if (PropertyInfo != null && Parent?.Model != null && PropertyInfo.GetValue(Parent.Model) is IList listValue)
            {
                Model = new List<object>(listValue.Cast<object>());
                List = (IList) Model;
            }

            var index = 0;
            foreach (var item in List)
            {
                DataModelVisualizationViewModel child;
                if (Children.Count <= index)
                {
                    child = CreateChild(item);
                    Children.Add(child);
                }
                else
                {
                    child = Children[index];
                    child.Model = item;
                }

                child.Update();
                index++;
            }

            while (Children.Count > List.Count)
                Children.RemoveAt(Children.Count - 1);

            Count = $"{Children.Count} {(Children.Count == 1 ? "item" : "items")}";
        }
    }
}