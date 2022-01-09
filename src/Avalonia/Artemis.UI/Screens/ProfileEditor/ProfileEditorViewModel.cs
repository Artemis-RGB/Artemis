using System;
using System.Reactive.Disposables;
using Artemis.Core;
using Artemis.UI.Screens.ProfileEditor.MenuBar;
using Artemis.UI.Screens.ProfileEditor.ProfileElementProperties;
using Artemis.UI.Screens.ProfileEditor.ProfileTree;
using Artemis.UI.Screens.ProfileEditor.VisualEditor;
using Artemis.UI.Shared.Services.ProfileEditor;
using Ninject;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor
{
    public class ProfileEditorViewModel : MainScreenViewModel
    {
        private ObservableAsPropertyHelper<ProfileConfiguration?>? _profileConfiguration;
        private ObservableAsPropertyHelper<ProfileEditorHistory?>? _history;

        /// <inheritdoc />
        public ProfileEditorViewModel(IScreen hostScreen,
            IKernel kernel,
            IProfileEditorService profileEditorService,
            VisualEditorViewModel visualEditorViewModel,
            ProfileTreeViewModel profileTreeViewModel,
            ProfileEditorTitleBarViewModel profileEditorTitleBarViewModel,
            MenuBarViewModel menuBarViewModel, 
            ProfileElementPropertiesViewModel profileElementPropertiesViewModel)
            : base(hostScreen, "profile-editor")
        {
            VisualEditorViewModel = visualEditorViewModel;
            ProfileTreeViewModel = profileTreeViewModel;
            ProfileElementPropertiesViewModel = profileElementPropertiesViewModel;

            if (OperatingSystem.IsWindows())
                TitleBarViewModel = profileEditorTitleBarViewModel;
            else
                MenuBarViewModel = menuBarViewModel;

            this.WhenActivated(d => _profileConfiguration = profileEditorService.ProfileConfiguration.ToProperty(this, vm => vm.ProfileConfiguration).DisposeWith(d));
            this.WhenActivated(d => _history = profileEditorService.History.ToProperty(this, vm => vm.History).DisposeWith(d));
        }

        public VisualEditorViewModel VisualEditorViewModel { get; }
        public ProfileTreeViewModel ProfileTreeViewModel { get; }
        public MenuBarViewModel? MenuBarViewModel { get; }
        public ProfileElementPropertiesViewModel ProfileElementPropertiesViewModel { get; }

        public ProfileConfiguration? ProfileConfiguration => _profileConfiguration?.Value;
        public ProfileEditorHistory? History => _history?.Value;

        public void OpenUrl(string url)
        {
            Utilities.OpenUrl(url);
        }
    }
}