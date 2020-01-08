using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core.Models.Profile.LayerProperties;
using Artemis.UI.Exceptions;
using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.PropertyTree.PropertyInput
{
    public abstract class PropertyInputViewModel : PropertyChangedBase
    {
        public bool Initialized { get; private set; }

        public abstract List<Type> CompatibleTypes { get; }
        public LayerPropertyViewModel LayerPropertyViewModel { get; private set; }

        public object InputValue
        {
            get => LayerPropertyViewModel.LayerProperty.KeyframeEngine.GetCurrentValue();
            set => UpdateInputValue(value);
        }

        public void Initialize(LayerPropertyViewModel layerPropertyViewModel)
        {
            if (Initialized)
                throw new ArtemisUIException("Cannot initialize the same property input VM twice");
            if (!CompatibleTypes.Contains(layerPropertyViewModel.LayerProperty.Type))
                throw new ArtemisUIException($"This input VM does not support the provided type {layerPropertyViewModel.LayerProperty.Type.Name}");

            LayerPropertyViewModel = layerPropertyViewModel;
            Update();

            Initialized = true;
        }

        public void Update()
        {
            NotifyOfPropertyChange(() => InputValue);
        }

        private void UpdateInputValue(object value)
        {
            // If keyframes are disabled, update the base value
            if (!LayerPropertyViewModel.KeyframesEnabled)
            {
                UpdateBaseValue(value);
                return;
            }

            // If on a keyframe, update the keyframe TODO: Make decisions..
            // var currentKeyframe = LayerPropertyViewModel.LayerProperty.UntypedKeyframes.FirstOrDefault(k => k.Position == LayerPropertyViewModel.)
            // Otherwise, add a new keyframe at the current position
        }

        protected abstract void UpdateBaseValue(object value);
        protected abstract void UpdateKeyframeValue(BaseKeyframe keyframe, object value);
        protected abstract void CreateKeyframeForValue(object value);
    }
}