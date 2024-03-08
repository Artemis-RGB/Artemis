using System;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Routing;
using Artemis.UI.Shared.Services;
using ReactiveUI;

namespace Artemis.UI.Screens.Sidebar;

public class SidebarProfileConfigurationViewModel : ActivatableViewModelBase
{
    private readonly IRouter _router;
    private readonly IProfileService _profileService;
    private readonly IWindowService _windowService;
    private ObservableAsPropertyHelper<bool>? _isDisabled;

    public SidebarProfileConfigurationViewModel(IRouter router, ProfileConfiguration profileConfiguration, IProfileService profileService, IWindowService windowService)
    {
        _router = router;
        _profileService = profileService;
        _windowService = windowService;

        ProfileConfiguration = profileConfiguration;
        EditProfile = ReactiveCommand.CreateFromTask(ExecuteEditProfile);
        ToggleSuspended = ReactiveCommand.Create(ExecuteToggleSuspended);
        ResumeAll = ReactiveCommand.Create<string>(ExecuteResumeAll);
        SuspendAll = ReactiveCommand.Create<string>(ExecuteSuspendAll);
        DeleteProfile = ReactiveCommand.CreateFromTask(ExecuteDeleteProfile);
        ExportProfile = ReactiveCommand.CreateFromTask(ExecuteExportProfile);
        DuplicateProfile = ReactiveCommand.CreateFromTask(ExecuteDuplicateProfile);

        this.WhenActivated(d => _isDisabled = ProfileConfiguration.WhenAnyValue(c => c.Profile)
            .Select(p => p == null)
            .ToProperty(this, vm => vm.IsDisabled)
            .DisposeWith(d));
    }

    public ProfileConfiguration ProfileConfiguration { get; }
    public ReactiveCommand<Unit, Unit> EditProfile { get; }
    public ReactiveCommand<Unit, Unit> ToggleSuspended { get; }
    public ReactiveCommand<string, Unit> ResumeAll { get; }
    public ReactiveCommand<string, Unit> SuspendAll { get; }
    public ReactiveCommand<Unit, Unit> DeleteProfile { get; }
    public ReactiveCommand<Unit, Unit> ExportProfile { get; }
    public ReactiveCommand<Unit, Unit> DuplicateProfile { get; }

    public bool IsDisabled => _isDisabled?.Value ?? false;

    private async Task ExecuteEditProfile()
    {
        await _windowService.ShowDialogAsync<ProfileConfigurationEditViewModel, ProfileConfiguration?>(ProfileConfiguration.Category, ProfileConfiguration);
    }

    private void ExecuteToggleSuspended()
    {
        ProfileConfiguration.IsSuspended = !ProfileConfiguration.IsSuspended;
        _profileService.SaveProfileCategory(ProfileConfiguration.Category);
    }

    private void ExecuteResumeAll(string direction)
    {
        int index = ProfileConfiguration.Category.ProfileConfigurations.IndexOf(ProfileConfiguration);
        if (direction == "above")
            for (int i = 0; i < index; i++)
                ProfileConfiguration.Category.ProfileConfigurations[i].IsSuspended = false;
        else
            for (int i = index + 1; i < ProfileConfiguration.Category.ProfileConfigurations.Count; i++)
                ProfileConfiguration.Category.ProfileConfigurations[i].IsSuspended = false;

        _profileService.SaveProfileCategory(ProfileConfiguration.Category);
    }

    private void ExecuteSuspendAll(string direction)
    {
        int index = ProfileConfiguration.Category.ProfileConfigurations.IndexOf(ProfileConfiguration);
        if (direction == "above")
            for (int i = 0; i < index; i++)
                ProfileConfiguration.Category.ProfileConfigurations[i].IsSuspended = true;
        else
            for (int i = index + 1; i < ProfileConfiguration.Category.ProfileConfigurations.Count; i++)
                ProfileConfiguration.Category.ProfileConfigurations[i].IsSuspended = true;

        _profileService.SaveProfileCategory(ProfileConfiguration.Category);
    }

    private async Task ExecuteDeleteProfile()
    {
        if (!await _windowService.ShowConfirmContentDialog("Delete profile", "Are you sure you want to permanently delete this profile?"))
            return;

        if (_profileService.FocusProfile == ProfileConfiguration)
            await _router.Navigate("home");
        _profileService.RemoveProfileConfiguration(ProfileConfiguration);
    }

    private async Task ExecuteExportProfile()
    {
        // Might not cover everything but then the dialog will complain and that's good enough
        string fileName = Path.GetInvalidFileNameChars().Aggregate(ProfileConfiguration.Name, (current, c) => current.Replace(c, '-'));
        string? result = await _windowService.CreateSaveFileDialog()
            .HavingFilter(f => f.WithExtension("zip").WithName("Artemis profile"))
            .WithInitialFileName(fileName)
            .ShowAsync();

        if (result == null)
            return;

        try
        {
            await using Stream stream = await _profileService.ExportProfile(ProfileConfiguration);
            await using FileStream fileStream = File.OpenWrite(result);
            await stream.CopyToAsync(fileStream);
        }
        catch (Exception e)
        {
            _windowService.ShowExceptionDialog("Failed to export profile", e);
        }
    }

    private async Task ExecuteDuplicateProfile()
    {
        await using Stream export = await _profileService.ExportProfile(ProfileConfiguration);
        await _profileService.ImportProfile(export, ProfileConfiguration.Category, true, false, "copy");
    }

    public bool Matches(string s)
    {
        return s.StartsWith("profile-editor") && s.EndsWith(ProfileConfiguration.ProfileId.ToString());
    }
}