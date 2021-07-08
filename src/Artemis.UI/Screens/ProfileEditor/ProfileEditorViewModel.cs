using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Events;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.ProfileEditor.DisplayConditions;
using Artemis.UI.Screens.ProfileEditor.LayerProperties;
using Artemis.UI.Screens.ProfileEditor.ProfileTree;
using Artemis.UI.Screens.ProfileEditor.Visualization;
using Artemis.UI.Screens.Sidebar;
using Artemis.UI.Services;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using MaterialDesignThemes.Wpf;
using Stylet;
using ProfileConfigurationEventArgs = Artemis.UI.Shared.ProfileConfigurationEventArgs;

namespace Artemis.UI.Screens.ProfileEditor
{
    public class ProfileEditorViewModel : MainScreenViewModel, IHandle<MainWindowFocusChangedEvent>
    {
        private readonly IDebugService _debugService;
        private readonly IMessageService _messageService;
        private readonly IProfileEditorService _profileEditorService;
        private readonly IProfileService _profileService;
        private readonly IScriptVmFactory _scriptVmFactory;
        private readonly ISettingsService _settingsService;
        private readonly ISidebarVmFactory _sidebarVmFactory;
        private readonly IWindowManager _windowManager;
        private readonly IEventAggregator _eventAggregator;
        private DisplayConditionsViewModel _displayConditionsViewModel;
        private LayerPropertiesViewModel _layerPropertiesViewModel;
        private ProfileTreeViewModel _profileTreeViewModel;
        private ProfileViewModel _profileViewModel;
        private bool _suspendedManually;

        public ProfileEditorViewModel(ProfileViewModel profileViewModel,
            ProfileTreeViewModel profileTreeViewModel,
            DisplayConditionsViewModel dataModelConditionsViewModel,
            LayerPropertiesViewModel layerPropertiesViewModel,
            IProfileEditorService profileEditorService,
            IProfileService profileService,
            IDialogService dialogService,
            ISettingsService settingsService,
            IMessageService messageService,
            IDebugService debugService,
            IWindowManager windowManager,
            IEventAggregator eventAggregator,
            IScriptVmFactory scriptVmFactory,
            ISidebarVmFactory sidebarVmFactory)
        {
            _profileEditorService = profileEditorService;
            _profileService = profileService;
            _settingsService = settingsService;
            _messageService = messageService;
            _debugService = debugService;
            _windowManager = windowManager;
            _eventAggregator = eventAggregator;
            _scriptVmFactory = scriptVmFactory;
            _sidebarVmFactory = sidebarVmFactory;

            DisplayName = "Profile Editor";
            DialogService = dialogService;

            // Populate the panels
            ProfileViewModel = profileViewModel;
            ProfileViewModel.ConductWith(this);
            ProfileTreeViewModel = profileTreeViewModel;
            ProfileTreeViewModel.ConductWith(this);
            DisplayConditionsViewModel = dataModelConditionsViewModel;
            DisplayConditionsViewModel.ConductWith(this);
            LayerPropertiesViewModel = layerPropertiesViewModel;
            LayerPropertiesViewModel.ConductWith(this);
        }

        public IDialogService DialogService { get; }

        public ProfileConfiguration ProfileConfiguration => _profileEditorService.SelectedProfileConfiguration;

        public DisplayConditionsViewModel DisplayConditionsViewModel
        {
            get => _displayConditionsViewModel;
            set => SetAndNotify(ref _displayConditionsViewModel, value);
        }

        public LayerPropertiesViewModel LayerPropertiesViewModel
        {
            get => _layerPropertiesViewModel;
            set => SetAndNotify(ref _layerPropertiesViewModel, value);
        }

        public ProfileTreeViewModel ProfileTreeViewModel
        {
            get => _profileTreeViewModel;
            set => SetAndNotify(ref _profileTreeViewModel, value);
        }

        public ProfileViewModel ProfileViewModel
        {
            get => _profileViewModel;
            set => SetAndNotify(ref _profileViewModel, value);
        }

        public bool SuspendedManually
        {
            get => _suspendedManually;
            set => SetAndNotify(ref _suspendedManually, value);
        }

