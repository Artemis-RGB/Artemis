using System;
using System.Collections;
using System.Collections.ObjectModel;
using Artemis.Core;
using Artemis.Core.Modules;
using Artemis.UI.Shared.Services;
using ReactiveUI;

namespace Artemis.UI.Shared.DataModelVisualization.Shared
{
    /// <summary>
    ///     Represents a view model that visualizes a list data model property
    /// </summary>
    public class DataModelListViewModel : DataModelVisualizationViewModel
    {
        private string _countDisplay;
        private Type? _displayValueType;
        private IEnumerable? _list;
        private ObservableCollection<DataModelVisualizationViewModel> _listChildren;
        private int _listCount;

        internal DataModelListViewModel(DataModel dataModel, DataModelVisualizationViewModel parent, DataModelPath dataModelPath)
            : base(dataModel, parent, dataModelPath)
        {
            _countDisplay = "0 items";
            _listChildren = new ObservableCollection<DataModelVisualizationViewModel>();
        }

        /// <summary>
        ///     Gets the instance of the list that is being visualized
        /// </summary>
        public IEnumerable? List
        {
            get => _list;
            private set => this.RaiseAndSetIfChanged(ref _list, value);
        }

        /// <summary>
        ///     Gets amount of elements in the list that is being visualized
        /// </summary>
        public int ListCount
        {
            get => _listCount;
            private set => this.RaiseAndSetIfChanged(ref _listCount, value);
        }

        /// <summary>
        ///     Gets the type of elements this list contains and that must be displayed as children
        /// </summary>
        public Type? DisplayValueType
        {
            get => _displayValueType;
            set => this.RaiseAndSetIfChanged(ref _displayValueType, value);
        }

        /// <summary>
        ///     Gets a human readable display count
        /// </summary>
        public string CountDisplay
        {
            get => _countDisplay;
            set => this.RaiseAndSetIfChanged(ref _countDisplay, value);
        }

        /// <summary>
        ///     Gets a list of child view models that visualize the elements in the list
        /// </summary>
        public ObservableCollection<DataModelVisualizationViewModel> ListChildren
        {
            get => _listChildren;
            private set => this.RaiseAndSetIfChanged(ref _listChildren, value);
        }

        /// <inheritdoc />
        public override void Update(IDataModelUIService dataModelUIService, DataModelUpdateConfiguration? configuration)
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

                DataModelVisualizationViewModel? child;
                if (ListChildren.Count <= index)
                {
                    child = CreateListChild(dataModelUIService, item.GetType(), DataModelPath?.GetPropertyDescription()?.ListItemName);
                    if (child == null)
                        continue;
                    ListChildren.Add(child);
                }
                else
                    child = ListChildren[index];

                if (child is DataModelListItemViewModel dataModelListPropertyViewModel)
                {
                    dataModelListPropertyViewModel.DisplayValue = item;
                    dataModelListPropertyViewModel.Index = index;
                    dataModelListPropertyViewModel.Update(dataModelUIService, configuration);
                }

                index++;
            }

            ListCount = index;

            while (ListChildren.Count > ListCount)
                ListChildren.RemoveAt(ListChildren.Count - 1);

            CountDisplay = $"{ListChildren.Count} {(ListChildren.Count == 1 ? "item" : "items")}";
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[List] {DisplayPath ?? Path} - {ListCount} item(s)";
        }

        private DataModelVisualizationViewModel? CreateListChild(IDataModelUIService dataModelUIService, Type listType, string? name)
        {
            // If a display VM was found, prefer to use that in any case
            DataModelDisplayViewModel? typeViewModel = dataModelUIService.GetDataModelDisplayViewModel(listType, PropertyDescription);

            return typeViewModel != null
                ? new DataModelListItemViewModel(listType, typeViewModel, name)
                : new DataModelListItemViewModel(listType, name);
        }
    }
}