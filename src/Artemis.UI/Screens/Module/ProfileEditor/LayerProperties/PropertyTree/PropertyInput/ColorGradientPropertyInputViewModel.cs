using System;
using System.Collections.Generic;
using Artemis.Core.Models.Profile;
using Artemis.UI.Services.Interfaces;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.PropertyTree.PropertyInput
{
    public class ColorGradientPropertyInputViewModel : PropertyInputViewModel
    {
        public ColorGradientPropertyInputViewModel(IProfileEditorService profileEditorService) : base(profileEditorService)
        {
        }

        public sealed override List<Type> CompatibleTypes { get; } = new List<Type> {typeof(ColorGradient)};

        public ColorGradient ColorGradientInputValue
        {
            get => (ColorGradient) InputValue ?? new ColorGradient();
            set => InputValue = value;
        }

        public override void Update()
        {
            NotifyOfPropertyChange(() => ColorGradientInputValue);
        }
    }
}