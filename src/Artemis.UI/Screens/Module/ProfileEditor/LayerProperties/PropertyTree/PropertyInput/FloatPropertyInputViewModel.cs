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
            set => InputValue = value;
        }

        public override void Update()
        {
            NotifyOfPropertyChange(() => FloatInputValue);
        }
    }
}