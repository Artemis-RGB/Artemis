using System;
using System.Reactive.Disposables;
using Artemis.Core;
using Artemis.UI.Screens.ProfileEditor.Panels.MenuBar;
using Artemis.UI.Screens.ProfileEditor.ProfileTree;
using Artemis.UI.Screens.ProfileEditor.VisualEditor;
using Artemis.UI.Services.ProfileEditor;
using Ninject;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor
{
    public class ProfileEditorViewModel : MainScreenViewModel
    {
        private ProfileConfiguration? _profileConfiguration;
        private ProfileEditorHistory? _history;

        /// <inheritdoc />
        public ProfileEditorViewModel(IScreen hostScreen,
            IKernel kernel,
            IProfileEditorService profileEditorService,
            VisualEditorViewModel visualEditorViewModel,
            ProfileTreeViewModel profileTreeViewModel,
            ProfileEditorTitleBarViewModel profileEditorTitleBarViewModel,
            MenuBarViewModel menuBarViewModel)
            : base(hostScreen, "profile-editor")
        {
            VisualEditorViewModel = visualEditorViewModel;
            ProfileTreeViewModel = profileTreeViewModel;

            if (OperatingSystem.IsWindows())
                TitleBarViewModel = profileEditorTitleBarViewModel;
            else
                MenuBarViewModel = menuBarViewModel;


            this.WhenActivated(d => profileEditorService.ProfileConfiguration.WhereNotNull().Subscribe(p => ProfileConfiguration = p).DisposeWith(d));
            this.WhenActivated(d => profileEditorService.History.Subscribe(history => History = history).DisposeWith(d));
        }

        public VisualEditorViewModel VisualEditorViewModel { get; }
        public ProfileTreeViewModel ProfileTreeViewModel { get; }
        public MenuBarViewModel? MenuBarViewModel { get; }

        public ProfileConfiguration? ProfileConfiguration
        {
            get => _profileConfiguration;
            set => this.RaiseAndSetIfChanged(ref _profileConfiguration, value);
        }

        public ProfileEditorHistory? History
        {
            get => _history;
            set => this.RaiseAndSetIfChanged(ref _history, value);
        }

        public void OpenUrl(string url)
        {
            Utilities.OpenUrl(url);
        }
    }
}