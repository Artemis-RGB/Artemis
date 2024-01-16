using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using Artemis.UI.DryIoc.Factories;
using Artemis.UI.Screens.Settings.Updating;
using Artemis.UI.Services.Updating;
using Artemis.UI.Shared.Routing;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.Builders;
using Artemis.WebClient.Updating;
using Avalonia.ReactiveUI;
using DynamicData;
using DynamicData.Binding;
using PropertyChanged.SourceGenerator;
using ReactiveUI;
using Serilog;
using StrawberryShake;

namespace Artemis.UI.Screens.Settings;

public partial class ReleasesTabViewModel : RoutableHostScreen<ReleaseDetailsViewModel>
{
    private readonly ILogger _logger;
    private readonly IUpdateService _updateService;
    private readonly IUpdatingClient _updatingClient;
    private readonly INotificationService _notificationService;
    private readonly IRouter _router;
    private readonly SourceList<IGetReleases_PublishedReleases_Nodes> _releases;
    [Notify(Setter.Private)] private bool _loading;
    [Notify] private ReleaseViewModel? _selectedReleaseViewModel;

    public ReleasesTabViewModel(ILogger logger, IUpdateService updateService, IUpdatingClient updatingClient, IReleaseVmFactory releaseVmFactory, INotificationService notificationService,
        IRouter router)
    {
        _logger = logger;
        _updateService = updateService;
        _updatingClient = updatingClient;
        _notificationService = notificationService;
        _router = router;

        _releases = new SourceList<IGetReleases_PublishedReleases_Nodes>();
        _releases.Connect()
            .Sort(SortExpressionComparer<IGetReleases_PublishedReleases_Nodes>.Descending(p => p.CreatedAt))
            .Transform(releaseVmFactory.ReleaseListViewModel)
            .ObserveOn(AvaloniaScheduler.Instance)
            .Bind(out ReadOnlyObservableCollection<ReleaseViewModel> releaseViewModels)
            .Subscribe();

        DisplayName = "Releases";
        RecycleScreen = false;
        ReleaseViewModels = releaseViewModels;
        Channel = _updateService.Channel;

        this.WhenAnyValue(vm => vm.SelectedReleaseViewModel).WhereNotNull().Subscribe(r => _router.Navigate($"settings/releases/{r.Release.Id}"));
        this.WhenActivated(d => _updateService.CacheLatestRelease().ToObservable().Subscribe().DisposeWith(d));
    }

    public ReadOnlyObservableCollection<ReleaseViewModel> ReleaseViewModels { get; }
    public string Channel { get; }
    
    public async Task GetReleases(CancellationToken cancellationToken)
    {
        try
        {
            Loading = true;

            IOperationResult<IGetReleasesResult> result = await _updatingClient.GetReleases.ExecuteAsync(_updateService.Channel, Platform.Windows, cancellationToken);
            if (result.Data?.PublishedReleases?.Nodes == null)
                return;

            _releases.Edit(r =>
            {
                r.Clear();
                r.AddRange(result.Data.PublishedReleases.Nodes);
            });
        }
        catch (TaskCanceledException)
        {
            // ignored
        }
        catch (Exception e)
        {
            _logger.Warning(e, "Failed to retrieve releases");
            _notificationService.CreateNotification()
                .WithTitle("Failed to retrieve releases")
                .WithMessage(e.Message)
                .WithSeverity(NotificationSeverity.Warning)
                .Show();
        }
        finally
        {
            Loading = false;
        }
    }
    
    /// <inheritdoc />
    public override async Task OnNavigating(NavigationArguments args, CancellationToken cancellationToken)
    {
        if (!ReleaseViewModels.Any())
            await GetReleases(cancellationToken);

        // If there is an ID parameter further down the path, preselect it
        if (args.RouteParameters.Length > 0 && args.RouteParameters[0] is Guid releaseId)
            SelectedReleaseViewModel = ReleaseViewModels.FirstOrDefault(vm => vm.Release.Id == releaseId);
        // Otherwise forward to the last release
        else
        {
            ReleaseViewModel? lastRelease = ReleaseViewModels.FirstOrDefault(r => r.IsCurrentVersion) ?? ReleaseViewModels.FirstOrDefault();
            if (lastRelease != null)
                await _router.Navigate($"settings/releases/{lastRelease.Release.Id}");
        }
    }
}