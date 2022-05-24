using Artemis.Core;

namespace Artemis.UI.Screens.ProfileEditor.ProfileTree.Dialogs.AdaptionHints;

public class KeyboardSectionAdaptionHintViewModel : AdaptionHintViewModelBase
{
    public KeyboardSectionAdaptionHintViewModel(KeyboardSectionAdaptionHint adaptionHint) : base(adaptionHint)
    {
        KeyboardSectionAdaptionHint = adaptionHint;
    }

    public KeyboardSectionAdaptionHint KeyboardSectionAdaptionHint { get; }
}