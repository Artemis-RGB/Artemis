using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Routing;
using Artemis.WebClient.Workshop.Models;
using Artemis.WebClient.Workshop.Services;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor;

public class ProfileViewModel : RoutableHostScreen<RoutableScreen, ProfileViewModelParameters>, IMainScreenViewModel
{
    private readonly IProfileService _profileService;
    private readonly IWorkshopService _workshopService;
    private readonly ObservableAsPropertyHelper<ViewModelBase?> _titleBarViewModel;

    public ProfileViewModel(IProfileService profileService, IWorkshopService workshopService)
    {
        _profileService = profileService;
        _workshopService = workshopService;

        _titleBarViewModel = this.WhenAnyValue(vm => vm.Screen).Select(screen => screen as IMainScreenViewModel)
            .Select(mainScreen => mainScreen?.TitleBarViewModel)
            .ToProperty(this, vm => vm.TitleBarViewModel);
    }

    public ViewModelBase? TitleBarViewModel => _titleBarViewModel.Value;

    /// <inheritdoc />
    public override async Task OnNavigating(ProfileViewModelParameters parameters, NavigationArguments args, CancellationToken cancellationToken)
    {
        ProfileConfiguration? profileConfiguration = _profileService.ProfileCategories.SelectMany(c => c.ProfileConfigurations).FirstOrDefault(c => c.ProfileId == parameters.ProfileId);

        // If the profile doesn't exist, cancel navigation
        if (profileConfiguration == null)
        {
            args.Cancel();
            return;
        }

        // If the profile is from the workshop, redirect to the workshop page
        InstalledEntry? workshopEntry = _workshopService.GetInstalledEntryByProfile(profileConfiguration);
        if (workshopEntry != null && workshopEntry.AutoUpdate)
        {
            if (!args.Path.EndsWith("workshop"))
                await args.Router.Navigate($"profile/{parameters.ProfileId}/workshop");
        }
        // Otherwise, show the profile editor if not already on the editor page
        else if (!args.Path.EndsWith("editor"))
            await args.Router.Navigate($"profile/{parameters.ProfileId}/editor");
    }
}

public class ProfileViewModelParameters
{
    public Guid ProfileId { get; set; }
}