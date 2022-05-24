using Artemis.Core;

namespace Artemis.UI.Screens.ProfileEditor.ProfileTree.Dialogs.AdaptionHints;

public class CategoryAdaptionHintViewModel : AdaptionHintViewModelBase
{
    public CategoryAdaptionHintViewModel(CategoryAdaptionHint adaptionHint) : base(adaptionHint)
    {
        CategoryAdaptionHint = adaptionHint;
    }

    public CategoryAdaptionHint CategoryAdaptionHint { get; }
}