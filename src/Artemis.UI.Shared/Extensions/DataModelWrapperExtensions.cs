using Artemis.Core;

namespace Artemis.UI.Shared
{
    public static class DataModelWrapperExtensions
    {
        public static DataModelPropertiesViewModel CreateViewModel(this EventPredicateWrapperDataModel wrapper)
        {
            return new DataModelPropertiesViewModel(wrapper, null, new DataModelPath(wrapper));
        }

        public static DataModelPropertiesViewModel CreateViewModel(this ListPredicateWrapperDataModel wrapper)
        {
            return new DataModelPropertiesViewModel(wrapper, null, new DataModelPath(wrapper));
        }
    }
}