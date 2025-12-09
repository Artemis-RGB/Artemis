using Artemis.Core;

namespace Artemis.UI.Screens.Profile.ProfileEditor.ProfileTree.Dialogs.AdaptionHints;

public class KeyboardSectionAdaptionHintViewModel : AdaptionHintViewModelBase
{
    public KeyboardSectionAdaptionHintViewModel(Layer layer, KeyboardSectionAdaptionHint adaptionHint) : base(layer, adaptionHint)
    {
        KeyboardSectionAdaptionHint = adaptionHint;
    }

    public KeyboardSectionAdaptionHint KeyboardSectionAdaptionHint { get; }
}