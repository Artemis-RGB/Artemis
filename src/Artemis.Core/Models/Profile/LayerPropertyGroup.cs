using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Artemis.Core.Annotations;
using Artemis.Core.Events;
using Artemis.Core.Exceptions;
using Artemis.Core.Models.Profile.LayerProperties;
using Artemis.Core.Models.Profile.LayerProperties.Attributes;
using Artemis.Core.Plugins.Exceptions;
using Artemis.Core.Services.Interfaces;
using Artemis.Storage.Entities.Profile;

namespace Artemis.Core.Models.Profile
{
    public abstract class LayerPropertyGroup
    {
        private readonly List<BaseLayerProperty> _layerProperties;
        private readonly List<LayerPropertyGroup> _layerPropertyGroups;
        private ReadOnlyCollection<BaseLayerProperty> _allLayerProperties;

        protected LayerPropertyGroup()
        {
            _layerProperties = new List<BaseLayerProperty>();
            _layerPropertyGroups = new List<LayerPropertyGroup>();
        }

        /// <summary>
        ///     The layer this property group applies to
        /// </summary>
        public Layer Layer { get; internal set; }

        /// <summary>
        ///     The path of this property group
        /// </summary>
        public string Path { get; internal set; }

        /// <summary>
        ///     The parent group of this layer property group, set after construction
        /// </summary>
        public LayerPropertyGroup Parent { get; internal set; }

        /// <summary>
        ///     Gets whether this property group's properties are all initialized
        /// </summary>
        public bool PropertiesInitialized { get; private set; }

        /// <summary>
        ///     Used to declare that this property group doesn't belong to a plugin and should use the core plugin GUID
        /// </summary>
        public bool IsCorePropertyGroup { get; internal set; }

        /// <summary>
        ///     Gets or sets whether the property is hidden in the UI
        /// </summary>
        public bool IsHidden { get; set; }

        /// <summary>
        ///     A list of all layer properties in this group
        /// </summary>
        public ReadOnlyCollection<BaseLayerProperty> LayerProperties => _layerProperties.AsReadOnly();

        /// <summary>
        ///     A list of al child groups in this group
        /// </summary>
        public ReadOnlyCollection<LayerPropertyGroup> LayerPropertyGroups => _layerPropertyGroups.AsReadOnly();

        /// <summary>
        ///     Recursively gets all layer properties on this group and any subgroups
        /// </summary>
        /// <returns></returns>
        public IReadOnlyCollection<BaseLayerProperty> GetAllLayerProperties()
        {
            if (!PropertiesInitialized)
                return new List<BaseLayerProperty>();
            if (_allLayerProperties != null)
                return _allLayerProperties;

            var result = new List<BaseLayerProperty>(LayerProperties);
            foreach (var layerPropertyGroup in LayerPropertyGroups)
                result.AddRange(layerPropertyGroup.GetAllLayerProperties());

            _allLayerProperties = result.AsReadOnly();
            return _allLayerProperties;
        }

        /// <summary>
        ///     Called before properties are fully initialized to allow you to populate
        ///     <see cref="LayerProperty{T}.DefaultValue" /> on the properties you want
        /// </summary>
        protected abstract void PopulateDefaults();

        /// <summary>
        ///     Called when all layer properties in this property group have been initialized, you may access all properties on the
        ///     group here
        /// </summary>
        protected abstract void OnPropertiesInitialized();

        protected virtual void OnPropertyGroupInitialized()
        {
            PropertyGroupInitialized?.Invoke(this, EventArgs.Empty);
        }

        internal void InitializeProperties(ILayerService layerService, Layer layer, [NotNull] string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));
            // Doubt this will happen but let's make sure
            if (PropertiesInitialized)
                throw new ArtemisCoreException("Layer property group already initialized, wut");

            Layer = layer;
            Path = path.TrimEnd('.');

