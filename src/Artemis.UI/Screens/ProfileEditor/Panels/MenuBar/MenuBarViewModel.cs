using System;
using System.Reactive;
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
    private readonly ISettingsService _settingsService;
    private ProfileEditorHistory? _history;
    private ObservableAsPropertyHelper<ProfileConfiguration?>? _profileConfiguration;
    private ObservableAsPropertyHelper<bool>? _isSuspended;

    public MenuBarViewModel(IProfileEditorService profileEditorService, IProfileService profileService, ISettingsService settingsService)
    {
        _profileService = profileService;
        _settingsService = settingsService;
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

        ToggleBooleanSetting = ReactiveCommand.Create<PluginSetting<bool>>(ExecuteToggleBooleanSetting);
    }

    private void ExecuteToggleBooleanSetting(PluginSetting<bool> setting)
    {
        setting.Value = !setting.Value;
        setting.Save();
    }

    public ReactiveCommand<PluginSetting<bool>, Unit> ToggleBooleanSetting { get; }
    public ProfileConfiguration? ProfileConfiguration => _profileConfiguration?.Value;
    public PluginSetting<bool> FocusSelectedLayer => _settingsService.GetSetting("ProfileEditor.FocusSelectedLayer", false);
    public PluginSetting<bool> ShowDataModelValues => _settingsService.GetSetting("ProfileEditor.ShowDataModelValues", false);
    public PluginSetting<bool> ShowFullPaths => _settingsService.GetSetting("ProfileEditor.ShowFullPaths", false);
    public PluginSetting<bool> AlwaysShowValues => _settingsService.GetSetting("ProfileEditor.AlwaysShowValues", false);
    public PluginSetting<bool> AlwaysApplyDataBindings => _settingsService.GetSetting("ProfileEditor.AlwaysApplyDataBindings", false);

    public ProfileEditorHistory? History
    {
        get => _history;
        set => RaiseAndSetIfChanged(ref _history, value);
    }

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