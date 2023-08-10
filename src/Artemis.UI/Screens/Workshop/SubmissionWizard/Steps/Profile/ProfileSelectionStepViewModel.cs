using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.Storage.Entities.Profile;
using Artemis.Storage.Repositories.Interfaces;
using Artemis.UI.Screens.Workshop.Profile;
using ReactiveUI;

namespace Artemis.UI.Screens.Workshop.SubmissionWizard.Steps.Profile;

public class ProfileSelectionStepViewModel : SubmissionViewModel
{
    private readonly IProfileService _profileService;
    private readonly IProfileCategoryRepository _profileCategoryRepository;
    private ProfileConfiguration? _selectedProfile;

    /// <inheritdoc />
    public ProfileSelectionStepViewModel(IProfileService profileService, ProfilePreviewViewModel profilePreviewViewModel)
    {
        _profileService = profileService;
        
        // Use copies of the profiles, the originals are used by the core and could be disposed
        Profiles = new ObservableCollection<ProfileConfiguration>(_profileService.ProfileConfigurations.Select(_profileService.CloneProfileConfiguration));
        ProfilePreview = profilePreviewViewModel;

        GoBack = ReactiveCommand.Create(() => State.ChangeScreen<EntryTypeViewModel>());
        Continue = ReactiveCommand.Create(ExecuteContinue, this.WhenAnyValue(vm => vm.SelectedProfile).Select(p => p != null));

        this.WhenAnyValue(vm => vm.SelectedProfile).Subscribe(p => Update(p));
        this.WhenActivated((CompositeDisposable _) =>
        {
            if (State.EntrySource is ProfileConfiguration profileConfiguration)
                SelectedProfile = Profiles.FirstOrDefault(p => p.ProfileId == profileConfiguration.ProfileId);
        });
    }

    private void Update(ProfileConfiguration? profileConfiguration)
    {
        ProfilePreview.ProfileConfiguration = null;
        
        foreach (ProfileConfiguration configuration in Profiles)
        {
            if (configuration == profileConfiguration)
                _profileService.ActivateProfile(configuration);
            else
                _profileService.DeactivateProfile(configuration);
        }

        ProfilePreview.ProfileConfiguration = profileConfiguration;
    }

    public ObservableCollection<ProfileConfiguration> Profiles { get; }
    public ProfilePreviewViewModel ProfilePreview { get; }

    public ProfileConfiguration? SelectedProfile
    {
        get => _selectedProfile;
        set => RaiseAndSetIfChanged(ref _selectedProfile, value);
    }

    /// <inheritdoc />
    public override ReactiveCommand<Unit, Unit> Continue { get; }

    /// <inheritdoc />
    public override ReactiveCommand<Unit, Unit> GoBack { get; }

    private void ExecuteContinue()
    {
        if (SelectedProfile == null)
            return;

        State.EntrySource = SelectedProfile;
        State.Name = SelectedProfile.Name;
        State.Icon = SelectedProfile.Icon.GetIconStream();

        State.ChangeScreen<ProfileAdaptionHintsStepViewModel>();
    }
}