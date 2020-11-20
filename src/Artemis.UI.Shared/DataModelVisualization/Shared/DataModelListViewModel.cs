using System;
using System.Collections;
using Artemis.Core;
using Artemis.Core.DataModelExpansions;
using Artemis.UI.Shared.Services;
using Stylet;

namespace Artemis.UI.Shared
{
    /// <summary>
    ///     Represents a view model that visualizes a list data model property
    /// </summary>
    public class DataModelListViewModel : DataModelVisualizationViewModel
    {
        private string _countDisplay;
        private Type _displayValueType;
        private IEnumerable _list;
        private BindableCollection<DataModelVisualizationViewModel> _listChildren;
        private int _listCount;

        internal DataModelListViewModel(DataModel dataModel, DataModelVisualizationViewModel parent, DataModelPath dataModelPath)
            : base(dataModel, parent, dataModelPath)
        {
            ListChildren = new BindableCollection<DataModelVisualizationViewModel>();
        }

        /// <summary>
        ///     Gets the instance of the list that is being visualized
        /// </summary>
        public IEnumerable List
        {
            get => _list;
            private set => SetAndNotify(ref _list, value);
        }

        /// <summary>
        ///     Gets amount of elements in the list that is being visualized
        /// </summary>
        public int ListCount
        {
            get => _listCount;
            private set => SetAndNotify(ref _listCount, value);
        }

        /// <summary>
        ///     Gets the type of elements this list contains and that must be displayed as children
        /// </summary>
        public Type DisplayValueType
        {
            get => _displayValueType;
            set => SetAndNotify(ref _displayValueType, value);
        }

        /// <summary>
        ///     Gets a human readable display count
        /// </summary>
        public string CountDisplay
        {
            get => _countDisplay;
            set => SetAndNotify(ref _countDisplay, value);
        }

        /// <summary>
        ///     Gets a list of child view models that visualize the elements in the list
        /// </summary>
        public BindableCollection<DataModelVisualizationViewModel> ListChildren
        {
            get => _listChildren;
            private set => SetAndNotify(ref _listChildren, value);
        }

        /// <inheritdoc />
        public override void Update(IDataModelUIService dataModelUIService, DataModelUpdateConfiguration configuration)
        {
            if (Parent != null && !Parent.IsVisualizationExpanded)
                return;

            List = GetCurrentValue() as IEnumerable;
            DisplayValueType = List?.GetType();
            if (List == null)
                return;

            int index = 0;
            foreach (object item in List)
            {
                if (item == null)
                    continue;

                DataModelVisualizationViewModel child;
                if (ListChildren.Count <= index)
                {
                    child = CreateListChild(dataModelUIService, item.GetType());
                    ListChildren.Add(child);
                }
                else
                {
                    child = ListChildren[index];
                }

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

                child.Update(dataModelUIService, configuration);
                index++;
            }

            ListCount = index + 1;

            while (ListChildren.Count > ListCount)
                ListChildren.RemoveAt(ListChildren.Count - 1);

            CountDisplay = $"{ListChildren.Count} {(ListChildren.Count == 1 ? "item" : "items")}";
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[List] {DisplayPath ?? Path} - {ListCount} item(s)";
        }

        private DataModelVisualizationViewModel CreateListChild(IDataModelUIService dataModelUIService, Type listType)
        {
            // If a display VM was found, prefer to use that in any case
            DataModelDisplayViewModel typeViewModel = dataModelUIService.GetDataModelDisplayViewModel(listType, PropertyDescription);
            if (typeViewModel != null)
                return new DataModelListPropertyViewModel(listType, typeViewModel);
            // For primitives, create a property view model, it may be null that is fine
            if (listType.IsPrimitive || listType.IsEnum || listType == typeof(string))
                return new DataModelListPropertyViewModel(listType);
            // For other value types create a child view model
            if (listType.IsClass || listType.IsStruct())
                return new DataModelListPropertiesViewModel(listType);

            return null;
        }
    }
}