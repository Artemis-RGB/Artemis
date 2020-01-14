using System;
using System.Collections.Generic;
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
            get => (int?) InputValue ?? 0;
            set => InputValue = value;
        }

        public override void Update()
        {
            NotifyOfPropertyChange(() => IntInputValue);
        }
    }
}