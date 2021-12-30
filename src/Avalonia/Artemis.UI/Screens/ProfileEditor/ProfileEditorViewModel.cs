using System;
using System.Reactive.Disposables;
using Artemis.Core;
using Artemis.UI.Screens.ProfileEditor.Panels.MenuBar;
using Artemis.UI.Screens.ProfileEditor.ProfileTree;
using Artemis.UI.Screens.ProfileEditor.VisualEditor;
using Artemis.UI.Services;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor
{
    public class ProfileEditorViewModel : MainScreenViewModel
    {
        private ProfileConfiguration? _profile;

        /// <inheritdoc />
        public ProfileEditorViewModel(IScreen hostScreen,
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

            this.WhenActivated(disposables => profileEditorService.CurrentProfileConfiguration.WhereNotNull().Subscribe(p => Profile = p).DisposeWith(disposables));
        }

        public VisualEditorViewModel VisualEditorViewModel { get; }
        public ProfileTreeViewModel ProfileTreeViewModel { get; }
        public MenuBarViewModel? MenuBarViewModel { get; }

        public ProfileConfiguration? Profile
        {
            get => _profile;
            set => this.RaiseAndSetIfChanged(ref _profile, value);
        }

        public void OpenUrl(string url)
        {
            Utilities.OpenUrl(url);
        }
    }
}