        public PluginSetting<GridLength> SidePanelsWidth => _settingsService.GetSetting("ProfileEditor.SidePanelsWidth", new GridLength(385));
        public PluginSetting<GridLength> DataModelConditionsHeight => _settingsService.GetSetting("ProfileEditor.DataModelConditionsHeight", new GridLength(345));
        public PluginSetting<GridLength> BottomPanelsHeight => _settingsService.GetSetting("ProfileEditor.BottomPanelsHeight", new GridLength(265));
        public PluginSetting<GridLength> ElementPropertiesWidth => _settingsService.GetSetting("ProfileEditor.ElementPropertiesWidth", new GridLength(545));
        public PluginSetting<bool> StopOnFocusLoss => _settingsService.GetSetting("ProfileEditor.StopOnFocusLoss", true);
        public PluginSetting<bool> ShowDataModelValues => _settingsService.GetSetting("ProfileEditor.ShowDataModelValues", false);
        public PluginSetting<bool> ShowFullPaths => _settingsService.GetSetting("ProfileEditor.ShowFullPaths", true);
        public PluginSetting<bool> FocusSelectedLayer => _settingsService.GetSetting("ProfileEditor.FocusSelectedLayer", true);
        public PluginSetting<bool> AlwaysApplyDataBindings => _settingsService.GetSetting("ProfileEditor.AlwaysApplyDataBindings", true);

        public void Undo()
        {
            // Expanded status is also undone because undoing works a bit crude, that's annoying
            List<LayerPropertyGroupViewModel> beforeGroups = LayerPropertiesViewModel.GetAllLayerPropertyGroupViewModels();
            List<string> expandedPaths = beforeGroups.Where(g => g.IsExpanded).Select(g => g.LayerPropertyGroup.Path).ToList();
            // Store the focused element so we can restore it later
            IInputElement focusedElement = FocusManager.GetFocusedElement(Window.GetWindow(View));

            if (!_profileEditorService.UndoSaveProfile())
            {
                _messageService.ShowMessage("Nothing to undo");
                return;
            }

            // Restore the expanded status
            foreach (LayerPropertyGroupViewModel allLayerPropertyGroupViewModel in LayerPropertiesViewModel.GetAllLayerPropertyGroupViewModels())
                allLayerPropertyGroupViewModel.IsExpanded = expandedPaths.Contains(allLayerPropertyGroupViewModel.LayerPropertyGroup.Path);
            // Restore the focused element
            Execute.PostToUIThread(async () =>
            {
                await Task.Delay(50);
                focusedElement?.Focus();
            });

            _messageService.ShowMessage("Undid profile update", "REDO", Redo);
        }

        public void Redo()
        {
            // Expanded status is also undone because undoing works a bit crude, that's annoying
            List<LayerPropertyGroupViewModel> beforeGroups = LayerPropertiesViewModel.GetAllLayerPropertyGroupViewModels();
            List<string> expandedPaths = beforeGroups.Where(g => g.IsExpanded).Select(g => g.LayerPropertyGroup.Path).ToList();
            // Store the focused element so we can restore it later
            IInputElement focusedElement = FocusManager.GetFocusedElement(Window.GetWindow(View));

            if (!_profileEditorService.RedoSaveProfile())
            {
                _messageService.ShowMessage("Nothing to redo");
                return;
            }

            // Restore the expanded status
            foreach (LayerPropertyGroupViewModel allLayerPropertyGroupViewModel in LayerPropertiesViewModel.GetAllLayerPropertyGroupViewModels())
                allLayerPropertyGroupViewModel.IsExpanded = expandedPaths.Contains(allLayerPropertyGroupViewModel.LayerPropertyGroup.Path);
            // Restore the focused element
            Execute.PostToUIThread(async () =>
            {
                await Task.Delay(50);
                focusedElement?.Focus();
            });

            _messageService.ShowMessage("Redid profile update", "UNDO", Undo);
        }

        public void ToggleSuspend()
        {
            _profileEditorService.SuspendEditing = !_profileEditorService.SuspendEditing;
            SuspendedManually = _profileEditorService.SuspendEditing;
        }

        public void ToggleAutoSuspend()
        {
            StopOnFocusLoss.Value = !StopOnFocusLoss.Value;
        }

        #region Overrides of Screen

        protected override void OnInitialActivate()
        {
            StopOnFocusLoss.AutoSave = true;
            ShowDataModelValues.AutoSave = true;
            ShowFullPaths.AutoSave = true;
            FocusSelectedLayer.AutoSave = true;
            AlwaysApplyDataBindings.AutoSave = true;

            _profileEditorService.SelectedProfileChanged += ProfileEditorServiceOnSelectedProfileChanged;
            _profileEditorService.SelectedProfileElementChanged += ProfileEditorServiceOnSelectedProfileElementChanged;
            _eventAggregator.Subscribe(this);
            base.OnInitialActivate();
        }

