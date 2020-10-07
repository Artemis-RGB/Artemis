using System;
using Artemis.Core;
using Artemis.UI.Shared.Services;

namespace Artemis.UI.Shared
{
    public class DataModelListPropertyViewModel : DataModelPropertyViewModel
    {
        private int _index;
        private Type _listType;

        public DataModelListPropertyViewModel(object listItem, DataModelDisplayViewModel displayViewModel) : base(null, null, null)
        {
            DataModel = ListPredicateWrapperDataModel.Create(listItem.GetType());
            ListType = listItem.GetType();
            DisplayValue = listItem;
            DisplayViewModel = displayViewModel;
        }

        public DataModelListPropertyViewModel(object listItem) : base(null, null, null)
        {
            DataModel = ListPredicateWrapperDataModel.Create(listItem.GetType());
            ListType = listItem.GetType();
            DisplayValue = listItem;
        }

        public int Index
        {
            get => _index;
            set => SetAndNotify(ref _index, value);
        }

        public Type ListType
        {
            get => _listType;
            set => SetAndNotify(ref _listType, value);
        }

        public override object GetCurrentValue()
        {
            return DisplayValue;
        }

        public override void Update(IDataModelUIService dataModelUIService)
        {
            // Display value gets updated by parent, don't do anything if it is null
            if (DisplayValue == null)
                return;

            if (DisplayViewModel == null)
                DisplayViewModel = dataModelUIService.GetDataModelDisplayViewModel(DisplayValue.GetType(), true);

            ListType = DisplayValue.GetType();
            UpdateDisplayParameters();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[List item {Index}] {DisplayPath ?? Path} - {DisplayValue}";
        }
    }
}