using System;
using System.Collections.Generic;
using Artemis.Core.Models.Profile.LayerProperties;
using Artemis.UI.Services.Interfaces;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.PropertyTree.PropertyInput
{
    public class FloatPropertyInputViewModel : PropertyInputViewModel
    {
        public FloatPropertyInputViewModel(IProfileEditorService profileEditorService) : base(profileEditorService)
        {
        }

        public sealed override List<Type> CompatibleTypes { get; } = new List<Type> {typeof(float)};

        public float FloatInputValue
        {
            get => (float) InputValue;
            set => InputValue = value;
        }

        public override void Update()
        {
            NotifyOfPropertyChange(() => FloatInputValue);
        }

        protected override void UpdateBaseValue(object value)
        {
            var layerProperty = (LayerProperty<float>) LayerPropertyViewModel.LayerProperty;
            layerProperty.Value = (float) value;
        }

        protected override void UpdateKeyframeValue(BaseKeyframe baseKeyframe, object value)
        {
            var keyframe = (Keyframe<float>) baseKeyframe;
            keyframe.Value = (float) value;
        }
    }
}