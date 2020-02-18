using System;
using System.Collections.Generic;
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
            get => ((SKPoint?) InputValue)?.X ?? 0;
            set => InputValue = new SKPoint(ApplyInputValue(value), Y);
        }

        [DependsOn(nameof(InputValue))]
        public float Y
        {
            get => ((SKPoint?) InputValue)?.Y ?? 0;
            set => InputValue = new SKPoint(X, ApplyInputValue(value));
        }

        public override void Update()
        {
            NotifyOfPropertyChange(() => X);
            NotifyOfPropertyChange(() => Y);
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