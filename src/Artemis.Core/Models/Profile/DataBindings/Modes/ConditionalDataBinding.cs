using Artemis.Storage.Entities.Profile.DataBindings;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a data binding mode that applies a value depending on conditions
    /// </summary>
    public class ConditionalDataBinding<TLayerProperty, TProperty> : IDataBindingMode<TLayerProperty, TProperty>
    {
        internal ConditionalDataBinding(DataBinding<TLayerProperty, TProperty> dataBinding, ConditionalDataBindingEntity entity)
        {
            DataBinding = dataBinding;
            Entity = entity;
        }

        /// <inheritdoc />
        public DataBinding<TLayerProperty, TProperty> DataBinding { get; }

        /// <inheritdoc />
        public TProperty GetValue(TProperty baseValue)
        {
            return default;
        }

        internal ConditionalDataBindingEntity Entity { get; }

        #region Storage

        /// <inheritdoc />
        public void Load()
        {
        }

        /// <inheritdoc />
        public void Save()
        {
        }

        #endregion

        #region IDisposable

        /// <inheritdoc />
        public void Dispose()
        {
        }

        #endregion
    }
}