            // Get all properties with a PropertyDescriptionAttribute
            foreach (var propertyInfo in GetType().GetProperties())
            {
                var propertyDescription = Attribute.GetCustomAttribute(propertyInfo, typeof(PropertyDescriptionAttribute));
                if (propertyDescription != null)
                {
                    if (!typeof(BaseLayerProperty).IsAssignableFrom(propertyInfo.PropertyType))
                        throw new ArtemisPluginException("Layer property with PropertyDescription attribute must be of type LayerProperty");

                    var instance = (BaseLayerProperty) Activator.CreateInstance(propertyInfo.PropertyType, true);
                    instance.Parent = this;
                    instance.Layer = layer;
                    InitializeProperty(layer, path + propertyInfo.Name, instance);
                    propertyInfo.SetValue(this, instance);
                    _layerProperties.Add(instance);
                }
                else
                {
                    var propertyGroupDescription = Attribute.GetCustomAttribute(propertyInfo, typeof(PropertyGroupDescriptionAttribute));
                    if (propertyGroupDescription != null)
                    {
                        if (!typeof(LayerPropertyGroup).IsAssignableFrom(propertyInfo.PropertyType))
                            throw new ArtemisPluginException("Layer property with PropertyGroupDescription attribute must be of type LayerPropertyGroup");

                        var instance = (LayerPropertyGroup) Activator.CreateInstance(propertyInfo.PropertyType);
                        instance.Parent = this;
                        instance.InitializeProperties(layerService, layer, $"{path}{propertyInfo.Name}.");
                        propertyInfo.SetValue(this, instance);
                        _layerPropertyGroups.Add(instance);
                    }
                }
            }

            PopulateDefaults();
            foreach (var layerProperty in _layerProperties.Where(p => !p.IsLoadedFromStorage))
                layerProperty.ApplyDefaultValue();

            OnPropertiesInitialized();
            PropertiesInitialized = true;
            OnPropertyGroupInitialized();
        }

        internal void ApplyToEntity()
        {
            // Get all properties with a PropertyDescriptionAttribute
            foreach (var propertyInfo in GetType().GetProperties())
            {
                var propertyDescription = Attribute.GetCustomAttribute(propertyInfo, typeof(PropertyDescriptionAttribute));
                if (propertyDescription != null)
                {
                    var layerProperty = (BaseLayerProperty) propertyInfo.GetValue(this);
                    layerProperty.ApplyToEntity();
                }
                else
                {
                    var propertyGroupDescription = Attribute.GetCustomAttribute(propertyInfo, typeof(PropertyGroupDescriptionAttribute));
                    if (propertyGroupDescription != null)
                    {
                        var layerPropertyGroup = (LayerPropertyGroup) propertyInfo.GetValue(this);
                        layerPropertyGroup.ApplyToEntity();
                    }
                }
            }
        }

        internal void Update(double deltaTime)
        {
            // Since at this point we don't know what properties the group has without using reflection,
            // let properties subscribe to the update event and update themselves
            OnPropertyGroupUpdating(new PropertyGroupUpdatingEventArgs(deltaTime));
        }

        internal void Override(TimeSpan overrideTime)
        {
            // Same as above, but now the progress is overridden
            OnPropertyGroupOverriding(new PropertyGroupUpdatingEventArgs(overrideTime));
        }

        private void InitializeProperty(Layer layer, string path, BaseLayerProperty instance)
        {
            var pluginGuid = IsCorePropertyGroup || instance.IsCoreProperty ? Constants.CorePluginInfo.Guid : layer.LayerBrush.PluginInfo.Guid;
            var entity = layer.LayerEntity.PropertyEntities.FirstOrDefault(p => p.PluginGuid == pluginGuid && p.Path == path);
            var fromStorage = true;
            if (entity == null)
            {
                fromStorage = false;
                entity = new PropertyEntity {PluginGuid = pluginGuid, Path = path};
                layer.LayerEntity.PropertyEntities.Add(entity);
            }

            instance.ApplyToLayerProperty(entity, this, fromStorage);
        }

        #region Events

        internal event EventHandler<PropertyGroupUpdatingEventArgs> PropertyGroupUpdating;
        internal event EventHandler<PropertyGroupUpdatingEventArgs> PropertyGroupOverriding;
        public event EventHandler PropertyGroupInitialized;

        internal virtual void OnPropertyGroupUpdating(PropertyGroupUpdatingEventArgs e)
        {
            PropertyGroupUpdating?.Invoke(this, e);
        }

        protected virtual void OnPropertyGroupOverriding(PropertyGroupUpdatingEventArgs e)
        {
            PropertyGroupOverriding?.Invoke(this, e);
        }

        #endregion
    }
}