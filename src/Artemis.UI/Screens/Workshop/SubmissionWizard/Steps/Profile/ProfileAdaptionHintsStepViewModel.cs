using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Screens.ProfileEditor.ProfileTree.Dialogs;
using Artemis.UI.Shared.Services;
using DynamicData;
using ReactiveUI;
using DynamicData.Aggregation;

namespace Artemis.UI.Screens.Workshop.SubmissionWizard.Steps.Profile;

public class ProfileAdaptionHintsStepViewModel : SubmissionViewModel
{
    private readonly IWindowService _windowService;
    private readonly IProfileService _profileService;
    private readonly SourceList<ProfileAdaptionHintsLayerViewModel> _layers;

    public ProfileAdaptionHintsStepViewModel(IWindowService windowService, IProfileService profileService, Func<Layer, ProfileAdaptionHintsLayerViewModel> getLayerViewModel)
    {
        _windowService = windowService;
        _profileService = profileService;
        _layers = new SourceList<ProfileAdaptionHintsLayerViewModel>();
        _layers.Connect().Bind(out ReadOnlyObservableCollection<ProfileAdaptionHintsLayerViewModel> layers).Subscribe();

        GoBack = ReactiveCommand.Create(() => State.ChangeScreen<ProfileSelectionStepViewModel>());
        Continue = ReactiveCommand.Create(ExecuteContinue, _layers.Connect().AutoRefresh(l => l.AdaptionHintCount).Filter(l => l.AdaptionHintCount == 0).IsEmpty());
        EditAdaptionHints = ReactiveCommand.CreateFromTask<Layer>(ExecuteEditAdaptionHints);
        Layers = layers;

        this.WhenActivated((CompositeDisposable _) =>
        {
            if (State.EntrySource is ProfileConfiguration profileConfiguration && profileConfiguration.Profile != null)
            {
                _layers.Edit(l =>
                {
                    l.Clear();
                    l.AddRange(profileConfiguration.Profile.GetAllLayers().Select(getLayerViewModel));
                });
            }
        });
    }

    public override ReactiveCommand<Unit, Unit> Continue { get; }
    public override ReactiveCommand<Unit, Unit> GoBack { get; }
    public ReactiveCommand<Layer, Unit> EditAdaptionHints { get; }
    public ReadOnlyObservableCollection<ProfileAdaptionHintsLayerViewModel> Layers { get; }

    private async Task ExecuteEditAdaptionHints(Layer layer)
    {
        await _windowService.ShowDialogAsync<LayerHintsDialogViewModel, bool>(layer);
        _profileService.SaveProfile(layer.Profile, true);
    }

    private void ExecuteContinue()
    {
        if (Layers.Any(l => l.AdaptionHintCount == 0))
            return;
        
        State.ChangeScreen<EntrySpecificationsStepViewModel>();
    }
}