using System;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using Artemis.Core;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services.ProfileEditor;
using PropertyChanged.SourceGenerator;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.StatusBar;

public partial class StatusBarViewModel : ActivatableViewModelBase
{
    private readonly IProfileEditorService _profileEditorService;
    private ObservableAsPropertyHelper<ProfileEditorHistory?>? _history;
    private ObservableAsPropertyHelper<int>? _pixelsPerSecond;
    private ObservableAsPropertyHelper<RenderProfileElement?>? _profileElement;
    [Notify] private bool _showStatusMessage;
    [Notify] private string? _statusMessage;

    public StatusBarViewModel(IProfileEditorService profileEditorService)
    {
        _profileEditorService = profileEditorService;
        this.WhenActivated(d =>
        {
            _profileElement = profileEditorService.ProfileElement.ToProperty(this, vm => vm.ProfileElement).DisposeWith(d);
            _history = profileEditorService.History.ToProperty(this, vm => vm.History).DisposeWith(d);
            _pixelsPerSecond = profileEditorService.PixelsPerSecond.ToProperty(this, vm => vm.PixelsPerSecond).DisposeWith(d);
        });

        this.WhenAnyValue(vm => vm.History)
            .Select(h => h?.Undo ?? Observable.Never<IProfileEditorCommand?>())
            .Switch()
            .Subscribe(c =>
            {
                StatusMessage = c != null ? $"Undid '{c.DisplayName}'." : "Nothing to undo.";
                ShowStatusMessage = true;
            });
        this.WhenAnyValue(vm => vm.History)
            .Select(h => h?.Redo ?? Observable.Never<IProfileEditorCommand?>())
            .Switch()
            .Subscribe(c =>
            {
                StatusMessage = c != null ? $"Redid '{c.DisplayName}'." : "Nothing to redo.";
                ShowStatusMessage = true;
            });

        this.WhenAnyValue(vm => vm.ShowStatusMessage).Where(v => v).Throttle(TimeSpan.FromSeconds(3)).Subscribe(_ => ShowStatusMessage = false);
    }

    public RenderProfileElement? ProfileElement => _profileElement?.Value;
    public ProfileEditorHistory? History => _history?.Value;

    public int PixelsPerSecond
    {
        get => _pixelsPerSecond?.Value ?? 0;
        set => _profileEditorService.ChangePixelsPerSecond(value);
    }
}