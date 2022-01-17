using System;
using System.Reactive.Linq;
using Artemis.Core;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services.ProfileEditor;
using Avalonia.Controls.Mixins;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.StatusBar;

public class StatusBarViewModel : ActivatableViewModelBase
{
    private readonly IProfileEditorService _profileEditorService;
    private ObservableAsPropertyHelper<ProfileEditorHistory?>? _history;
    private ObservableAsPropertyHelper<int>? _pixelsPerSecond;
    private ObservableAsPropertyHelper<RenderProfileElement?>? _profileElement;
    private bool _showStatusMessage;
    private string? _statusMessage;

    public StatusBarViewModel(IProfileEditorService profileEditorService)
    {
        _profileEditorService = profileEditorService;
        this.WhenActivated(d =>
        {
            _profileElement = profileEditorService.ProfileElement.ToProperty(this, vm => vm.ProfileElement).DisposeWith(d);
            _history = profileEditorService.History.ToProperty(this, vm => vm.History).DisposeWith(d);
            _pixelsPerSecond = profileEditorService.PixelsPerSecond.ToProperty(this, vm => vm.PixelsPerSecond);
        });

        this.WhenAnyValue(vm => vm.History)
            .Select(h => h?.Undo ?? Observable.Never<IProfileEditorCommand?>())
            .Switch()
            .Subscribe(c => StatusMessage = c != null ? $"Undid '{c.DisplayName}'." : "Nothing to undo.");
        this.WhenAnyValue(vm => vm.History)
            .Select(h => h?.Redo ?? Observable.Never<IProfileEditorCommand?>())
            .Switch()
            .Subscribe(c => StatusMessage = c != null ? $"Redid '{c.DisplayName}'." : "Nothing to redo.");

        this.WhenAnyValue(vm => vm.StatusMessage).Subscribe(_ => ShowStatusMessage = true);
        this.WhenAnyValue(vm => vm.StatusMessage).Throttle(TimeSpan.FromSeconds(3)).Subscribe(_ => ShowStatusMessage = false);
    }

    public RenderProfileElement? ProfileElement => _profileElement?.Value;
    public ProfileEditorHistory? History => _history?.Value;

    public int PixelsPerSecond
    {
        get => _pixelsPerSecond?.Value ?? 0;
        set => _profileEditorService.ChangePixelsPerSecond(value);
    }

    public string? StatusMessage
    {
        get => _statusMessage;
        set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
    }

    public bool ShowStatusMessage
    {
        get => _showStatusMessage;
        set => this.RaiseAndSetIfChanged(ref _showStatusMessage, value);
    }
}