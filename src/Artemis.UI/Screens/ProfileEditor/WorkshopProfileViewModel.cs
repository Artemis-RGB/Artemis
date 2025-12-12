using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Screens.Workshop.Library.Tabs;
using Artemis.UI.Shared.Routing;
using Artemis.WebClient.Workshop.Models;
using Artemis.WebClient.Workshop.Services;
using PropertyChanged.SourceGenerator;

namespace Artemis.UI.Screens.ProfileEditor;

public partial class WorkshopProfileViewModel : RoutableScreen<ProfileViewModelParameters>
{
    private readonly IProfileService _profileService;
    private readonly IWorkshopService _workshopService;
    private readonly IRouter _router;
    private readonly Func<InstalledEntry, InstalledTabItemViewModel> _getInstalledTabItemViewModel;

    [Notify] private ProfileConfiguration? _profileConfiguration;
    [Notify] private InstalledEntry? _workshopEntry;
    [Notify] private InstalledTabItemViewModel? _entryViewModel;

    public WorkshopProfileViewModel(IProfileService profileService, IWorkshopService workshopService, IRouter router, Func<InstalledEntry, InstalledTabItemViewModel> getInstalledTabItemViewModel)
    {
        _profileService = profileService;
        _workshopService = workshopService;
        _router = router;
        _getInstalledTabItemViewModel = getInstalledTabItemViewModel;
        ParameterSource = ParameterSource.Route;
    }

    public async Task DisableAutoUpdate()
    {
        if (WorkshopEntry != null)
        {
            _workshopService.SetAutoUpdate(WorkshopEntry, false);
        }

        if (ProfileConfiguration != null)
        {
            await _router.Navigate($"profile/{ProfileConfiguration.ProfileId}/editor");
        }
    }

    public override Task OnNavigating(ProfileViewModelParameters parameters, NavigationArguments args, CancellationToken cancellationToken)
    {
        ProfileConfiguration = _profileService.ProfileCategories.SelectMany(c => c.ProfileConfigurations).FirstOrDefault(c => c.ProfileId == parameters.ProfileId);

        // If the profile doesn't exist, cancel navigation
        if (ProfileConfiguration == null)
        {
            args.Cancel();
            return Task.CompletedTask;
        }

        WorkshopEntry = _workshopService.GetInstalledEntryByProfile(ProfileConfiguration);
        EntryViewModel = WorkshopEntry != null ? _getInstalledTabItemViewModel(WorkshopEntry) : null;
        if (EntryViewModel != null)
            EntryViewModel.DisplayManagement = false;
        return Task.CompletedTask;
    }
}