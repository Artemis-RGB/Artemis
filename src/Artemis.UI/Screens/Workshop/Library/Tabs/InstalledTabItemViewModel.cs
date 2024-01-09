using System;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Routing;
using Artemis.UI.Shared.Services;
using Artemis.WebClient.Workshop;
using Artemis.WebClient.Workshop.Handlers.InstallationHandlers;
using Artemis.WebClient.Workshop.Services;
using PropertyChanged.SourceGenerator;
using ReactiveUI;

namespace Artemis.UI.Screens.Workshop.Library.Tabs;

public partial class InstalledTabItemViewModel : ViewModelBase
{
    private readonly IWorkshopService _workshopService;
    private readonly IRouter _router;
    private readonly EntryInstallationHandlerFactory _factory;
    private readonly IWindowService _windowService;
    [Notify(Setter.Private)] private bool _isRemoved;

    public InstalledTabItemViewModel(InstalledEntry installedEntry, IWorkshopService workshopService, IRouter router, EntryInstallationHandlerFactory factory, IWindowService windowService)
    {
        _workshopService = workshopService;
        _router = router;
        _factory = factory;
        _windowService = windowService;
        InstalledEntry = installedEntry;

        ViewWorkshopPage = ReactiveCommand.CreateFromTask(ExecuteViewWorkshopPage);
        ViewLocal = ReactiveCommand.CreateFromTask(ExecuteViewLocal);
        Uninstall = ReactiveCommand.CreateFromTask(ExecuteUninstall);
    }

    public InstalledEntry InstalledEntry { get; }
    public ReactiveCommand<Unit, Unit> ViewWorkshopPage { get; }
    public ReactiveCommand<Unit,Unit> ViewLocal { get; }
    public ReactiveCommand<Unit, Unit> Uninstall { get; }

    private async Task ExecuteViewWorkshopPage()
    {
        await _workshopService.NavigateToEntry(InstalledEntry.EntryId, InstalledEntry.EntryType);
    }

    private async Task ExecuteViewLocal(CancellationToken cancellationToken)
    {
        if (InstalledEntry.EntryType == EntryType.Profile && InstalledEntry.TryGetMetadata("ProfileId", out Guid profileId))
        {
            await _router.Navigate($"profile-editor/{profileId}");
        }
    }
    
    private async Task ExecuteUninstall(CancellationToken cancellationToken)
    {
        bool confirmed = await _windowService.ShowConfirmContentDialog("Do you want to uninstall this entry?", "Both the entry and its contents will be removed.");
        if (!confirmed)
            return;
        
        IEntryInstallationHandler handler = _factory.CreateHandler(InstalledEntry.EntryType);
        await handler.UninstallAsync(InstalledEntry, cancellationToken);
        IsRemoved = true;
    }
}