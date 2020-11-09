using System;
using Artemis.Core.DataModelExpansions;

namespace Artemis.Core
{
    internal class ListPredicateWrapperDataModel<T> : ListPredicateWrapperDataModel
    {
        [DataModelProperty(Name = "List item", Description = "The current item in the list")]
        public T Value => (UntypedValue is T typedValue ? typedValue : default)!;
    }

    /// <summary>
    ///     Represents a datamodel that wraps a value in a list
    /// </summary>
    public abstract class ListPredicateWrapperDataModel : DataModel
    {
        internal ListPredicateWrapperDataModel()
        {
            Implementation = Constants.CorePluginInfo;
        }

        [DataModelIgnore]
        public object? UntypedValue { get; set; }

        /// <summary>
        /// Creates a new instance of the <see cref="ListPredicateWrapperDataModel"/> class
        /// </summary>
        public static ListPredicateWrapperDataModel Create(Type type)
        {
            object? instance = Activator.CreateInstance(typeof(ListPredicateWrapperDataModel<>).MakeGenericType(type));
            if (instance == null)
                throw new ArtemisCoreException($"Failed to create an instance of ListPredicateWrapperDataModel<T> for type {type.Name}");

            return (ListPredicateWrapperDataModel) instance;
        }
    }
}