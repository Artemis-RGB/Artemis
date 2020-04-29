using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Artemis.Core.Events;
using Artemis.Core.Exceptions;
using Artemis.Core.Models.Profile.LayerProperties;
using Artemis.Core.Models.Profile.LayerProperties.Attributes;
using Artemis.Core.Plugins.Exceptions;
using Artemis.Core.Services.Interfaces;

namespace Artemis.Core.Models.Profile
{
    public class LayerPropertyGroup
    {
        private ReadOnlyCollection<BaseLayerProperty> _allLayerProperties;

        protected LayerPropertyGroup()
        {
            LayerProperties = new List<BaseLayerProperty>();
            LayerPropertyGroups = new List<LayerPropertyGroup>();
        }

        public bool PropertiesInitialized { get; private set; }

        /// <summary>
        ///     Used to declare that this property group doesn't belong to a plugin and should use the core plugin GUID
        /// </summary>
        internal bool IsCorePropertyGroup { get; set; }

        /// <summary>
        ///     A list of all layer properties in this group
        /// </summary>
        internal List<BaseLayerProperty> LayerProperties { get; set; }

        /// <summary>
        ///     A list of al child groups in this group
        /// </summary>
        internal List<LayerPropertyGroup> LayerPropertyGroups { get; set; }

        /// <summary>
        ///     Called when all layer properties in this property group have been initialized
        /// </summary>
        protected virtual void OnPropertiesInitialized()
        {
        }

        internal void InitializeProperties(ILayerService layerService, Layer layer, string path)
        {
            // Doubt this will happen but let's make sure
            if (PropertiesInitialized)
                throw new ArtemisCoreException("Layer property group already initialized, wut");

            // Get all properties with a PropertyDescriptionAttribute
            foreach (var propertyInfo in GetType().GetProperties())
            {
                var propertyDescription = Attribute.GetCustomAttribute(propertyInfo, typeof(PropertyDescriptionAttribute));
                if (propertyDescription != null)
                {
                    if (!typeof(LayerProperty<>).IsAssignableFrom(propertyInfo.PropertyType))
                        throw new ArtemisPluginException("Layer property with PropertyDescription attribute must be of type LayerProperty");

                    var instance = (BaseLayerProperty) Activator.CreateInstance(propertyInfo.PropertyType);
                    InitializeProperty(layer, path, instance);
                    propertyInfo.SetValue(this, instance);
                    LayerProperties.Add(instance);
                }
                else
                {
                    var propertyGroupDescription = Attribute.GetCustomAttribute(propertyInfo, typeof(PropertyGroupDescriptionAttribute));
                    if (propertyGroupDescription != null)
                    {
                        if (!typeof(LayerPropertyGroup).IsAssignableFrom(propertyInfo.PropertyType))
                            throw new ArtemisPluginException("Layer property with PropertyGroupDescription attribute must be of type LayerPropertyGroup");

                        var instance = (LayerPropertyGroup) Activator.CreateInstance(propertyInfo.PropertyType);
                        instance.InitializeProperties(layerService, layer, $"{path}{propertyInfo.Name}.");
                        propertyInfo.SetValue(this, instance);
                        LayerPropertyGroups.Add(instance);
                    }
                }
            }

            OnPropertiesInitialized();
            PropertiesInitialized = true;
        }

        internal void ApplyToEntity()
        {
            // Get all properties with a PropertyDescriptionAttribute
            foreach (var propertyInfo in GetType().GetProperties())
            {
                var propertyDescription = Attribute.GetCustomAttribute(propertyInfo, typeof(PropertyDescriptionAttribute));
                if (propertyDescription != null)
                {
                }
                else
                {
                    var propertyGroupDescription = Attribute.GetCustomAttribute(propertyInfo, typeof(PropertyGroupDescriptionAttribute));
                    if (propertyGroupDescription != null)
                    {
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

        /// <summary>
        ///     Recursively gets all layer properties on this group and any subgroups
        /// </summary>
        /// <returns></returns>
        internal IReadOnlyCollection<BaseLayerProperty> GetAllLayerProperties()
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

        private void InitializeProperty(Layer layer, string path, BaseLayerProperty instance)
        {
            var pluginGuid = IsCorePropertyGroup || instance.IsCoreProperty ? Constants.CorePluginInfo.Guid : layer.LayerBrush.PluginInfo.Guid;
            var entity = layer.LayerEntity.PropertyEntities.FirstOrDefault(p => p.PluginGuid == pluginGuid && p.Path == path);
            if (entity != null)
                instance.ApplyToLayerProperty(entity, this);
        }

        #region Events

        internal event EventHandler<PropertyGroupUpdatingEventArgs> PropertyGroupUpdating;
        internal event EventHandler<PropertyGroupUpdatingEventArgs> PropertyGroupOverriding;

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