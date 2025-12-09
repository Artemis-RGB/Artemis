using System.Reactive;
using Artemis.Core;
using Artemis.UI.Shared;
using ReactiveUI;

namespace Artemis.UI.Screens.Profile.ProfileEditor.ProfileTree.Dialogs.AdaptionHints;

public abstract class AdaptionHintViewModelBase : ViewModelBase
{
    protected AdaptionHintViewModelBase(Layer layer, IAdaptionHint adaptionHint)
    {
        Layer = layer;
        AdaptionHint = adaptionHint;
        Remove = ReactiveCommand.Create(ExecuteRemove);
    }

    public Layer Layer { get; }
    public IAdaptionHint AdaptionHint { get; }
    public ReactiveCommand<Unit, Unit> Remove { get; }

    private void ExecuteRemove()
    {
        Layer.Adapter.Remove(AdaptionHint);
    }
}