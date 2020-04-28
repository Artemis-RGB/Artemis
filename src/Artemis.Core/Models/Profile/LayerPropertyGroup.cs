using System;
using System.Linq;
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
                    if (!typeof(GenericLayerProperty<>).IsAssignableFrom(propertyInfo.PropertyType))
                        throw new ArtemisPluginException("Layer property with PropertyDescription attribute must be of type LayerProperty");

                    var instance = (LayerProperty) Activator.CreateInstance(propertyInfo.PropertyType);
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

        private void InitializeProperty(Layer layer, string path, LayerProperty instance)
        {
            var entity = layer.LayerEntity.PropertyEntities.FirstOrDefault(p => p.Id == path);
            if (entity != null)
                instance.LoadFromEntity(entity);
        }
    }
}