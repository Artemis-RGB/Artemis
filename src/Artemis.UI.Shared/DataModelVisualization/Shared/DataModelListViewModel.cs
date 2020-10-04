using System;
using System.Collections;
using Artemis.Core;
using Artemis.Core.DataModelExpansions;
using Artemis.UI.Shared.Services;
using Stylet;

namespace Artemis.UI.Shared
{
    public class DataModelListViewModel : DataModelVisualizationViewModel
    {
        private string _count;
        private IList _list;
        private DataModelVisualizationViewModel _listTypePropertyViewModel;

        internal DataModelListViewModel(DataModel dataModel, DataModelVisualizationViewModel parent, DataModelPath dataModelPath) : base(dataModel, parent, dataModelPath)
        {
            ListChildren = new BindableCollection<DataModelVisualizationViewModel>();
        }

        public IList List
        {
            get => _list;
            set => SetAndNotify(ref _list, value);
        }

        public BindableCollection<DataModelVisualizationViewModel> ListChildren { get; set; }

        public string Count
        {
            get => _count;
            set => SetAndNotify(ref _count, value);
        }

        public DataModelPropertiesViewModel GetListTypeViewModel(IDataModelUIService dataModelUIService)
        {
            var type = DataModelPath.GetPropertyType();
            if (type == null)
                return null;

            // Create a property VM describing the type of the list
            var viewModel = CreateListChild(dataModelUIService, type.GenericTypeArguments[0]);

            // Put an empty value into the list type property view model
            if (viewModel is DataModelListPropertiesViewModel dataModelListClassViewModel)
            {
                dataModelListClassViewModel.DisplayValue = Activator.CreateInstance(dataModelListClassViewModel.ListType);
                dataModelListClassViewModel.Update(dataModelUIService);
                return dataModelListClassViewModel;
            }

            if (viewModel is DataModelListPropertyViewModel dataModelListPropertyViewModel)
            {
                dataModelListPropertyViewModel.DisplayValue = Activator.CreateInstance(dataModelListPropertyViewModel.ListType);
                var wrapper = new DataModelPropertiesViewModel(null, null, null);
                wrapper.Children.Add(dataModelListPropertyViewModel);
                return wrapper;
            }

            return null;
        }

        public override void Update(IDataModelUIService dataModelUIService)
        {
            if (Parent != null && !Parent.IsVisualizationExpanded)
                return;

            List = GetCurrentValue() as IList;
            if (List == null)
                return;

            var index = 0;
            foreach (var item in List)
            {
                DataModelVisualizationViewModel child;
                if (ListChildren.Count <= index)
                {
                    child = CreateListChild(dataModelUIService, item);
                    ListChildren.Add(child);
                }
                else
                    child = ListChildren[index];

                if (child is DataModelListPropertiesViewModel dataModelListClassViewModel)
                {
                    dataModelListClassViewModel.DisplayValue = item;
                    dataModelListClassViewModel.Index = index;
                }
                else if (child is DataModelListPropertyViewModel dataModelListPropertyViewModel)
                {
                    dataModelListPropertyViewModel.DisplayValue = item;
                    dataModelListPropertyViewModel.Index = index;
                }

                child.Update(dataModelUIService);
                index++;
            }

            while (ListChildren.Count > List.Count)
                ListChildren.RemoveAt(ListChildren.Count - 1);

            Count = $"{ListChildren.Count} {(ListChildren.Count == 1 ? "item" : "items")}";
        }

        protected DataModelVisualizationViewModel CreateListChild(IDataModelUIService dataModelUIService, object listItem)
        {
            var listType = listItem.GetType();
            var path = new DataModelPath(listItem, "");

            // If a display VM was found, prefer to use that in any case
            var typeViewModel = dataModelUIService.GetDataModelDisplayViewModel(listType);
            if (typeViewModel != null)
                return new DataModelListPropertyViewModel(DataModel, this, path) {DisplayViewModel = typeViewModel};
            // For primitives, create a property view model, it may be null that is fine
            if (listType.IsPrimitive || listType.IsEnum || listType == typeof(string))
                return new DataModelListPropertyViewModel(DataModel, this, path) {ListType = listType};
            // For other value types create a child view model
            if (listType.IsClass || listType.IsStruct())
                return new DataModelListPropertiesViewModel(DataModel, this, path) {ListType = listType};

            return null;
        }
    }
}