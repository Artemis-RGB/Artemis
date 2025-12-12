using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Screens.ProfileEditor.ProfileTree.Dialogs;
using Artemis.UI.Shared.Services;
using Artemis.WebClient.Workshop.Handlers.UploadHandlers;
using DynamicData;
using DynamicData.Aggregation;
using ReactiveUI;

namespace Artemis.UI.Screens.Workshop.SubmissionWizard.Steps.Profile;

public class ProfileAdaptionHintsStepViewModel : SubmissionViewModel
{
    private readonly SourceList<ProfileAdaptionHintsLayerViewModel> _layers;
    private readonly IProfileService _profileService;
    private readonly IWindowService _windowService;

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
            if (State.EntrySource is not ProfileEntrySource profileEntrySource || profileEntrySource.ProfileConfiguration.Profile == null)
                return;
            
            _layers.Edit(l =>
            {
                l.Clear();
                l.AddRange(profileEntrySource.ProfileConfiguration.Profile.GetAllLayers().Select(getLayerViewModel));
            });
        });
    }

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

        if (State.EntryId == null)
            State.ChangeScreen<SpecificationsStepViewModel>();
        else
            State.ChangeScreen<ChangelogStepViewModel>();
    }
}