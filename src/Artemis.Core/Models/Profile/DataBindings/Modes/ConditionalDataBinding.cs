﻿using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Storage.Entities.Profile.DataBindings;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a data binding mode that applies a value depending on conditions
    /// </summary>
    public class ConditionalDataBinding<TLayerProperty, TProperty> : IDataBindingMode<TLayerProperty, TProperty>
    {
        private readonly List<DataBindingCondition<TLayerProperty, TProperty>> _conditions = new List<DataBindingCondition<TLayerProperty, TProperty>>();
        private bool _disposed;

        internal ConditionalDataBinding(DataBinding<TLayerProperty, TProperty> dataBinding, ConditionalDataBindingEntity entity)
        {
            DataBinding = dataBinding;
            Entity = entity;
        }

        internal ConditionalDataBindingEntity Entity { get; }

        /// <summary>
        ///     Gets a list of conditions applied to this data binding
        /// </summary>
        public IReadOnlyList<DataBindingCondition<TLayerProperty, TProperty>> Conditions => _conditions.AsReadOnly();

        /// <inheritdoc />
        public DataBinding<TLayerProperty, TProperty> DataBinding { get; }

        /// <inheritdoc />
        public TProperty GetValue(TProperty baseValue)
        {
            if (_disposed)
                throw new ObjectDisposedException("ConditionalDataBinding");

            var condition = Conditions.FirstOrDefault(c => c.Evaluate());
            return condition == null ? baseValue : condition.Value;
        }

        #region IDisposable

        /// <inheritdoc />
        public void Dispose()
        {
            _disposed = true;

            foreach (var dataBindingCondition in Conditions)
                dataBindingCondition.Dispose();
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

            var condition = new DataBindingCondition<TLayerProperty, TProperty>(this);
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

        internal void ApplyOrder()
        {
            _conditions.Sort((a, b) => a.Order.CompareTo(b.Order));
            for (var index = 0; index < _conditions.Count; index++)
            {
                var condition = _conditions[index];
                condition.Order = index + 1;
            }
        }

        #endregion


        #region Storage

        /// <inheritdoc />
        public void Load()
        {
            foreach (var dataBindingConditionEntity in Entity.Values)
                _conditions.Add(new DataBindingCondition<TLayerProperty, TProperty>(this, dataBindingConditionEntity));

            ApplyOrder();
        }

        /// <inheritdoc />
        public void Save()
        {
            Entity.Values.Clear();
            foreach (var dataBindingCondition in Conditions)
                dataBindingCondition.Save();
        }

        #endregion

        #region Events

        /// <summary>
        ///     Occurs when a condition is added or removed
        /// </summary>
        public event EventHandler ConditionsUpdated;

        protected virtual void OnConditionsUpdated()
        {
            ConditionsUpdated?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}