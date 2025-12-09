using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Screens.Profile.ProfileEditor.ProfileTree.Dialogs;
using Artemis.UI.Screens.ProfileEditor.ProfileTree.Dialogs;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using PropertyChanged.SourceGenerator;
using ReactiveUI;

namespace Artemis.UI.Screens.Workshop.SubmissionWizard.Steps.Profile;

public partial class ProfileAdaptionHintsLayerViewModel : ViewModelBase
{
    private readonly ObservableAsPropertyHelper<string> _adaptionHintText;
    private readonly IProfileService _profileService;
    private readonly IWindowService _windowService;
    [Notify(Setter.Private)] private int _adaptionHintCount;

    public ProfileAdaptionHintsLayerViewModel(Layer layer, IWindowService windowService, IProfileService profileService)
    {
        _windowService = windowService;
        _profileService = profileService;
        _adaptionHintText = this.WhenAnyValue(vm => vm.AdaptionHintCount).Select(c => c == 1 ? "1 adaption hint" : $"{c} adaption hints").ToProperty(this, vm => vm.AdaptionHintText);

        Layer = layer;
        EditAdaptionHints = ReactiveCommand.CreateFromTask(ExecuteEditAdaptionHints);
        AdaptionHintCount = layer.Adapter.AdaptionHints.Count;
    }

    public Layer Layer { get; }
    public ReactiveCommand<Unit, Unit> EditAdaptionHints { get; }
    public string AdaptionHintText => _adaptionHintText.Value;

    private async Task ExecuteEditAdaptionHints()
    {
        await _windowService.ShowDialogAsync<LayerHintsDialogViewModel, bool>(Layer);
        _profileService.SaveProfile(Layer.Profile, true);

        AdaptionHintCount = Layer.Adapter.AdaptionHints.Count;
    }
}