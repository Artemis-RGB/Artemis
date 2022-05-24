using System.Reactive;
using Artemis.Core;
using Artemis.UI.Shared;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.ProfileTree.Dialogs.AdaptionHints;

public abstract class AdaptionHintViewModelBase : ViewModelBase
{
    protected AdaptionHintViewModelBase(IAdaptionHint adaptionHint)
    {
        AdaptionHint = adaptionHint;
        Remove = ReactiveCommand.Create(ExecuteRemove);
    }

    public ReactiveCommand<Unit, Unit> Remove { get; }
    public IAdaptionHint AdaptionHint { get; }
    
    private void ExecuteRemove()
    {
    }
}