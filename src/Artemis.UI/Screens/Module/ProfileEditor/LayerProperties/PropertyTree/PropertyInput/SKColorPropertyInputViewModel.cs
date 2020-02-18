using System;
using System.Collections.Generic;
using Artemis.UI.Services.Interfaces;
using SkiaSharp;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.PropertyTree.PropertyInput
{
    public class SKColorPropertyInputViewModel : PropertyInputViewModel
    {
        public SKColorPropertyInputViewModel(IProfileEditorService profileEditorService) : base(profileEditorService)
        {
        }

        public sealed override List<Type> CompatibleTypes { get; } = new List<Type> {typeof(SKColor)};

        public SKColor SKColorInputValue
        {
            get => (SKColor?) InputValue ?? new SKColor();
            set => InputValue = value;
        }

        public override void Update()
        {
            NotifyOfPropertyChange(() => SKColorInputValue);
        }

        public override void ApplyInputDrag(object startValue, double dragDistance)
        {
            throw new NotImplementedException();
        }
    }
}