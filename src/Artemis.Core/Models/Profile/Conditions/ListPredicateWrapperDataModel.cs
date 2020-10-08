using System;
using Artemis.Core.DataModelExpansions;

namespace Artemis.Core
{
    internal class ListPredicateWrapperDataModel<T> : ListPredicateWrapperDataModel
    {
        public T Value => (UntypedValue is T typedValue ? typedValue : default)!;
    }

    public abstract class ListPredicateWrapperDataModel : DataModel
    {
        internal ListPredicateWrapperDataModel()
        {
            PluginInfo = Constants.CorePluginInfo;
        }

        [DataModelIgnore]
        public object? UntypedValue { get; set; }

        public static ListPredicateWrapperDataModel Create(Type type)
        {
            object? instance = Activator.CreateInstance(typeof(ListPredicateWrapperDataModel<>).MakeGenericType(type));
            if (instance == null)
                throw new ArtemisCoreException($"Failed to create an instance of ListPredicateWrapperDataModel<T> for type {type.Name}");

            return (ListPredicateWrapperDataModel) instance;
        }
    }
}