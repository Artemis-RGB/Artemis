using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Artemis.Storage.Entities.Profile.DataBindings;

namespace Artemis.Core
{
    /// <inheritdoc />
    public class DataBinding<TLayerProperty> : IDataBinding
    {
        private readonly List<IDataBindingProperty> _properties = new();
        private bool _disposed;
        private bool _isEnabled;

        internal DataBinding(LayerProperty<TLayerProperty> layerProperty)
        {
            LayerProperty = layerProperty;

            Entity = new DataBindingEntity();
            Script = new DataBindingNodeScript<TLayerProperty>(GetScriptName(), "The value to put into the data binding", this, LayerProperty.ProfileElement.Profile);

            Save();
        }

        internal DataBinding(LayerProperty<TLayerProperty> layerProperty, DataBindingEntity entity)
        {
            LayerProperty = layerProperty;

            Entity = entity;
            Script = new DataBindingNodeScript<TLayerProperty>(GetScriptName(), "The value to put into the data binding", this, LayerProperty.ProfileElement.Profile);

            // Load will add children so be initialized before that
            Load();
        }

        /// <summary>
        ///     Gets the layer property this data binding targets
        /// </summary>
        public LayerProperty<TLayerProperty> LayerProperty { get; }

        /// <summary>
        ///     Gets the script used to populate the data binding
        /// </summary>
        public DataBindingNodeScript<TLayerProperty> Script { get; private set; }

        /// <summary>
        ///     Gets the data binding entity this data binding uses for persistent storage
        /// </summary>
        public DataBindingEntity Entity { get; }

        /// <summary>
        ///     Updates the pending values of this data binding
        /// </summary>
        public void Update()
        {
            if (_disposed)
                throw new ObjectDisposedException("DataBinding");

            if (!IsEnabled)
                return;

            // TODO: Update the 'base value' node

            Script.Run();
        }

        /// <summary>
        ///     Registers a data binding property so that is available to the data binding system
        /// </summary>
        /// <typeparam name="TProperty">The type of the layer property</typeparam>
        /// <param name="getter">The function to call to get the value of the property</param>
        /// <param name="setter">The action to call to set the value of the property</param>
        /// <param name="displayName">The display name of the data binding property</param>
        public DataBindingProperty<TProperty> RegisterDataBindingProperty<TProperty>(Func<TProperty> getter, Action<TProperty?> setter, string displayName)
        {
            if (_disposed)
                throw new ObjectDisposedException("DataBinding");
            if (Properties.Any(d => d.DisplayName == displayName))
                throw new ArtemisCoreException($"A data binding property named '{displayName}' is already registered.");

            DataBindingProperty<TProperty> property = new(getter, setter, displayName);
            _properties.Add(property);

            OnDataBindingPropertyRegistered();
            return property;
        }

        /// <summary>
        ///     Removes all data binding properties so they are no longer available to the data binding system
        /// </summary>
        public void ClearDataBindingProperties()
        {
            if (_disposed)
                throw new ObjectDisposedException("LayerProperty");

            _properties.Clear();
            OnDataBindingPropertiesCleared();
        }

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
                _isEnabled = false;

                Script.Dispose();
            }
        }

        /// <summary>
        ///     Invokes the <see cref="DataBindingPropertyRegistered" /> event
        /// </summary>
        protected virtual void OnDataBindingPropertyRegistered()
        {
            DataBindingPropertyRegistered?.Invoke(this, new DataBindingEventArgs(this));
        }

        /// <summary>
        ///     Invokes the <see cref="DataBindingDisabled" /> event
        /// </summary>
        protected virtual void OnDataBindingPropertiesCleared()
        {
            DataBindingPropertiesCleared?.Invoke(this, new DataBindingEventArgs(this));
        }

        /// <summary>
        ///     Invokes the <see cref="DataBindingEnabled" /> event
        /// </summary>
        protected virtual void OnDataBindingEnabled(DataBindingEventArgs e)
        {
            DataBindingEnabled?.Invoke(this, e);
        }

        /// <summary>
        ///     Invokes the <see cref="DataBindingDisabled" /> event
        /// </summary>
        protected virtual void OnDataBindingDisabled(DataBindingEventArgs e)
        {
            DataBindingDisabled?.Invoke(this, e);
        }

        private string GetScriptName()
        {
            return LayerProperty.PropertyDescription.Name ?? LayerProperty.Path;
        }

        /// <inheritdoc />
        public ILayerProperty BaseLayerProperty => LayerProperty;

        /// <inheritdoc />
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                _isEnabled = value;

                if (_isEnabled)
                    OnDataBindingEnabled(new DataBindingEventArgs(this));
                else
                    OnDataBindingDisabled(new DataBindingEventArgs(this));
            }
        }

        /// <inheritdoc />
        public ReadOnlyCollection<IDataBindingProperty> Properties => _properties.AsReadOnly();

        /// <inheritdoc />
        public void Apply()
        {
            if (_disposed)
                throw new ObjectDisposedException("DataBinding");

            if (!IsEnabled)
                return;

            Script.DataBindingExitNode.ApplyToDataBinding();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public event EventHandler<DataBindingEventArgs>? DataBindingPropertyRegistered;

        /// <inheritdoc />
        public event EventHandler<DataBindingEventArgs>? DataBindingPropertiesCleared;

        /// <inheritdoc />
        public event EventHandler<DataBindingEventArgs>? DataBindingEnabled;

        /// <inheritdoc />
        public event EventHandler<DataBindingEventArgs>? DataBindingDisabled;


        #region Storage

        /// <inheritdoc />
        public void Load()
        {
            if (_disposed)
                throw new ObjectDisposedException("DataBinding");

            IsEnabled = Entity.IsEnabled;
        }

        /// <inheritdoc />
        public void LoadNodeScript()
        {
            Script.Dispose();
            Script = Entity.NodeScript != null
                ? new DataBindingNodeScript<TLayerProperty>(GetScriptName(), "The value to put into the data binding", this, Entity.NodeScript, LayerProperty.ProfileElement.Profile)
                : new DataBindingNodeScript<TLayerProperty>(GetScriptName(), "The value to put into the data binding", this, LayerProperty.ProfileElement.Profile);
        }

        /// <inheritdoc />
        public void Save()
        {
            if (_disposed)
                throw new ObjectDisposedException("DataBinding");

            Script.Save();
            Entity.IsEnabled = IsEnabled;
            Entity.NodeScript = Script.Entity.Nodes.Any() ? Script.Entity : null;
        }

        #endregion
    }
}