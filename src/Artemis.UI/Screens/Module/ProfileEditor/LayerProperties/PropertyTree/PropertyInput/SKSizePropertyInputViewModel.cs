using System;
using System.Collections.Generic;
using Artemis.Core.Models.Profile.LayerProperties;
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
            set => InputValue = new SKSize(value, Height);
        }

        [DependsOn(nameof(InputValue))]
        public float Height
        {
            get => ((SKSize?)InputValue)?.Height ?? 0;
            set => InputValue = new SKSize(Width, value);
        }

        public override void Update()
        {
            NotifyOfPropertyChange(() => Width);
            NotifyOfPropertyChange(() => Height);
        }
    }
}