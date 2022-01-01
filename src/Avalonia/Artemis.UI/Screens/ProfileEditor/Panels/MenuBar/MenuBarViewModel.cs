using System;
using System.Reactive.Disposables;
using Artemis.UI.Services.ProfileEditor;
using Artemis.UI.Shared;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.Panels.MenuBar
{
    public class MenuBarViewModel : ActivatableViewModelBase
    {
        private ProfileEditorHistory? _history;

        public MenuBarViewModel(IProfileEditorService profileEditorService)
        {
            this.WhenActivated(d => profileEditorService.History.Subscribe(history => History = history).DisposeWith(d));
        }

        public ProfileEditorHistory? History
        {
            get => _history;
            set => this.RaiseAndSetIfChanged(ref _history, value);
        }
    }
}