using Artemis.Core;

namespace Artemis.UI.Screens.ProfileEditor.LayerProperties.DataBindings.DirectDataBinding.ModifierTypes
{
    public class ModifierTypeViewModel : IModifierTypeViewModel
    {
        public ModifierTypeViewModel(BaseDataBindingModifierType modifierType)
        {
            ModifierType = modifierType;
        }

        public BaseDataBindingModifierType ModifierType { get; set; }
    }
}