using System.Collections.Generic;
using System.Linq;
using Artemis.Core.Models.Profile.LayerProperties;
using Artemis.Core.Models.Profile.LayerProperties.Attributes;
using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Abstract;
using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Timeline;
using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Tree;
using Artemis.UI.Services.Interfaces;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties
{
    public class LayerPropertyViewModel<T> : LayerPropertyViewModel
    {
        public LayerPropertyViewModel(IProfileEditorService profileEditorService, LayerProperty<T> layerProperty, PropertyDescriptionAttribute propertyDescription)
            : base(profileEditorService, layerProperty)
        {
            LayerProperty = layerProperty;
            PropertyDescription = propertyDescription;

            TreePropertyViewModel = new TreePropertyViewModel<T>(this);
            TimelinePropertyViewModel = new TimelinePropertyViewModel<T>(this);

            TreePropertyBaseViewModel = TreePropertyViewModel;
            TimelinePropertyBaseViewModel = TimelinePropertyViewModel;
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
        }

        public void SetCurrentValue(T value, bool saveChanges)
        {
            LayerProperty.SetCurrentValue(value, ProfileEditorService.CurrentTime);
            if (saveChanges)
                ProfileEditorService.UpdateSelectedProfileElement();
            else
                ProfileEditorService.UpdateProfilePreview();
        }
    }

    public abstract class LayerPropertyViewModel : LayerPropertyBaseViewModel
    {
        public IProfileEditorService ProfileEditorService { get; }
        public BaseLayerProperty BaseLayerProperty { get; }

        protected LayerPropertyViewModel(IProfileEditorService profileEditorService, BaseLayerProperty baseLayerProperty)
        {
            ProfileEditorService = profileEditorService;
            BaseLayerProperty = baseLayerProperty;
        }

        public PropertyDescriptionAttribute PropertyDescription { get; protected set; }
        public TreePropertyViewModel TreePropertyBaseViewModel { get; set; }
        public TimelinePropertyViewModel TimelinePropertyBaseViewModel { get; set; }
    }
}