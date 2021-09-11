using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Artemis.Storage.Entities.Profile.DataBindings;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a data binding mode that applies a value depending on conditions
    /// </summary>
    public class ConditionalDataBinding<TLayerProperty, TProperty> : IDataBindingMode<TLayerProperty, TProperty>
    {
        private readonly List<DataBindingCondition<TLayerProperty, TProperty>> _conditions = new();
        private bool _disposed;

        internal ConditionalDataBinding(DataBinding<TLayerProperty, TProperty> dataBinding, ConditionalDataBindingEntity entity)
        {
            DataBinding = dataBinding;
            Entity = entity;
            Conditions = new(_conditions);
            Load();
        }

        /// <summary>
        ///     Gets a list of conditions applied to this data binding
        /// </summary>
        public ReadOnlyCollection<DataBindingCondition<TLayerProperty, TProperty>> Conditions { get; }

        internal ConditionalDataBindingEntity Entity { get; }

        /// <inheritdoc />
        public DataBinding<TLayerProperty, TProperty> DataBinding { get; }

        /// <inheritdoc />
        public TProperty GetValue(TProperty baseValue)
        {
            if (_disposed)
                throw new ObjectDisposedException("ConditionalDataBinding");

            DataBindingCondition<TLayerProperty, TProperty>? condition = Conditions.FirstOrDefault(c => c.Evaluate());
            return condition == null ? baseValue : condition.Value;
        }

        #region IDisposable

        /// <summary>
        ///     Releases the unmanaged resources used by the object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">
        ///     <see langword="true" /> to release both managed and unmanaged resources;
        ///     <see langword="false" /> to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _disposed = true;

                foreach (DataBindingCondition<TLayerProperty, TProperty> dataBindingCondition in Conditions)
                    dataBindingCondition.Dispose();
            }
        }
        
        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Values

        /// <summary>
        ///     Adds a condition to the conditional data binding's <see cref="Conditions" /> collection
        /// </summary>
        /// <returns>The newly created <see cref="DataBindingCondition{TLayerProperty,TProperty}" /></returns>
        public DataBindingCondition<TLayerProperty, TProperty> AddCondition()
        {
            if (_disposed)
                throw new ObjectDisposedException("ConditionalDataBinding");

            DataBindingCondition<TLayerProperty, TProperty> condition = new(this);
            _conditions.Add(condition);

            ApplyOrder();
            OnConditionsUpdated();

            return condition;
        }

        /// <summary>
        ///     Removes a condition from the conditional data binding's <see cref="Conditions" /> collection and disposes it
        /// </summary>
        /// <param name="condition"></param>
        public void RemoveCondition(DataBindingCondition<TLayerProperty, TProperty> condition)
        {
            if (_disposed)
                throw new ObjectDisposedException("ConditionalDataBinding");
            if (!_conditions.Contains(condition))
                return;

            _conditions.Remove(condition);
            condition.Dispose();

            ApplyOrder();
            OnConditionsUpdated();
        }

        /// <summary>
        ///     Applies the current order of conditions to the <see cref="Conditions" /> collection
        /// </summary>
        public void ApplyOrder()
        {
            if (_disposed)
                throw new ObjectDisposedException("ConditionalDataBinding");

            _conditions.Sort((a, b) => a.Order.CompareTo(b.Order));
            for (int index = 0; index < _conditions.Count; index++)
            {
                DataBindingCondition<TLayerProperty, TProperty> condition = _conditions[index];
                condition.Order = index + 1;
            }
        }

        #endregion

        #region Storage

        /// <inheritdoc />
        public void Load()
        {
            foreach (DataBindingConditionEntity dataBindingConditionEntity in Entity.Values)
                _conditions.Add(new DataBindingCondition<TLayerProperty, TProperty>(this, dataBindingConditionEntity));

            ApplyOrder();
        }

        /// <inheritdoc />
        public void Save()
        {
            Entity.Values.Clear();
            foreach (DataBindingCondition<TLayerProperty, TProperty> dataBindingCondition in Conditions)
                dataBindingCondition.Save();
        }

        #endregion

        #region Events

        /// <summary>
        ///     Occurs when a condition is added or removed
        /// </summary>
        public event EventHandler? ConditionsUpdated;

        /// <summary>
        ///     Invokes the <see cref="ConditionsUpdated" /> event
        /// </summary>
        protected virtual void OnConditionsUpdated()
        {
            ConditionsUpdated?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}