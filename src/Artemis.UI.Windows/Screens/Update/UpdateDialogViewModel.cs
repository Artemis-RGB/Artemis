using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Providers;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.Builders;
using Artemis.UI.Windows.Models;
using Artemis.UI.Windows.Providers;
using Avalonia.Threading;
using DynamicData;
using ReactiveUI;

namespace Artemis.UI.Windows.Screens.Update;

public class UpdateDialogViewModel : DialogViewModelBase<bool>
{
    // Based on https://docs.microsoft.com/en-us/azure/devops/pipelines/repos/github?view=azure-devops&tabs=yaml#skipping-ci-for-individual-commits
    private readonly string[] _excludedCommitMessages =
    {
        "[skip ci]",
        "[ci skip]",
        "skip-checks: true",
        "skip-checks:true",
        "[skip azurepipelines]",
        "[azurepipelines skip]",
        "[skip azpipelines]",
        "[azpipelines skip]",
        "[skip azp]",
        "[azp skip]",
        "***NO_CI***"
    };

    private readonly INotificationService _notificationService;
    private readonly UpdateProvider _updateProvider;
    private bool _hasChanges;
    private string? _latestBuild;

    private bool _retrievingChanges;

    public UpdateDialogViewModel(string channel, IUpdateProvider updateProvider, INotificationService notificationService)
    {
        _updateProvider = (UpdateProvider) updateProvider;
        _notificationService = notificationService;

        Channel = channel;
        CurrentBuild = Constants.BuildInfo.BuildNumberDisplay;

        this.WhenActivated((CompositeDisposable _) => Dispatcher.UIThread.InvokeAsync(GetBuildChanges));
        Install = ReactiveCommand.Create(() => Close(true));
        AskLater = ReactiveCommand.Create(() => Close(false));
    }

    public ReactiveCommand<Unit, Unit> Install { get; }
    public ReactiveCommand<Unit, Unit> AskLater { get; }

    public string Channel { get; }
    public string CurrentBuild { get; }

    public ObservableCollection<string> Changes { get; } = new();

    public bool RetrievingChanges
    {
        get => _retrievingChanges;
        set => RaiseAndSetIfChanged(ref _retrievingChanges, value);
    }

    public bool HasChanges
    {
        get => _hasChanges;
        set => RaiseAndSetIfChanged(ref _hasChanges, value);
    }

    public string? LatestBuild
    {
        get => _latestBuild;
        set => RaiseAndSetIfChanged(ref _latestBuild, value);
    }

    private async Task GetBuildChanges()
    {
        try
        {
            RetrievingChanges = true;
            Task<DevOpsBuild?> currentTask = _updateProvider.GetBuildInfo(1, CurrentBuild);
            Task<DevOpsBuild?> latestTask = _updateProvider.GetBuildInfo(1);

            DevOpsBuild? current = await currentTask;
            DevOpsBuild? latest = await latestTask;

            LatestBuild = latest?.BuildNumber;
            if (current != null && latest != null)
            {
                GitHubDifference difference = await _updateProvider.GetBuildDifferences(current, latest);

                // Only take commits with one parents (no merges)
                Changes.Clear();
                Changes.AddRange(difference.Commits.Where(c => c.Parents.Count == 1)
                    .SelectMany(c => c.Commit.Message.Split("\n"))
                    .Select(m => m.Trim())
                    .Where(m => !string.IsNullOrWhiteSpace(m) && !_excludedCommitMessages.Contains(m))
                    .OrderBy(m => m)
                );
                HasChanges = Changes.Any();
            }
        }
        catch (Exception e)
        {
            _notificationService.CreateNotification().WithTitle("Failed to retrieve build changes").WithMessage(e.Message).WithSeverity(NotificationSeverity.Error).Show();
        }
        finally
        {
            RetrievingChanges = false;
        }
    }
}