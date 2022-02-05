using System;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Screens.ProfileEditor.MenuBar;
using Artemis.UI.Screens.ProfileEditor.ProfileTree;
using Artemis.UI.Screens.ProfileEditor.Properties;
using Artemis.UI.Screens.ProfileEditor.StatusBar;
using Artemis.UI.Screens.ProfileEditor.VisualEditor;
using Artemis.UI.Screens.ProfileEditor.VisualEditor.Visualizers;
using Artemis.UI.Shared.Services.ProfileEditor;
using DynamicData;
using DynamicData.Binding;
using Ninject;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor
{
    public class ProfileEditorViewModel : MainScreenViewModel
    {
        private readonly ISettingsService _settingsService;
        private ObservableAsPropertyHelper<ProfileConfiguration?>? _profileConfiguration;
        private ObservableAsPropertyHelper<ProfileEditorHistory?>? _history;
        private ReadOnlyObservableCollection<IToolViewModel> _tools;

        /// <inheritdoc />
        public ProfileEditorViewModel(IScreen hostScreen,
            IProfileEditorService profileEditorService,
            ISettingsService settingsService,
            VisualEditorViewModel visualEditorViewModel,
            ProfileTreeViewModel profileTreeViewModel,
            ProfileEditorTitleBarViewModel profileEditorTitleBarViewModel,
            MenuBarViewModel menuBarViewModel,
            PropertiesViewModel propertiesViewModel,
            StatusBarViewModel statusBarViewModel)
            : base(hostScreen, "profile-editor")
        {
            _settingsService = settingsService;
            VisualEditorViewModel = visualEditorViewModel;
            ProfileTreeViewModel = profileTreeViewModel;
            PropertiesViewModel = propertiesViewModel;
            StatusBarViewModel = statusBarViewModel;

            if (OperatingSystem.IsWindows())
                TitleBarViewModel = profileEditorTitleBarViewModel;
            else
                MenuBarViewModel = menuBarViewModel;

            this.WhenActivated(d =>
            {
                _profileConfiguration = profileEditorService.ProfileConfiguration.ToProperty(this, vm => vm.ProfileConfiguration).DisposeWith(d);
                _history = profileEditorService.History.ToProperty(this, vm => vm.History).DisposeWith(d);
                profileEditorService.Tools.Connect()
                    .Filter(t => t.ShowInToolbar)
                    .Sort(SortExpressionComparer<IToolViewModel>.Ascending(vm => vm.Order))
                    .Bind(out ReadOnlyObservableCollection<IToolViewModel> tools)
                    .Subscribe()
                    .DisposeWith(d);
                Tools = tools;
            });
        }

        public VisualEditorViewModel VisualEditorViewModel { get; }
        public ProfileTreeViewModel ProfileTreeViewModel { get; }
        public MenuBarViewModel? MenuBarViewModel { get; }
        public PropertiesViewModel PropertiesViewModel { get; }
        public StatusBarViewModel StatusBarViewModel { get; }

        public ReadOnlyObservableCollection<IToolViewModel> Tools
        {
            get => _tools;
            set => this.RaiseAndSetIfChanged(ref _tools, value);
        }

        public ProfileConfiguration? ProfileConfiguration => _profileConfiguration?.Value;
        public ProfileEditorHistory? History => _history?.Value;
        public PluginSetting<double> TreeWidth => _settingsService.GetSetting("ProfileEditor.TreeWidth", 350.0);
        public PluginSetting<double> ConditionsHeight => _settingsService.GetSetting("ProfileEditor.ConditionsHeight", 300.0);
        public PluginSetting<double> PropertiesHeight => _settingsService.GetSetting("ProfileEditor.PropertiesHeight", 300.0);

        public void OpenUrl(string url)
        {
            Utilities.OpenUrl(url);
        }
    }
}