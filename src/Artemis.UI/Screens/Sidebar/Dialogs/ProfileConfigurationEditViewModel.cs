using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Modules;
using Artemis.Core.Services;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.VisualScripting;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.ProfileEditor;
using Avalonia.Media.Imaging;
using Avalonia.Svg.Skia;
using Avalonia.Threading;
using Material.Icons;
using Newtonsoft.Json;
using ReactiveUI;

namespace Artemis.UI.Screens.Sidebar
{
    public class ProfileConfigurationEditViewModel : DialogViewModelBase<ProfileConfiguration?>
    {
        private readonly ProfileCategory _profileCategory;
        private readonly IProfileService _profileService;
        private readonly IProfileEditorService _profileEditorService;
        private readonly IWindowService _windowService;
        private Hotkey? _disableHotkey;
        private Hotkey? _enableHotkey;
        private ProfileConfigurationHotkeyMode _hotkeyMode;
        private ProfileConfigurationIconType _iconType;
        private ObservableCollection<ProfileIconViewModel>? _materialIcons;
        private ProfileConfiguration _profileConfiguration;
        private string _profileName;
        private Bitmap? _selectedBitmapSource;
        private string? _selectedIconPath;
        private ProfileIconViewModel? _selectedMaterialIcon;
        private ProfileModuleViewModel? _selectedModule;
        private SvgImage? _selectedSvgSource;
        private readonly ObservableAsPropertyHelper<ModuleActivationRequirementsViewModel?> _moduleActivationRequirementsViewModel;

        public ProfileConfigurationEditViewModel(
            ProfileCategory profileCategory,
            ProfileConfiguration? profileConfiguration,
            IWindowService windowService,
            IProfileService profileService,
            IProfileEditorService profileEditorService,
            IPluginManagementService pluginManagementService,
            INodeVmFactory nodeVmFactory)
        {
            _profileCategory = profileCategory;
            _windowService = windowService;
            _profileService = profileService;
            _profileEditorService = profileEditorService;
            _profileConfiguration = profileConfiguration ?? profileService.CreateProfileConfiguration(profileCategory, "New profile", Enum.GetValues<MaterialIconKind>().First().ToString());
            _profileName = _profileConfiguration.Name;
            _iconType = _profileConfiguration.Icon.IconType;
            _hotkeyMode = _profileConfiguration.HotkeyMode;
            if (_profileConfiguration.EnableHotkey != null)
                _enableHotkey = new Hotkey {Key = _profileConfiguration.EnableHotkey.Key, Modifiers = _profileConfiguration.EnableHotkey.Modifiers};
            if (_profileConfiguration.DisableHotkey != null)
                _disableHotkey = new Hotkey {Key = _profileConfiguration.DisableHotkey.Key, Modifiers = _profileConfiguration.DisableHotkey.Modifiers};

            IsNew = profileConfiguration == null;
            DisplayName = IsNew ? "Artemis | Add profile" : "Artemis | Edit profile";
            Modules = new ObservableCollection<ProfileModuleViewModel?>(
                pluginManagementService.GetFeaturesOfType<Module>().Where(m => !m.IsAlwaysAvailable).Select(m => new ProfileModuleViewModel(m))
            );
            Modules.Insert(0, null);
            
            VisualEditorViewModel = nodeVmFactory.NodeScriptViewModel(_profileConfiguration.ActivationCondition, true);

            BrowseBitmapFile = ReactiveCommand.CreateFromTask(ExecuteBrowseBitmapFile);
            BrowseSvgFile = ReactiveCommand.CreateFromTask(ExecuteBrowseSvgFile);
            OpenConditionEditor = ReactiveCommand.CreateFromTask(ExecuteOpenConditionEditor);
            Confirm = ReactiveCommand.CreateFromTask(ExecuteConfirm);
            Import = ReactiveCommand.CreateFromTask(ExecuteImport);
            Delete = ReactiveCommand.CreateFromTask(ExecuteDelete);
            Cancel = ReactiveCommand.Create(ExecuteCancel);

            _moduleActivationRequirementsViewModel = this.WhenAnyValue(vm => vm.SelectedModule)
                .Select(m => m != null ? new ModuleActivationRequirementsViewModel(m.Module) : null)
                .ToProperty(this, vm => vm.ModuleActivationRequirementsViewModel);

            Dispatcher.UIThread.Post(LoadIcon, DispatcherPriority.Background);
        }

