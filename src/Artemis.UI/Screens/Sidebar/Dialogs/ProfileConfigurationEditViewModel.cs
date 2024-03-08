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
using Artemis.UI.DryIoc.Factories;
using Artemis.UI.Extensions;
using Artemis.UI.Screens.VisualScripting;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.ProfileEditor;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Material.Icons;
using PropertyChanged.SourceGenerator;
using ReactiveUI;

namespace Artemis.UI.Screens.Sidebar;

public partial class ProfileConfigurationEditViewModel : DialogViewModelBase<ProfileConfiguration?>
{
    private readonly ObservableAsPropertyHelper<ModuleActivationRequirementsViewModel?> _moduleActivationRequirementsViewModel;
    private readonly ProfileCategory _profileCategory;
    private readonly IProfileEditorService _profileEditorService;
    private readonly IProfileService _profileService;
    private readonly IWindowService _windowService;
    private string? _selectedIconPath;
    [Notify] private ProfileConfigurationIconType _iconType;
    [Notify] private Bitmap? _selectedBitmapSource;
    [Notify] private ProfileIconViewModel? _selectedMaterialIcon;
    [Notify] private Hotkey? _disableHotkey;
    [Notify] private Hotkey? _enableHotkey;
    [Notify] private bool _fadeInAndOut;
    [Notify] private ProfileConfigurationHotkeyMode _hotkeyMode;
    [Notify] private ProfileConfiguration _profileConfiguration;
    [Notify] private string _profileName;
    [Notify] private ProfileModuleViewModel? _selectedModule;

    public ProfileConfigurationEditViewModel(
        ProfileCategory profileCategory,
        ProfileConfiguration profileConfiguration,
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

        _profileConfiguration = profileConfiguration == ProfileConfiguration.Empty
            ? profileService.CreateProfileConfiguration(profileCategory, "New profile", Enum.GetValues<MaterialIconKind>().First().ToString())
            : profileConfiguration;
        _profileName = _profileConfiguration.Name;
        _iconType = _profileConfiguration.Icon.IconType;
        _hotkeyMode = _profileConfiguration.HotkeyMode;
        _fadeInAndOut = _profileConfiguration.FadeInAndOut;
        if (_profileConfiguration.EnableHotkey != null)
            _enableHotkey = new Hotkey {Key = _profileConfiguration.EnableHotkey.Key, Modifiers = _profileConfiguration.EnableHotkey.Modifiers};
        if (_profileConfiguration.DisableHotkey != null)
            _disableHotkey = new Hotkey {Key = _profileConfiguration.DisableHotkey.Key, Modifiers = _profileConfiguration.DisableHotkey.Modifiers};

        IsNew = profileConfiguration == ProfileConfiguration.Empty;
        DisplayName = IsNew ? "Artemis | Add profile" : "Artemis | Edit profile properties";
        Modules = new ObservableCollection<ProfileModuleViewModel?>(pluginManagementService.GetFeaturesOfType<Module>().Where(m => !m.IsAlwaysAvailable).Select(m => new ProfileModuleViewModel(m)));
        Modules.Insert(0, null);
        _selectedModule = Modules.FirstOrDefault(m => m?.Module == _profileConfiguration.Module);

        VisualEditorViewModel = nodeVmFactory.NodeScriptViewModel(_profileConfiguration.ActivationCondition, true);

        BrowseBitmapFile = ReactiveCommand.CreateFromTask(ExecuteBrowseBitmapFile);
        OpenConditionEditor = ReactiveCommand.CreateFromTask(ExecuteOpenConditionEditor);
        Confirm = ReactiveCommand.CreateFromTask(ExecuteConfirm);
        Delete = ReactiveCommand.CreateFromTask(ExecuteDelete);
        Cancel = ReactiveCommand.Create(ExecuteCancel);

        _moduleActivationRequirementsViewModel = this.WhenAnyValue(vm => vm.SelectedModule)
            .Select(m => m != null ? new ModuleActivationRequirementsViewModel(m.Module) : null)
            .ToProperty(this, vm => vm.ModuleActivationRequirementsViewModel);

        Dispatcher.UIThread.Post(LoadIcon, DispatcherPriority.Background);
    }

    public bool IsNew { get; }
    public ObservableCollection<ProfileModuleViewModel?> Modules { get; }
    public NodeScriptViewModel VisualEditorViewModel { get; }
    public ModuleActivationRequirementsViewModel? ModuleActivationRequirementsViewModel => _moduleActivationRequirementsViewModel.Value;

    public ReactiveCommand<Unit, Unit> OpenConditionEditor { get; }
    public ReactiveCommand<Unit, Unit> BrowseBitmapFile { get; }
    public ReactiveCommand<Unit, Unit> Confirm { get; }
    public ReactiveCommand<Unit, Unit> Delete { get; }
    public ReactiveCommand<Unit, Unit> Cancel { get; }

    private async Task ExecuteDelete()
    {
        if (IsNew)
            return;
        if (!await _windowService.ShowConfirmContentDialog("Delete profile", "Are you sure you want to permanently delete this profile?"))
            return;

        if (_profileService.FocusProfile == _profileConfiguration)
            await _profileEditorService.ChangeCurrentProfileConfiguration(null);
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
        ProfileConfiguration.FadeInAndOut = FadeInAndOut;

        await SaveIcon();

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
    
    private void LoadIcon()
    {
        // Preselect the icon based on streams if needed
        if (_profileConfiguration.Icon.IconType == ProfileConfigurationIconType.BitmapImage)
        {
            try
            {
                Stream? iconStream = _profileConfiguration.Icon.IconBytes != null ? new MemoryStream(_profileConfiguration.Icon.IconBytes) : null;
                SelectedBitmapSource = iconStream != null ? new Bitmap(iconStream) : null;
            }
            catch (Exception e)
            {
                _windowService.ShowConfirmContentDialog("Failed to load profile icon", e.Message, "Meh", null);
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
            : icons.ElementAt(Random.Shared.Next(0, icons.Count - 1));
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
            ProfileConfiguration.Icon.SetIconByStream(fileStream);
        }
    }

    private async Task ExecuteBrowseBitmapFile()
    {
        string[]? result = await _windowService.CreateOpenFileDialog()
            .HavingFilter(f => f.WithBitmaps())
            .ShowAsync();

        if (result == null)
            return;

        SelectedBitmapSource = BitmapExtensions.LoadAndResize(result[0], 128);
        _selectedIconPath = result[0];
    }

    private async Task ExecuteOpenConditionEditor()
    {
        await _windowService.ShowDialogAsync<NodeScriptWindowViewModel, bool>(ProfileConfiguration.ActivationCondition);
    }

    #endregion
}