using System;
using System.Reactive.Disposables;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services.ProfileEditor;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.MenuBar
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