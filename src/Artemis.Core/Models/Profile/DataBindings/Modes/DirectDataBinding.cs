using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Artemis.Storage.Entities.Profile.DataBindings;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a data binding mode that directly applies a data model value to a data binding
    /// </summary>
    public class DirectDataBinding<TLayerProperty, TProperty> : IDataBindingMode<TLayerProperty, TProperty>
    {
        private readonly List<DataBindingModifier<TLayerProperty, TProperty>> _modifiers = new List<DataBindingModifier<TLayerProperty, TProperty>>();
        private bool _disposed;

        internal DirectDataBinding(DataBinding<TLayerProperty, TProperty> dataBinding, DirectDataBindingEntity entity)
        {
            DataBinding = dataBinding;
            Entity = entity;

            Load();
        }

        /// <summary>
        ///     Gets the path of the source property
        /// </summary>
        public DataModelPath? SourcePath { get; private set; }

        /// <summary>
        ///     Gets a list of modifiers applied to this data binding
        /// </summary>
        public ReadOnlyCollection<DataBindingModifier<TLayerProperty, TProperty>> Modifiers => _modifiers.AsReadOnly();

        internal DirectDataBindingEntity Entity { get; }

        /// <inheritdoc />
        public DataBinding<TLayerProperty, TProperty> DataBinding { get; }

        /// <inheritdoc />
        public TProperty GetValue(TProperty baseValue)
        {
            if (_disposed)
                throw new ObjectDisposedException("DirectDataBinding");

            if (SourcePath == null || !SourcePath.IsValid || DataBinding.Converter == null)
                return baseValue;

            object? dataBindingValue = SourcePath.GetValue();
            foreach (DataBindingModifier<TLayerProperty, TProperty> dataBindingModifier in Modifiers)
                dataBindingValue = dataBindingModifier.Apply(dataBindingValue);

            return DataBinding.Converter.ConvertFromObject(dataBindingValue);
        }

        #region IDisposable

        /// <inheritdoc />
        public void Dispose()
        {
            _disposed = true;

            foreach (DataBindingModifier<TLayerProperty, TProperty> dataBindingModifier in Modifiers)
                dataBindingModifier.Dispose();

            SourcePath?.Dispose();
        }

        #endregion

        #region Storage

        /// <inheritdoc />
        public void Load()
        {
            // Source
            if (Entity.SourcePath != null)
                SourcePath = new DataModelPath(null, Entity.SourcePath);

            // Modifiers
            foreach (DataBindingModifierEntity dataBindingModifierEntity in Entity.Modifiers)
                _modifiers.Add(new DataBindingModifier<TLayerProperty, TProperty>(this, dataBindingModifierEntity));

            ApplyOrder();
        }

        /// <inheritdoc />
        public void Save()
        {
            // Don't save an invalid state
            if (SourcePath != null && !SourcePath.IsValid)
                return;

            SourcePath?.Save();
            Entity.SourcePath = SourcePath?.Entity;

            // Modifiers
            Entity.Modifiers.Clear();
            foreach (DataBindingModifier<TLayerProperty, TProperty> dataBindingModifier in Modifiers)
                dataBindingModifier.Save();
        }

        #endregion

        #region Source

        /// <summary>
        ///     Returns the type of the source property of this data binding
        /// </summary>
        public Type? GetSourceType()
        {
            return SourcePath?.GetPropertyType();
        }

        /// <summary>
        ///     Updates the source of the data binding
        /// </summary>
        /// <param name="path">The path pointing to the source</param>
        public void UpdateSource(DataModelPath? path)
        {
            if (_disposed)
                throw new ObjectDisposedException("DirectDataBinding");

            if (path != null && !path.IsValid)
                throw new ArtemisCoreException("Cannot update source of data binding to an invalid path");

            SourcePath?.Dispose();
            SourcePath = path != null ? new DataModelPath(path) : null;
        }

        #endregion

        #region Modifiers

        /// <summary>
        ///     Adds a modifier to the direct data binding's <see cref="Modifiers" /> collection
        /// </summary>
        /// <param name="type">The type of the parameter, can either be dynamic (based on a data model value) or static</param>
        public DataBindingModifier<TLayerProperty, TProperty> AddModifier(ProfileRightSideType type)
        {
            if (_disposed)
                throw new ObjectDisposedException("DirectDataBinding");

            DataBindingModifier<TLayerProperty, TProperty> modifier = new DataBindingModifier<TLayerProperty, TProperty>(this, type);
            _modifiers.Add(modifier);

            ApplyOrder();
            OnModifiersUpdated();

            return modifier;
        }

        /// <summary>
        ///     Removes a modifier from the direct data binding's <see cref="Modifiers" /> collection and disposes it
        /// </summary>
        public void RemoveModifier(DataBindingModifier<TLayerProperty, TProperty> modifier)
        {
            if (_disposed)
                throw new ObjectDisposedException("DirectDataBinding");
            if (!_modifiers.Contains(modifier))
                return;

            _modifiers.Remove(modifier);
            modifier.Dispose();

            ApplyOrder();
            OnModifiersUpdated();
        }

        /// <summary>
        ///     Applies the current order of conditions to the <see cref="Modifiers" /> collection
        /// </summary>
        public void ApplyOrder()
        {
            _modifiers.Sort((a, b) => a.Order.CompareTo(b.Order));
            for (int index = 0; index < _modifiers.Count; index++)
            {
                DataBindingModifier<TLayerProperty, TProperty> modifier = _modifiers[index];
                modifier.Order = index + 1;
            }
        }

        #endregion

        #region Events

        /// <summary>
        ///     Occurs when a modifier is added or removed
        /// </summary>
        public event EventHandler? ModifiersUpdated;

        /// <summary>
        ///     Invokes the <see cref="ModifiersUpdated" /> event
        /// </summary>
        protected virtual void OnModifiersUpdated()
        {
            ModifiersUpdated?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}