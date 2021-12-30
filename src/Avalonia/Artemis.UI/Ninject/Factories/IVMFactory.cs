using System.Collections.ObjectModel;
using Artemis.Core;
using Artemis.UI.Screens.Device;
using Artemis.UI.Screens.Plugins;
using Artemis.UI.Screens.ProfileEditor;
using Artemis.UI.Screens.ProfileEditor.ProfileTree;
using Artemis.UI.Screens.Settings;
using Artemis.UI.Screens.Sidebar;
using Artemis.UI.Screens.SurfaceEditor;
using Artemis.UI.Services;
using ReactiveUI;

namespace Artemis.UI.Ninject.Factories
{
    public interface IVmFactory
    {
    }

    public interface IDeviceVmFactory : IVmFactory
    {
        DevicePropertiesViewModel DevicePropertiesViewModel(ArtemisDevice device);
        DeviceSettingsViewModel DeviceSettingsViewModel(ArtemisDevice device, DevicesTabViewModel devicesTabViewModel);
        DeviceDetectInputViewModel DeviceDetectInputViewModel(ArtemisDevice device);
        DevicePropertiesTabViewModel DevicePropertiesTabViewModel(ArtemisDevice device);
        DeviceInfoTabViewModel DeviceInfoTabViewModel(ArtemisDevice device);
        DeviceLedsTabViewModel DeviceLedsTabViewModel(ArtemisDevice device, ObservableCollection<ArtemisLed> selectedLeds);
        InputMappingsTabViewModel InputMappingsTabViewModel(ArtemisDevice device, ObservableCollection<ArtemisLed> selectedLeds);
    }

    public interface ISettingsVmFactory : IVmFactory
    {
        PluginSettingsViewModel CreatePluginSettingsViewModel(Plugin plugin);
        PluginFeatureViewModel CreatePluginFeatureViewModel(PluginFeatureInfo pluginFeatureInfo, bool showShield);
        // DeviceSettingsViewModel CreateDeviceSettingsViewModel(ArtemisDevice device);
    }

    public interface ISidebarVmFactory : IVmFactory
    {
        SidebarViewModel? SidebarViewModel(IScreen hostScreen);
        SidebarCategoryViewModel SidebarCategoryViewModel(SidebarViewModel sidebarViewModel, ProfileCategory profileCategory);
        SidebarProfileConfigurationViewModel SidebarProfileConfigurationViewModel(SidebarViewModel sidebarViewModel, ProfileConfiguration profileConfiguration);
    }

    public interface SurfaceVmFactory : IVmFactory
    {
        SurfaceDeviceViewModel SurfaceDeviceViewModel(ArtemisDevice device);
        ListDeviceViewModel ListDeviceViewModel(ArtemisDevice device);
    }

    public interface IPrerequisitesVmFactory : IVmFactory
    {
        PluginPrerequisiteViewModel PluginPrerequisiteViewModel(PluginPrerequisite pluginPrerequisite, bool uninstall);
    }

    public interface IProfileEditorVmFactory : IVmFactory
    {
        ProfileEditorViewModel ProfileEditorViewModel(IScreen hostScreen);
        FolderTreeItemViewModel FolderTreeItemViewModel(TreeItemViewModel? parent, Folder folder);
        LayerTreeItemViewModel LayerTreeItemViewModel(TreeItemViewModel? parent, Layer layer);
    }
}