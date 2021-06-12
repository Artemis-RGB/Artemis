using System;

namespace Artemis.Core.Modules
{
    /// <summary>
    ///     Represents a dynamic child value with its property attribute
    /// </summary>
    public class DynamicChild<T> : DynamicChild
    {
        internal DynamicChild(T value, string key, DataModelPropertyAttribute attribute) : base(key, attribute, typeof(T))
        {
            Value = value;
        }

        /// <summary>
        ///     Gets or sets the current value of the dynamic child
        /// </summary>
        public new T Value { get; set; }

        /// <inheritdoc />
        protected override object? GetValue()
        {
            return Value;
        }
    }

    /// <summary>
    ///     Represents a dynamic child value with its property attribute
    /// </summary>
    public abstract class DynamicChild
    {
        internal DynamicChild(string key, DataModelPropertyAttribute attribute, Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            Key = key ?? throw new ArgumentNullException(nameof(key));
            Attribute = attribute ?? throw new ArgumentNullException(nameof(attribute));
            Type = type;
        }

        /// <summary>
        ///     Gets the key of the dynamic child
        /// </summary>
        public string Key { get; }

        /// <summary>
        ///     Gets the attribute describing the dynamic child
        /// </summary>
        public DataModelPropertyAttribute Attribute { get; }

        /// <summary>
        ///     Gets the type of <see cref="BaseValue" />
        /// </summary>
        public Type Type { get; }

        /// <summary>
        ///     Gets the current value of the dynamic child
        /// </summary>
        public object? BaseValue => GetValue();

        /// <summary>
        ///     Gets the current value of the dynamic child
        /// </summary>
        /// <returns>The current value of the dynamic child</returns>
        protected abstract object? GetValue();
    }
}