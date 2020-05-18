using System;
using System.Collections.Generic;
using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Profile.LayerProperties;
using Artemis.Core.Models.Profile.LayerProperties.Attributes;
using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Abstract;
using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Timeline;
using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Tree;
using Artemis.UI.Services.Interfaces;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties
{
    public class LayerPropertyGroupViewModel : LayerPropertyBaseViewModel
    {
        public LayerPropertyGroupViewModel(IProfileEditorService profileEditorService, LayerPropertyGroup layerPropertyGroup, PropertyGroupDescriptionAttribute propertyGroupDescription)
        {
            ProfileEditorService = profileEditorService;

            LayerPropertyGroup = layerPropertyGroup;
            PropertyGroupDescription = propertyGroupDescription;
            IsExpanded = PropertyGroupDescription.ExpandByDefault;

            TreePropertyGroupViewModel = new TreePropertyGroupViewModel(this);
            TimelinePropertyGroupViewModel = new TimelinePropertyGroupViewModel(this);

            PopulateChildren();
        }

        public override bool IsVisible => !LayerPropertyGroup.IsHidden;

        public IProfileEditorService ProfileEditorService { get; }

        public LayerPropertyGroup LayerPropertyGroup { get; }
        public PropertyGroupDescriptionAttribute PropertyGroupDescription { get; }

        public TreePropertyGroupViewModel TreePropertyGroupViewModel { get; set; }
        public TimelinePropertyGroupViewModel TimelinePropertyGroupViewModel { get; set; }

        private void PopulateChildren()
        {
            // Get all properties and property groups and create VMs for them
            foreach (var propertyInfo in LayerPropertyGroup.GetType().GetProperties())
            {
                var propertyAttribute = (PropertyDescriptionAttribute) Attribute.GetCustomAttribute(propertyInfo, typeof(PropertyDescriptionAttribute));
                var groupAttribute = (PropertyGroupDescriptionAttribute) Attribute.GetCustomAttribute(propertyInfo, typeof(PropertyGroupDescriptionAttribute));
                var value = propertyInfo.GetValue(LayerPropertyGroup);

                // Create VMs for properties on the group
                if (propertyAttribute != null && value is BaseLayerProperty baseLayerProperty)
                {
                    var viewModel = ProfileEditorService.CreateLayerPropertyViewModel(baseLayerProperty, propertyAttribute);
                    if (viewModel != null)
                        Children.Add(viewModel);
                }
                // Create VMs for child groups on this group, resulting in a nested structure
                else if (groupAttribute != null && value is LayerPropertyGroup layerPropertyGroup)
                {
                    Children.Add(new LayerPropertyGroupViewModel(ProfileEditorService, layerPropertyGroup, groupAttribute));
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

        public override void Dispose()
        {
            foreach (var layerPropertyBaseViewModel in Children)
                layerPropertyBaseViewModel.Dispose();
        }

        public List<LayerPropertyBaseViewModel> GetAllChildren()
        {
            var result = new List<LayerPropertyBaseViewModel>();
            foreach (var layerPropertyBaseViewModel in Children)
            {
                result.Add(layerPropertyBaseViewModel);
                if (layerPropertyBaseViewModel is LayerPropertyGroupViewModel layerPropertyGroupViewModel)
                    result.AddRange(layerPropertyGroupViewModel.GetAllChildren());
            }

            return result;
        }
    }
}