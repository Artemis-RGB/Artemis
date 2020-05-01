using System;
using System.Collections.Generic;
using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Profile.LayerProperties;
using Artemis.Core.Models.Profile.LayerProperties.Attributes;
using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Abstract;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties
{
    public class LayerPropertyGroupViewModel : LayerPropertyBaseViewModel
    {
        public LayerPropertyGroupViewModel(LayerPropertyGroup layerPropertyGroup, PropertyGroupDescriptionAttribute propertyGroupDescription)
        {
            LayerPropertyGroup = layerPropertyGroup;
            PropertyGroupDescription = propertyGroupDescription;

            IsExpanded = PropertyGroupDescription.ExpandByDefault;

            Children = new List<LayerPropertyBaseViewModel>();
            PopulateChildren();
        }

        public LayerPropertyGroup LayerPropertyGroup { get; }
        public PropertyGroupDescriptionAttribute PropertyGroupDescription { get; }
        public bool IsExpanded { get; set; }

        public List<LayerPropertyBaseViewModel> Children { get; set; }

        private void PopulateChildren()
        {
            // Get all properties and property groups and create VMs for them
            foreach (var propertyInfo in LayerPropertyGroup.GetType().GetProperties())
            {
                var propertyAttribute = (PropertyDescriptionAttribute) Attribute.GetCustomAttribute(propertyInfo, typeof(PropertyDescriptionAttribute));
                var groupAttribute = (PropertyGroupDescriptionAttribute) Attribute.GetCustomAttribute(propertyInfo, typeof(PropertyGroupDescriptionAttribute));
                var value = propertyInfo.GetValue(LayerPropertyGroup);

                // Create VMs for properties on the group
                if (propertyAttribute != null && value is BaseLayerProperty)
                {
                    // Go through the pain of instantiating a generic type VM now via reflection to make things a lot simpler down the line
                    var genericType = propertyInfo.PropertyType.GetGenericArguments()[0];
                    var genericViewModel = typeof(LayerPropertyViewModel<>).MakeGenericType(genericType);
                    var instance = Activator.CreateInstance(genericViewModel, value, propertyAttribute);
                    Children.Add((LayerPropertyBaseViewModel) instance);
                }
                // Create VMs for child groups on this group, resulting in a nested structure
                else if (groupAttribute != null && value is LayerPropertyGroup layerPropertyGroup)
                {
                    Children.Add(new LayerPropertyGroupViewModel(layerPropertyGroup, groupAttribute));
                }
            }
        }

        public override List<BaseLayerPropertyKeyframe> GetKeyframes(bool visibleOnly)
        {
            var result = new List<BaseLayerPropertyKeyframe>();
            if (!IsExpanded)
                return result;

            foreach (var layerPropertyBaseViewModel in Children)
                result.AddRange(layerPropertyBaseViewModel.GetKeyframes(visibleOnly));

            return result;
        }
    }
}