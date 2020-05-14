using System;
using System.Collections.Generic;
using Artemis.UI.Services.Interfaces;
using PropertyChanged;
using SkiaSharp;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.PropertyTree.PropertyInput
{
    public class SKSizePropertyInputViewModel : PropertyInputViewModel
    {
        public SKSizePropertyInputViewModel(IProfileEditorService profileEditorService) : base(profileEditorService)
        {
        }

        public sealed override List<Type> CompatibleTypes { get; } = new List<Type> {typeof(SKSize)};

        // Since SKSize is immutable we need to create properties that replace the SKPoint entirely
        [DependsOn(nameof(InputValue))]
        public float Width
        {
            get => ((SKSize?) InputValue)?.Width ?? 0;
            set => InputValue = new SKSize(ApplyInputValue(value), Height);
        }

        [DependsOn(nameof(InputValue))]
        public float Height
        {
            get => ((SKSize?) InputValue)?.Height ?? 0;
            set => InputValue = new SKSize(Width, ApplyInputValue(value));
        }

        public override void Update()
        {
            NotifyOfPropertyChange(() => Width);
            NotifyOfPropertyChange(() => Height);
        }

        private float ApplyInputValue(float value)
        {
            if (LayerPropertyViewModel.LayerProperty.MaxInputValue != null &&
                LayerPropertyViewModel.LayerProperty.MaxInputValue is float maxFloat)
                value = Math.Min(value, maxFloat);
            if (LayerPropertyViewModel.LayerProperty.MinInputValue != null &&
                LayerPropertyViewModel.LayerProperty.MinInputValue is float minFloat)
                value = Math.Max(value, minFloat);

            return value;
        }
    }
}