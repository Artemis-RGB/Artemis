using System.Collections.Generic;
using System.Linq;
using Artemis.Core.Models.Profile.LayerProperties;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.PropertyTree.PropertyInput;
using Artemis.UI.Services.Interfaces;
using Ninject;
using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties
{
    public class LayerPropertyViewModel : PropertyChangedBase
    {
        private readonly IKernel _kernel;
        private readonly IProfileEditorService _profileEditorService;
        private bool _keyframesEnabled;

        public LayerPropertyViewModel(BaseLayerProperty layerProperty, LayerPropertyViewModel parent, ILayerPropertyViewModelFactory layerPropertyViewModelFactory, IKernel kernel, IProfileEditorService profileEditorService)
        {
            _kernel = kernel;
            _profileEditorService = profileEditorService;
            _keyframesEnabled = layerProperty.UntypedKeyframes.Any();

            LayerProperty = layerProperty;
            Parent = parent;
            Children = new List<LayerPropertyViewModel>();

            foreach (var child in layerProperty.Children)
                Children.Add(layerPropertyViewModelFactory.Create(child, this));
        }

        public BaseLayerProperty LayerProperty { get; }

        public LayerPropertyViewModel Parent { get; }
        public List<LayerPropertyViewModel> Children { get; set; }

        public bool IsExpanded { get; set; }

        public bool KeyframesEnabled
        {
            get => _keyframesEnabled;
            set
            {
                _keyframesEnabled = value;
                UpdateKeyframes();
            }
        }

        private void UpdateKeyframes()
        {
            // Either create a new first keyframe or clear all the keyframes
            if (_keyframesEnabled)
                LayerProperty.CreateNewKeyframe(_profileEditorService.CurrentTime);
            else
                LayerProperty.ClearKeyframes();

            // Force the keyframe engine to update, the new keyframe is the current keyframe
            LayerProperty.KeyframeEngine.Update(0);

            _profileEditorService.UpdateSelectedProfileElement();
        }

        public PropertyInputViewModel GetPropertyInputViewModel()
        {
            var match = _kernel.Get<List<PropertyInputViewModel>>().FirstOrDefault(p => p.CompatibleTypes.Contains(LayerProperty.Type));
            if (match == null)
                return null;

            match.Initialize(this);
            return match;
        }
    }
}