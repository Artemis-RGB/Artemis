using System;
using System.Linq;
using Artemis.Core.Models.Profile.LayerProperties;
using Artemis.Core.Utilities;
using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Abstract;
using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Tree.PropertyInput.Abstract;
using Artemis.UI.Services.Interfaces;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Tree
{
    public class TreePropertyViewModel<T> : TreePropertyViewModel
    {
        private readonly IProfileEditorService _profileEditorService;

        public TreePropertyViewModel(LayerPropertyBaseViewModel layerPropertyBaseViewModel, PropertyInputViewModel<T> propertyInputViewModel,
            IProfileEditorService profileEditorService) : base(layerPropertyBaseViewModel)
        {
            _profileEditorService = profileEditorService;
            LayerPropertyViewModel = (LayerPropertyViewModel<T>) layerPropertyBaseViewModel;
            PropertyInputViewModel = propertyInputViewModel;
        }

        public LayerPropertyViewModel<T> LayerPropertyViewModel { get; }
        public PropertyInputViewModel<T> PropertyInputViewModel { get; set; }

        public bool KeyframesEnabled
        {
            get => LayerPropertyViewModel.LayerProperty.KeyframesEnabled;
            set => ApplyKeyframesEnabled(value);
        }

        public override void Dispose()
        {
            PropertyInputViewModel.Dispose();
        }

        private void ApplyKeyframesEnabled(bool enable)
        {
            // If enabling keyframes for the first time, add a keyframe with the current value at the current position
            if (enable && !LayerPropertyViewModel.LayerProperty.Keyframes.Any())
            {
                LayerPropertyViewModel.LayerProperty.AddKeyframe(new LayerPropertyKeyframe<T>(
                    LayerPropertyViewModel.LayerProperty.CurrentValue,
                    _profileEditorService.CurrentTime,
                    Easings.Functions.Linear,
                    LayerPropertyViewModel.LayerProperty
                ));
            }
            // If disabling keyframes, set the base value to the current value
            else if (!enable && LayerPropertyViewModel.LayerProperty.Keyframes.Any()) 
                LayerPropertyViewModel.LayerProperty.BaseValue = LayerPropertyViewModel.LayerProperty.CurrentValue;

            LayerPropertyViewModel.LayerProperty.KeyframesEnabled = enable;

            _profileEditorService.UpdateSelectedProfileElement();
        }
    }

    public abstract class TreePropertyViewModel : IDisposable
    {
        protected TreePropertyViewModel(LayerPropertyBaseViewModel layerPropertyBaseViewModel)
        {
            LayerPropertyBaseViewModel = layerPropertyBaseViewModel;
        }

        public LayerPropertyBaseViewModel LayerPropertyBaseViewModel { get; }
        public abstract void Dispose();
    }
}