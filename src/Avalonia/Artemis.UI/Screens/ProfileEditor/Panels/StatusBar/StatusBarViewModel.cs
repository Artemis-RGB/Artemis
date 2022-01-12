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
    private ProfileEditorHistory? _history;
    private RenderProfileElement? _profileElement;
    private string? _statusMessage;
    private bool _showStatusMessage;

    public StatusBarViewModel(IProfileEditorService profileEditorService)
    {
        this.WhenActivated(d =>
        {
            profileEditorService.ProfileElement.Subscribe(p => ProfileElement = p).DisposeWith(d);
            profileEditorService.History.Subscribe(history => History = history).DisposeWith(d);
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

    public RenderProfileElement? ProfileElement
    {
        get => _profileElement;
        set => this.RaiseAndSetIfChanged(ref _profileElement, value);
    }

    public ProfileEditorHistory? History
    {
        get => _history;
        set => this.RaiseAndSetIfChanged(ref _history, value);
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