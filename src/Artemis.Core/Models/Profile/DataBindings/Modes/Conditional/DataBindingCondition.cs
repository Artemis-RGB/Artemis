using System;
using Artemis.Storage.Entities.Profile.DataBindings;
using Newtonsoft.Json;

namespace Artemis.Core
{
    /// <inheritdoc />
    public class DataBindingCondition<TLayerProperty, TProperty> : IDataBindingCondition
    {
        private bool _disposed;

        /// <summary>
        ///     Creates a new instance of the <see cref="DataBindingCondition{TLayerProperty,TProperty}" /> class
        /// </summary>
        /// <param name="conditionalDataBinding">The conditional data binding this condition is applied too</param>
        internal DataBindingCondition(ConditionalDataBinding<TLayerProperty, TProperty> conditionalDataBinding)
        {
            ConditionalDataBinding = conditionalDataBinding ?? throw new ArgumentNullException(nameof(conditionalDataBinding));
            Order = conditionalDataBinding.Conditions.Count + 1;
            Condition = new DataModelConditionGroup(null);
            Entity = new DataBindingConditionEntity();
            Save();
        }

        internal DataBindingCondition(ConditionalDataBinding<TLayerProperty, TProperty> conditionalDataBinding, DataBindingConditionEntity entity)
        {
            ConditionalDataBinding = conditionalDataBinding ?? throw new ArgumentNullException(nameof(conditionalDataBinding));
            Entity = entity;
            Load();
        }

        /// <summary>
        ///     Gets the conditional data binding this condition is applied to
        /// </summary>
        public ConditionalDataBinding<TLayerProperty, TProperty> ConditionalDataBinding { get; }

        /// <summary>
        ///     Gets or sets the position at which the modifier appears on the data binding
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        ///     Gets or sets the value to be applied when the condition is met
        /// </summary>
        public TProperty Value { get; set; }

        /// <summary>
        ///     Gets the root group of the condition that must be met
        /// </summary>
        public DataModelConditionGroup Condition { get; private set; }

        internal DataBindingConditionEntity Entity { get; set; }

        /// <inheritdoc />
        public bool Evaluate()
        {
            return Condition.Evaluate();
        }

        /// <inheritdoc />
        public void Save()
        {
            if (_disposed)
                throw new ObjectDisposedException("DataBindingCondition");

            if (!ConditionalDataBinding.Entity.Values.Contains(Entity))
                ConditionalDataBinding.Entity.Values.Add(Entity);

            Entity.Condition = Condition.Entity;
            Condition.Save();

            Entity.Value = JsonConvert.SerializeObject(Value);
            Entity.Order = Order;
        }

        /// <inheritdoc />
        public void Load()
        {
            if (_disposed)
                throw new ObjectDisposedException("DataBindingCondition");

            Condition = Entity.Condition != null
                ? new DataModelConditionGroup(null, Entity.Condition)
                : new DataModelConditionGroup(null);

            Value = Entity.Value == null ? default : JsonConvert.DeserializeObject<TProperty>(Entity.Value);
            Order = Entity.Order;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _disposed = true;

            Condition.Dispose();
        }
    }
}