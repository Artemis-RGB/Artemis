using System;
using System.Collections.Generic;
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
            get => (float?) InputValue ?? 0f;
            set => InputValue = ApplyInputValue(value);
        }

        public override void Update()
        {
            NotifyOfPropertyChange(() => FloatInputValue);
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