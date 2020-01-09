using System;
using System.Collections.Generic;
using Artemis.Core.Models.Profile.LayerProperties;
using Artemis.UI.Services.Interfaces;
using PropertyChanged;
using SkiaSharp;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.PropertyTree.PropertyInput
{
    public class SKPointPropertyInputViewModel : PropertyInputViewModel
    {
        public SKPointPropertyInputViewModel(IProfileEditorService profileEditorService) : base(profileEditorService)
        {
        }

        public sealed override List<Type> CompatibleTypes { get; } = new List<Type> {typeof(SKPoint)};

        // Since SKPoint is immutable we need to create properties that replace the SKPoint entirely
        [DependsOn(nameof(InputValue))]
        public float X
        {
            get => ((SKPoint) InputValue).X;
            set => InputValue = new SKPoint(value, Y);
        }

        [DependsOn(nameof(InputValue))]
        public float Y
        {
            get => ((SKPoint) InputValue).Y;
            set => InputValue = new SKPoint(X, value);
        }

        public override void Update()
        {
            NotifyOfPropertyChange(() => X);
            NotifyOfPropertyChange(() => Y);
        }

        protected override void UpdateBaseValue(object value)
        {
            var layerProperty = (LayerProperty<SKPoint>) LayerPropertyViewModel.LayerProperty;
            layerProperty.Value = (SKPoint) value;
        }

        protected override void UpdateKeyframeValue(BaseKeyframe baseKeyframe, object value)
        {
            var keyframe = (Keyframe<SKPoint>) baseKeyframe;
            keyframe.Value = (SKPoint) value;
        }
    }
}