        public bool IsNew { get; }

        public ProfileConfiguration ProfileConfiguration
        {
            get => _profileConfiguration;
            set => RaiseAndSetIfChanged(ref _profileConfiguration, value);
        }

        public string ProfileName
        {
            get => _profileName;
            set => RaiseAndSetIfChanged(ref _profileName, value);
        }

        public ProfileConfigurationHotkeyMode HotkeyMode
        {
            get => _hotkeyMode;
            set => RaiseAndSetIfChanged(ref _hotkeyMode, value);
        }

        public Hotkey? EnableHotkey
        {
            get => _enableHotkey;
            set => RaiseAndSetIfChanged(ref _enableHotkey, value);
        }

        public Hotkey? DisableHotkey
        {
            get => _disableHotkey;
            set => RaiseAndSetIfChanged(ref _disableHotkey, value);
        }

        public ObservableCollection<ProfileModuleViewModel?> Modules { get; }

        public ProfileModuleViewModel? SelectedModule
        {
            get => _selectedModule;
            set => RaiseAndSetIfChanged(ref _selectedModule, value);
        }

        public NodeScriptViewModel VisualEditorViewModel { get; }
        public ModuleActivationRequirementsViewModel? ModuleActivationRequirementsViewModel => _moduleActivationRequirementsViewModel.Value;

        public ReactiveCommand<Unit, Unit> OpenConditionEditor { get; }
        public ReactiveCommand<Unit, Unit> BrowseBitmapFile { get; }
        public ReactiveCommand<Unit, Unit> BrowseSvgFile { get; }
        public ReactiveCommand<Unit, Unit> Confirm { get; }
        public ReactiveCommand<Unit, Unit> Import { get; }
        public ReactiveCommand<Unit, Unit> Delete { get; }
        public new ReactiveCommand<Unit, Unit> Cancel { get; }

        private async Task ExecuteImport()
        {
            if (!IsNew)
                return;

            string[]? result = await _windowService.CreateOpenFileDialog()
                .HavingFilter(f => f.WithExtension("json").WithName("Artemis profile"))
                .ShowAsync();

            if (result == null)
                return;

            string json = await File.ReadAllTextAsync(result[0]);
            ProfileConfigurationExportModel? profileConfigurationExportModel = null;
            try
            {
                profileConfigurationExportModel = JsonConvert.DeserializeObject<ProfileConfigurationExportModel>(json, IProfileService.ExportSettings);
            }
            catch (JsonException e)
            {
                _windowService.ShowExceptionDialog("Import profile failed", e);
            }

            if (profileConfigurationExportModel == null)
            {
                await _windowService.ShowConfirmContentDialog("Import profile", "Failed to import this profile, make sure it is a valid Artemis profile.", "Confirm", null);
                return;
            }

            // Remove the temporary profile configuration
            _profileService.RemoveProfileConfiguration(_profileConfiguration);
            // Import the new profile configuration
            _profileService.ImportProfile(_profileCategory, profileConfigurationExportModel);

            Close(_profileConfiguration);
        }

        private async Task ExecuteDelete()
        {
            if (IsNew)
                return;
            if (!await _windowService.ShowConfirmContentDialog("Delete profile", "Are you sure you want to permanently delete this profile?"))
                return;

            if (_profileConfiguration.IsBeingEdited)
                _profileEditorService.ChangeCurrentProfileConfiguration(null);
            _profileService.RemoveProfileConfiguration(_profileConfiguration);
            Close(_profileConfiguration);
        }

        private async Task ExecuteConfirm()
        {
            ProfileConfiguration.Name = ProfileName;
            ProfileConfiguration.Module = SelectedModule?.Module;
            ProfileConfiguration.HotkeyMode = HotkeyMode;
            ProfileConfiguration.EnableHotkey = EnableHotkey;
            ProfileConfiguration.DisableHotkey = DisableHotkey;

            await SaveIcon();

            _profileService.SaveProfileConfigurationIcon(ProfileConfiguration);
            _profileService.SaveProfileCategory(_profileCategory);
            Close(ProfileConfiguration);
        }

