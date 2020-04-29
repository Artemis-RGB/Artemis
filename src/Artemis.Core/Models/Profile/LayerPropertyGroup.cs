using System;
using System.Linq;
using Artemis.Core.Events;
using Artemis.Core.Models.Profile.LayerProperties;
using Artemis.Core.Models.Profile.LayerProperties.Attributes;
using Artemis.Core.Plugins.Exceptions;
using Artemis.Core.Services.Interfaces;

namespace Artemis.Core.Models.Profile
{
    public class LayerPropertyGroup
    {
        public bool PropertiesInitialized { get; private set; }

        /// <summary>
        /// Used to declare that this property group doesn't belong to a plugin and should use the core plugin GUID
        /// </summary>
        internal bool IsCorePropertyGroup { get; set; }

        /// <summary>
        ///     Called when all layer properties in this property group have been initialized
        /// </summary>
        protected virtual void OnPropertiesInitialized()
        {
        }

        internal void InitializeProperties(ILayerService layerService, Layer layer, string path)
        {
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