using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Screens.ProfileEditor.ProfileTree.Dialogs;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using FluentAvalonia.Core;
using ReactiveUI;

namespace Artemis.UI.Screens.Workshop.SubmissionWizard.Steps.Profile;

public class ProfileAdaptionHintsLayerViewModel : ViewModelBase
{
    private readonly IWindowService _windowService;
    private readonly IProfileService _profileService;
    private readonly ObservableAsPropertyHelper<string> _adaptionHintText;
    private int _adaptionHintCount;

    public Layer Layer { get; }

    public ProfileAdaptionHintsLayerViewModel(Layer layer, IWindowService windowService, IProfileService profileService)
    {
        _windowService = windowService;
        _profileService = profileService;
        _adaptionHintText = this.WhenAnyValue(vm => vm.AdaptionHintCount).Select(c => c == 1 ? "1 adaption hint" : $"{c} adaption hints").ToProperty(this, vm => vm.AdaptionHintText);
        
        Layer = layer;
        EditAdaptionHints = ReactiveCommand.CreateFromTask(ExecuteEditAdaptionHints);
        AdaptionHintCount = layer.Adapter.AdaptionHints.Count;
    }

    public ReactiveCommand<Unit, Unit> EditAdaptionHints { get; }

    public int AdaptionHintCount
    {
        get => _adaptionHintCount;
        private set => RaiseAndSetIfChanged(ref _adaptionHintCount, value);
    }

    public string AdaptionHintText => _adaptionHintText.Value;

    private async Task ExecuteEditAdaptionHints()
    {
        await _windowService.ShowDialogAsync<LayerHintsDialogViewModel, bool>(Layer);
        _profileService.SaveProfile(Layer.Profile, true);

        AdaptionHintCount = Layer.Adapter.AdaptionHints.Count;
    }
}