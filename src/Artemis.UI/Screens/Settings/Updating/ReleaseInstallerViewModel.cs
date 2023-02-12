using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.UI.Extensions;
using Artemis.UI.Services.Updating;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using ReactiveUI;

namespace Artemis.UI.Screens.Settings.Updating;

public class ReleaseInstallerViewModel : ActivatableViewModelBase
{
    private readonly ReleaseInstaller _releaseInstaller;
    private readonly IWindowService _windowService;
    private ObservableAsPropertyHelper<float>? _overallProgress;
    private ObservableAsPropertyHelper<float>? _stepProgress;
    private bool _ready;
    private bool _restartWhenFinished;

    public ReleaseInstallerViewModel(ReleaseInstaller releaseInstaller, IWindowService windowService)
    {
        _releaseInstaller = releaseInstaller;
        _windowService = windowService;

        Restart = ReactiveCommand.Create(() => Utilities.ApplyUpdate(false));
        this.WhenActivated(d =>
        {
            _overallProgress = Observable.FromEventPattern<float>(x => _releaseInstaller.OverallProgress.ProgressChanged += x, x => _releaseInstaller.OverallProgress.ProgressChanged -= x)
                .Select(e => e.EventArgs)
                .ToProperty(this, vm => vm.OverallProgress)
                .DisposeWith(d);
            _stepProgress = Observable.FromEventPattern<float>(x => _releaseInstaller.StepProgress.ProgressChanged += x, x => _releaseInstaller.StepProgress.ProgressChanged -= x)
                .Select(e => e.EventArgs)
                .ToProperty(this, vm => vm.StepProgress)
                .DisposeWith(d);

            Task.Run(() => InstallUpdate(d.AsCancellationToken()));
        });
    }

    public ReactiveCommand<Unit, Unit> Restart { get; }

    public float OverallProgress => _overallProgress?.Value ?? 0;
    public float StepProgress => _stepProgress?.Value ?? 0;

    public bool Ready
    {
        get => _ready;
        set => RaiseAndSetIfChanged(ref _ready, value);
    }

    public bool RestartWhenFinished
    {
        get => _restartWhenFinished;
        set => RaiseAndSetIfChanged(ref _restartWhenFinished, value);
    }

    private async Task InstallUpdate(CancellationToken cancellationToken)
    {
        try
        {
            await _releaseInstaller.InstallAsync(cancellationToken);
            Ready = true;
            if (RestartWhenFinished)
                Utilities.ApplyUpdate(false);
        }
        catch (TaskCanceledException)
        {
            // ignored
        }
        catch (Exception e)
        {
            _windowService.ShowExceptionDialog("Something went wrong while installing the update", e);
        }
    }
}