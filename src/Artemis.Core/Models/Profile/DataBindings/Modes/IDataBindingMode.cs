using System;

namespace Artemis.Core
{
    /// <summary>
    /// Represents a data binding mode
    /// </summary>
    public interface IDataBindingMode<TLayerProperty, TProperty> : IStorageModel, IDisposable
    {
        /// <summary>
        ///     Gets the data binding this mode is applied to
        /// </summary>
        DataBinding<TLayerProperty, TProperty> DataBinding { get; }

        /// <summary>
        ///     Gets the current value of the data binding
        /// </summary>
        /// <param name="baseValue">The base value of the property the data binding is applied to</param>
        /// <returns></returns>
        TProperty GetValue(TProperty baseValue);
    }
}