        private void ExecuteCancel()
        {
            if (IsNew)
                _profileService.RemoveProfileConfiguration(_profileConfiguration);
            Close(null);
        }

        #region Icon

        public ProfileConfigurationIconType IconType
        {
            get => _iconType;
            set => RaiseAndSetIfChanged(ref _iconType, value);
        }

        public ObservableCollection<ProfileIconViewModel>? MaterialIcons
        {
            get => _materialIcons;
            set => RaiseAndSetIfChanged(ref _materialIcons, value);
        }

        public ProfileIconViewModel? SelectedMaterialIcon
        {
            get => _selectedMaterialIcon;
            set => RaiseAndSetIfChanged(ref _selectedMaterialIcon, value);
        }

        public Bitmap? SelectedBitmapSource
        {
            get => _selectedBitmapSource;
            set => RaiseAndSetIfChanged(ref _selectedBitmapSource, value);
        }

        public SvgImage? SelectedSvgSource
        {
            get => _selectedSvgSource;
            set => RaiseAndSetIfChanged(ref _selectedSvgSource, value);
        }

        private void LoadIcon()
        {
            // Preselect the icon based on streams if needed
            if (_profileConfiguration.Icon.IconType == ProfileConfigurationIconType.BitmapImage)
            {
                SelectedBitmapSource = new Bitmap(_profileConfiguration.Icon.GetIconStream());
            }
            else if (_profileConfiguration.Icon.IconType == ProfileConfigurationIconType.SvgImage)
            {
                Stream? iconStream = _profileConfiguration.Icon.GetIconStream();
                if (iconStream != null)
                {
                    SvgSource newSource = new();
                    newSource.Load(iconStream);
                    SelectedSvgSource = new SvgImage {Source = newSource};
                }
            }

            // Prepare the contents of the dropdown box, it should be virtualized so no need to wait with this
            ObservableCollection<ProfileIconViewModel> icons = new(Enum.GetValues<MaterialIconKind>()
                .Select(kind => new ProfileIconViewModel(kind))
                .DistinctBy(vm => vm.DisplayName)
                .OrderBy(vm => vm.DisplayName));

            // Preselect the icon or fall back to a random one
            SelectedMaterialIcon = !IsNew && Enum.TryParse(_profileConfiguration.Icon.IconName, out MaterialIconKind enumValue)
                ? icons.FirstOrDefault(m => m.Icon == enumValue)
                : icons.ElementAt(new Random().Next(0, icons.Count - 1));
            MaterialIcons = icons;
        }

        private async Task SaveIcon()
        {
            if (IconType == ProfileConfigurationIconType.MaterialIcon && SelectedMaterialIcon != null)
            {
                ProfileConfiguration.Icon.SetIconByName(SelectedMaterialIcon.Icon.ToString());
            }
            else if (_selectedIconPath != null)
            {
                await using FileStream fileStream = File.OpenRead(_selectedIconPath);
                ProfileConfiguration.Icon.SetIconByStream(Path.GetFileName(_selectedIconPath), fileStream);
            }
        }

        private async Task ExecuteBrowseBitmapFile()
        {
            string[]? result = await _windowService.CreateOpenFileDialog()
                .HavingFilter(f => f.WithExtension("png").WithExtension("jpg").WithExtension("bmp").WithName("Bitmap image"))
                .ShowAsync();

            if (result == null)
                return;

            SelectedBitmapSource = new Bitmap(result[0]);
            _selectedIconPath = result[0];
        }

        private async Task ExecuteBrowseSvgFile()
        {
            string[]? result = await _windowService.CreateOpenFileDialog()
                .HavingFilter(f => f.WithExtension("svg").WithName("SVG image"))
                .ShowAsync();

            if (result == null)
                return;

            SvgSource newSource = new();
            newSource.Load(result[0]);

            SelectedSvgSource = new SvgImage {Source = newSource};
            _selectedIconPath = result[0];
        }

        private async Task ExecuteOpenConditionEditor()
        {
            await _windowService.ShowDialogAsync<NodeScriptWindowViewModel, bool>(("nodeScript", ProfileConfiguration.ActivationCondition));
        }

        #endregion
    }
}