using System;
using Artemis.Core;
using Artemis.UI.Shared.Services;

namespace Artemis.UI.Shared
{
    public class DataModelListPropertyViewModel : DataModelPropertyViewModel
    {
        private int _index;
        private Type _listType;

        public DataModelListPropertyViewModel(Type listType, DataModelDisplayViewModel displayViewModel) : base(null, null, null)
        {
            DataModel = ListPredicateWrapperDataModel.Create(listType);
            ListType = listType;
            DisplayViewModel = displayViewModel;
        }

        public DataModelListPropertyViewModel(Type listType) : base(null, null, null)
        {
            DataModel = ListPredicateWrapperDataModel.Create(listType);
            ListType = listType;
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

            ((ListPredicateWrapperDataModel)DataModel).UntypedValue = DisplayValue;

            if (DisplayViewModel == null)
                DisplayViewModel = dataModelUIService.GetDataModelDisplayViewModel(DisplayValue.GetType(), PropertyDescription, true);

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