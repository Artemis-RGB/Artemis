using System.Collections.ObjectModel;
using Artemis.Core;
using Artemis.Core.LayerBrushes;
using Artemis.Core.LayerEffects;
using Artemis.UI.Screens.Device;
using Artemis.UI.Screens.Plugins;
using Artemis.UI.Screens.ProfileEditor;
using Artemis.UI.Screens.ProfileEditor.ProfileElementProperties;
using Artemis.UI.Screens.ProfileEditor.ProfileElementProperties.Timeline;
using Artemis.UI.Screens.ProfileEditor.ProfileElementProperties.Tree;
using Artemis.UI.Screens.ProfileEditor.ProfileTree;
using Artemis.UI.Screens.Settings;
using Artemis.UI.Screens.Sidebar;
using Artemis.UI.Screens.SurfaceEditor;
using Artemis.UI.Services;
using DynamicData.Binding;
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

    public interface ILayerPropertyVmFactory : IVmFactory
    {
        ProfileElementPropertyViewModel ProfileElementPropertyViewModel(ILayerProperty layerProperty);
        ProfileElementPropertyGroupViewModel ProfileElementPropertyGroupViewModel(LayerPropertyGroup layerPropertyGroup);
        ProfileElementPropertyGroupViewModel ProfileElementPropertyGroupViewModel(LayerPropertyGroup layerPropertyGroup, BaseLayerBrush layerBrush);
        ProfileElementPropertyGroupViewModel ProfileElementPropertyGroupViewModel(LayerPropertyGroup layerPropertyGroup, BaseLayerEffect layerEffect);

        TreeGroupViewModel TreeGroupViewModel(ProfileElementPropertyGroupViewModel profileElementPropertyGroupViewModel);
        // TimelineGroupViewModel TimelineGroupViewModel(ProfileElementPropertiesViewModel profileElementPropertiesViewModel);

        // TreeViewModel TreeViewModel(ProfileElementPropertiesViewModel profileElementPropertiesViewModel, IObservableCollection<ProfileElementPropertyGroupViewModel> profileElementPropertyGroups);
        // EffectsViewModel EffectsViewModel(ProfileElementPropertiesViewModel profileElementPropertiesViewModel);
        // TimelineViewModel TimelineViewModel(ProfileElementPropertiesViewModel profileElementPropertiesViewModel, IObservableCollection<ProfileElementPropertyGroupViewModel> profileElementPropertyGroups);
        // TimelineSegmentViewModel TimelineSegmentViewModel(SegmentViewModelType segment, IObservableCollection<ProfileElementPropertyGroupViewModel> profileElementPropertyGroups);
    }

    public interface IPropertyVmFactory
    {
        ITreePropertyViewModel TreePropertyViewModel(ILayerProperty layerProperty, ProfileElementPropertyViewModel profileElementPropertyViewModel);
        ITimelinePropertyViewModel TimelinePropertyViewModel(ILayerProperty layerProperty, ProfileElementPropertyViewModel profileElementPropertyViewModel);
    }
}