using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core.Models.Profile.LayerProperties;
using Artemis.Core.Models.Profile.LayerProperties.Attributes;
using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Abstract;
using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Timeline;
using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Tree;
using Artemis.UI.Services.Interfaces;
using Humanizer;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties
{
    public class LayerPropertyViewModel<T> : LayerPropertyViewModel
    {
        public LayerPropertyViewModel(IProfileEditorService profileEditorService, LayerProperty<T> layerProperty, PropertyDescriptionAttribute propertyDescription)
            : base(profileEditorService, layerProperty)
        {
            LayerProperty = layerProperty;
            PropertyDescription = propertyDescription;

            TreePropertyViewModel = ProfileEditorService.CreateTreePropertyViewModel(this);
            TimelinePropertyViewModel = new TimelinePropertyViewModel<T>(this, profileEditorService);

            TreePropertyBaseViewModel = TreePropertyViewModel;
            TimelinePropertyBaseViewModel = TimelinePropertyViewModel;

            // Generate a fallback name if the description does not contain one
            if (PropertyDescription.Name == null)
            {
                var propertyInfo = LayerProperty.Parent?.GetType().GetProperties().FirstOrDefault(p => ReferenceEquals(p.GetValue(LayerProperty.Parent), LayerProperty));
                if (propertyInfo != null)
                    PropertyDescription.Name = propertyInfo.Name.Humanize();
                else
                    PropertyDescription.Name = $"Unknown {typeof(T).Name} property";
            }

            LayerProperty.VisibilityChanged += LayerPropertyOnVisibilityChanged;
        }

        public override bool IsVisible => !LayerProperty.IsHidden;

        public LayerProperty<T> LayerProperty { get; }

        public TreePropertyViewModel<T> TreePropertyViewModel { get; set; }
        public TimelinePropertyViewModel<T> TimelinePropertyViewModel { get; set; }

        public override List<BaseLayerPropertyKeyframe> GetKeyframes(bool visibleOnly)
        {
            return LayerProperty.BaseKeyframes.ToList();
        }

        public override void Dispose()
        {
            TreePropertyViewModel.Dispose();
            TimelinePropertyViewModel.Dispose();

            LayerProperty.VisibilityChanged -= LayerPropertyOnVisibilityChanged;
        }

        public void SetCurrentValue(T value, bool saveChanges)
        {
            LayerProperty.SetCurrentValue(value, ProfileEditorService.CurrentTime);
            if (saveChanges)
                ProfileEditorService.UpdateSelectedProfileElement();
            else
                ProfileEditorService.UpdateProfilePreview();
        }

        private void LayerPropertyOnVisibilityChanged(object? sender, EventArgs e)
        {
           NotifyOfPropertyChange(nameof(IsVisible));
        }
    }

    public abstract class LayerPropertyViewModel : LayerPropertyBaseViewModel
    {
        protected LayerPropertyViewModel(IProfileEditorService profileEditorService, BaseLayerProperty baseLayerProperty)
        {
            ProfileEditorService = profileEditorService;
            BaseLayerProperty = baseLayerProperty;
        }

        public IProfileEditorService ProfileEditorService { get; }
        public BaseLayerProperty BaseLayerProperty { get; }

        public PropertyDescriptionAttribute PropertyDescription { get; protected set; }
        public TreePropertyViewModel TreePropertyBaseViewModel { get; set; }
        public TimelinePropertyViewModel TimelinePropertyBaseViewModel { get; set; }
    }
}