using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services.ProfileEditor;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.MenuBar;

public class MenuBarViewModel : ActivatableViewModelBase
{
    private readonly IProfileService _profileService;
    private ProfileEditorHistory? _history;
    private ObservableAsPropertyHelper<ProfileConfiguration?>? _profileConfiguration;
    private ObservableAsPropertyHelper<bool>? _isSuspended;

    public MenuBarViewModel(IProfileEditorService profileEditorService, IProfileService profileService)
    {
        _profileService = profileService;
        this.WhenActivated(d =>
        {
            profileEditorService.History.Subscribe(history => History = history).DisposeWith(d);
            _profileConfiguration = profileEditorService.ProfileConfiguration.ToProperty(this, vm => vm.ProfileConfiguration).DisposeWith(d);
            _isSuspended = profileEditorService.ProfileConfiguration
                .Select(p => p?.WhenAnyValue(c => c.IsSuspended) ?? Observable.Never<bool>())
                .Switch()
                .ToProperty(this, vm => vm.IsSuspended)
                .DisposeWith(d);
        });
    }

    public ProfileEditorHistory? History
    {
        get => _history;
        set => RaiseAndSetIfChanged(ref _history, value);
    }

    public ProfileConfiguration? ProfileConfiguration => _profileConfiguration?.Value;

    public bool IsSuspended
    {
        get => _isSuspended?.Value ?? false;
        set
        {
            if (ProfileConfiguration == null)
                return;

            ProfileConfiguration.IsSuspended = value;
            _profileService.SaveProfileCategory(ProfileConfiguration.Category);
        }
    }
}