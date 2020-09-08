using System;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a data binding converter that acts as the bridge between a
    ///     <see cref="DataBinding{TLayerProperty, TProperty}" /> and a <see cref="LayerProperty{T}" />
    /// </summary>
    public interface IDataBindingConverter
    {
        /// <summary>
        ///     Gets the type this converter supports
        /// </summary>
        public Type SupportedType { get; }
    }
}