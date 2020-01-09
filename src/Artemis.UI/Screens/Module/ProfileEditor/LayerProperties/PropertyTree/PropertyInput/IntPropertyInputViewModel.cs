using System;
using System.Collections.Generic;
using Artemis.Core.Models.Profile.LayerProperties;
using Artemis.UI.Services.Interfaces;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.PropertyTree.PropertyInput
{
    public class IntPropertyInputViewModel : PropertyInputViewModel
    {
        public IntPropertyInputViewModel(IProfileEditorService profileEditorService) : base(profileEditorService)
        {
        }

        public sealed override List<Type> CompatibleTypes { get; } = new List<Type> {typeof(int)};

        public int IntInputValue
        {
            get => (int) InputValue;
            set => InputValue = value;
        }

        public override void Update()
        {
            NotifyOfPropertyChange(() => IntInputValue);
        }

        protected override void UpdateBaseValue(object value)
        {
            var layerProperty = (LayerProperty<int>) LayerPropertyViewModel.LayerProperty;
            layerProperty.Value = (int) value;
        }

        protected override void UpdateKeyframeValue(BaseKeyframe baseKeyframe, object value)
        {
            var keyframe = (Keyframe<int>) baseKeyframe;
            keyframe.Value = (int) value;
        }
    }
}