using Artemis.Core;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.LayerProperties.DataBindings
{
    public class DataBindingModifierViewModel : PropertyChangedBase
    {
        public DataBindingModifierViewModel(DataBindingModifier modifier)
        {
            Modifier = modifier;
        }

        public DataBindingModifier Modifier { get; }
    }
}