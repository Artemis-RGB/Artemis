using Artemis.Core;

namespace Artemis.UI.Screens.Profile.ProfileEditor.ProfileTree.Dialogs.AdaptionHints;

public class CategoryAdaptionHintViewModel : AdaptionHintViewModelBase
{
    public CategoryAdaptionHintViewModel(Layer layer, CategoryAdaptionHint adaptionHint) : base(layer, adaptionHint)
    {
        CategoryAdaptionHint = adaptionHint;
    }

    public CategoryAdaptionHint CategoryAdaptionHint { get; }
}