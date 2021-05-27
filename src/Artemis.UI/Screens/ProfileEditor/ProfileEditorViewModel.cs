using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Artemis.Core;
using Artemis.Core.Modules;
using Artemis.Core.Services;
using Artemis.UI.Extensions;
using Artemis.UI.Screens.ProfileEditor.Dialogs;
using Artemis.UI.Screens.ProfileEditor.DisplayConditions;
using Artemis.UI.Screens.ProfileEditor.LayerProperties;
using Artemis.UI.Screens.ProfileEditor.ProfileTree;
using Artemis.UI.Screens.ProfileEditor.Visualization;
using Artemis.UI.Shared.Services;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor
{
    public class ProfileEditorViewModel : Screen
    {
        private readonly IMessageService _messageService;
        private readonly IProfileEditorService _profileEditorService;
        private readonly IProfileService _profileService;
        private readonly ISettingsService _settingsService;
        private PluginSetting<GridLength> _bottomPanelsHeight;
        private PluginSetting<GridLength> _dataModelConditionsHeight;
        private DisplayConditionsViewModel _displayConditionsViewModel;
        private PluginSetting<GridLength> _elementPropertiesWidth;
        private LayerPropertiesViewModel _layerPropertiesViewModel;
        private ProfileTreeViewModel _profileTreeViewModel;
        private ProfileViewModel _profileViewModel;
        private PluginSetting<GridLength> _sidePanelsWidth;

        public ProfileEditorViewModel(ProfileViewModel profileViewModel,
            ProfileTreeViewModel profileTreeViewModel,
            DisplayConditionsViewModel dataModelConditionsViewModel,
            LayerPropertiesViewModel layerPropertiesViewModel,
            IProfileEditorService profileEditorService,
            IProfileService profileService,
            IDialogService dialogService,
            ISettingsService settingsService,
            IMessageService messageService)
        {
            _profileEditorService = profileEditorService;
            _profileService = profileService;
            _settingsService = settingsService;
            _messageService = messageService;

            DisplayName = "PROFILE EDITOR";
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

        public PluginSetting<GridLength> SidePanelsWidth
        {
            get => _sidePanelsWidth;
            set => SetAndNotify(ref _sidePanelsWidth, value);
        }

        public PluginSetting<GridLength> DataModelConditionsHeight
        {
            get => _dataModelConditionsHeight;
            set => SetAndNotify(ref _dataModelConditionsHeight, value);
        }

        public PluginSetting<GridLength> BottomPanelsHeight
        {
            get => _bottomPanelsHeight;
            set => SetAndNotify(ref _bottomPanelsHeight, value);
        }

        public PluginSetting<GridLength> ElementPropertiesWidth
        {
            get => _elementPropertiesWidth;
            set => SetAndNotify(ref _elementPropertiesWidth, value);
        }

        public async Task AdaptActiveProfile()
        {
            if (_profileEditorService.SelectedProfile == null)
                return;

            if (!await DialogService.ShowConfirmDialog(
                "Adapt profile",
                "Are you sure you want to adapt the profile to your current surface? Layer assignments may change."
            ))
                return;

            _profileService.AdaptProfile(_profileEditorService.SelectedProfile);
        }

        public void Undo()
        {
            // Expanded status is also undone because undoing works a bit crude, that's annoying
            List<LayerPropertyGroupViewModel> beforeGroups = LayerPropertiesViewModel.GetAllLayerPropertyGroupViewModels();
            List<string> expandedPaths = beforeGroups.Where(g => g.IsExpanded).Select(g => g.LayerPropertyGroup.Path).ToList();
            // Store the focused element so we can restore it later
            IInputElement focusedElement = FocusManager.GetFocusedElement(Window.GetWindow(View));

            if (!_profileEditorService.UndoUpdateProfile())
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

            if (!_profileEditorService.RedoUpdateProfile())
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

        protected override void OnInitialActivate()
        {
            LoadWorkspaceSettings();
            base.OnInitialActivate();
        }

        protected override void OnClose()
        {
            SaveWorkspaceSettings();
            base.OnClose();
        }

        private void LoadWorkspaceSettings()
        {
            SidePanelsWidth = _settingsService.GetSetting("ProfileEditor.SidePanelsWidth", new GridLength(385));
            DataModelConditionsHeight = _settingsService.GetSetting("ProfileEditor.DataModelConditionsHeight", new GridLength(345));
            BottomPanelsHeight = _settingsService.GetSetting("ProfileEditor.BottomPanelsHeight", new GridLength(265));
            ElementPropertiesWidth = _settingsService.GetSetting("ProfileEditor.ElementPropertiesWidth", new GridLength(545));
        }

        private void SaveWorkspaceSettings()
        {
            SidePanelsWidth.Save();
            DataModelConditionsHeight.Save();
            BottomPanelsHeight.Save();
            ElementPropertiesWidth.Save();
        }
    }
}