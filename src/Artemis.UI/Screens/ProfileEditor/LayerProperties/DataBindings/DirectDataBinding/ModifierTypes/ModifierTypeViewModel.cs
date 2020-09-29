using Artemis.Core;

namespace Artemis.UI.Screens.ProfileEditor.LayerProperties.DataBindings.DirectDataBinding.ModifierTypes
{
    public class ModifierTypeViewModel : IModifierTypeViewModel
    {
        public ModifierTypeViewModel(DataBindingModifierType modifierType)
        {
            ModifierType = modifierType;
        }

        public DataBindingModifierType ModifierType { get; set; }
    }
}