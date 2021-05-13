using Artemis.Core;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.ProfileTree.Dialogs.AdaptionHints
{
    public abstract class AdaptionHintViewModel : Screen
    {
        protected AdaptionHintViewModel(IAdaptionHint adaptionHint)
        {
            AdaptionHint = adaptionHint;
        }

        public IAdaptionHint AdaptionHint { get; }

        public void Remove()
        {
            ((LayerHintsDialogViewModel) Parent).RemoveAdaptionHint(this);
        }
    }
}