using System;
using System.Reflection;
using Artemis.Core.Modules;

namespace Artemis.Core
{
    internal class ListPredicateWrapperDataModel<T> : ListPredicateWrapperDataModel
    {
        public T Value => (UntypedValue is T typedValue ? typedValue : default)!;
    }

    /// <summary>
    ///     Represents a datamodel that wraps a value in a list
    /// </summary>
    public abstract class ListPredicateWrapperDataModel : DataModel
    {
        internal ListPredicateWrapperDataModel()
        {
            Module = Constants.CorePluginFeature;
        }

        /// <summary>
        ///     Gets or sets the value of this list as an object
        /// </summary>
        [DataModelIgnore]
        public object? UntypedValue { get; set; }

        /// <summary>
        ///     Gets or sets the name of the list item
        /// </summary>
        [DataModelIgnore]
        public string? ItemName { get; set; }

        #region Overrides of DataModel

        /// <inheritdoc />
        public override DataModelPropertyAttribute? GetPropertyDescription(PropertyInfo propertyInfo)
        {
            if (!string.IsNullOrWhiteSpace(ItemName))
                return new DataModelPropertyAttribute {Name = ItemName};
            return base.GetPropertyDescription(propertyInfo);
        }

        #endregion

        /// <summary>
        ///     Creates a new instance of the <see cref="ListPredicateWrapperDataModel" /> class
        /// </summary>
        public static ListPredicateWrapperDataModel Create(Type type, string? name = null)
        {
            ListPredicateWrapperDataModel? instance = Activator.CreateInstance(typeof(ListPredicateWrapperDataModel<>).MakeGenericType(type)) as ListPredicateWrapperDataModel;
            if (instance == null)
                throw new ArtemisCoreException($"Failed to create an instance of ListPredicateWrapperDataModel<T> for type {type.Name}");
            
            instance.ItemName = name;
            return instance;
        }
    }
}