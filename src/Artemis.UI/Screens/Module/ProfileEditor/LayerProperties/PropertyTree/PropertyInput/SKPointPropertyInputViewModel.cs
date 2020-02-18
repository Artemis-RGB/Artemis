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
            set => InputValue = new SKPoint(value, Y);
        }

        [DependsOn(nameof(InputValue))]
        public float Y
        {
            get => ((SKPoint?) InputValue)?.Y ?? 0;
            set => InputValue = new SKPoint(X, value);
        }

        public override void Update()
        {
            NotifyOfPropertyChange(() => X);
            NotifyOfPropertyChange(() => Y);
        }

        public override void ApplyInputDrag(object startValue, double dragDistance)
        {
            throw new NotImplementedException();
        }
    }
}