        protected override void OnClose()
        {
            StopOnFocusLoss.AutoSave = false;
            ShowDataModelValues.AutoSave = false;
            ShowFullPaths.AutoSave = false;
            FocusSelectedLayer.AutoSave = false;
            AlwaysApplyDataBindings.AutoSave = false;

            _profileEditorService.SelectedProfileChanged -= ProfileEditorServiceOnSelectedProfileChanged;
            _profileEditorService.SelectedProfileElementChanged -= ProfileEditorServiceOnSelectedProfileElementChanged;
            _eventAggregator.Unsubscribe(this);
            SaveWorkspaceSettings();

            _profileEditorService.SuspendEditing = false;
            _profileEditorService.ChangeSelectedProfileConfiguration(null);

            base.OnClose();
        }

        #endregion

        private void ProfileEditorServiceOnSelectedProfileChanged(object sender, ProfileConfigurationEventArgs e)
        {
            NotifyOfPropertyChange(nameof(ProfileConfiguration));
        }

        private void ProfileEditorServiceOnSelectedProfileElementChanged(object sender, RenderProfileElementEventArgs e)
        {
            NotifyOfPropertyChange(nameof(HasSelectedElement));
        }

        private void SaveWorkspaceSettings()
        {
            SidePanelsWidth.Save();
            DataModelConditionsHeight.Save();
            BottomPanelsHeight.Save();
            ElementPropertiesWidth.Save();
        }

        #region Menu

        public bool HasSelectedElement => _profileEditorService.SelectedProfileElement != null;
        public bool CanPaste => _profileEditorService.GetCanPasteProfileElement();

        public async Task ViewProperties()
        {
            await _sidebarVmFactory.SidebarProfileConfigurationViewModel(_profileEditorService.SelectedProfileConfiguration).ViewProperties();
        }

        public void ViewScripts()
        {
            _windowManager.ShowWindow(_scriptVmFactory.ScriptsDialogViewModel(ProfileConfiguration.Profile));
        }

        public async Task AdaptProfile()
        {
            if (_profileEditorService.SelectedProfileConfiguration?.Profile == null)
                return;

            if (!await DialogService.ShowConfirmDialog(
                "Adapt profile",
                "Are you sure you want to adapt the profile to your current surface? Layer assignments may change."
            ))
                return;

            _profileService.AdaptProfile(_profileEditorService.SelectedProfileConfiguration.Profile);
        }

        public void DuplicateProfile()
        {
            ProfileConfigurationExportModel export = _profileService.ExportProfile(ProfileConfiguration);
            _profileService.ImportProfile(ProfileConfiguration.Category, export, true, false, "copy");
        }

        public async Task DeleteProfile()
        {
            await _sidebarVmFactory.SidebarProfileConfigurationViewModel(_profileEditorService.SelectedProfileConfiguration).Delete();
        }

        public async Task ExportProfile()
        {
            await _sidebarVmFactory.SidebarProfileConfigurationViewModel(_profileEditorService.SelectedProfileConfiguration).Export();
        }

        public void Copy()
        {
            if (_profileEditorService.SelectedProfileElement != null)
                _profileEditorService.CopyProfileElement(_profileEditorService.SelectedProfileElement);
        }

        public void Duplicate()
        {
            if (_profileEditorService.SelectedProfileElement != null)
                _profileEditorService.DuplicateProfileElement(_profileEditorService.SelectedProfileElement);
        }

        public void Paste()
        {
            if (_profileEditorService.SelectedProfileElement != null && _profileEditorService.SelectedProfileElement.Parent is Folder parent)
            {
                _profileEditorService.PasteProfileElement(parent, _profileEditorService.SelectedProfileElement.Order - 1);
            }
            else
            {
                Folder rootFolder = _profileEditorService.SelectedProfile?.GetRootFolder();
                if (rootFolder != null)
                    _profileEditorService.PasteProfileElement(rootFolder, rootFolder.Children.Count);
            }
        }
        
        public void OpenDebugger()
        {
            _debugService.ShowDebugger();
        }

        public void OpenUrl(string url)
        {
            Core.Utilities.OpenUrl(url);
        }

        public void EditMenuOpened()
        {
            NotifyOfPropertyChange(nameof(CanPaste));
        }

        #endregion

        #region Implementation of IHandle<in MainWindowFocusChangedEvent>

        /// <inheritdoc />
        public void Handle(MainWindowFocusChangedEvent message)
        {
            if (!StopOnFocusLoss.Value || SuspendedManually)
                return;

            _profileEditorService.SuspendEditing = !message.IsFocused;
        }

        #endregion
    }
}