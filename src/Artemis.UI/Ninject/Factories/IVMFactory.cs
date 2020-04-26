using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Profile.LayerProperties;
using Artemis.Core.Models.Profile.LayerProperties.Abstract;
using Artemis.Core.Models.Surface;
using Artemis.Core.Plugins.Abstract;
using Artemis.UI.Screens.Module;
using Artemis.UI.Screens.Module.ProfileEditor;
using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties;
using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.PropertyTree;
using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Timeline;
using Artemis.UI.Screens.Module.ProfileEditor.ProfileTree.TreeItem;
using Artemis.UI.Screens.Module.ProfileEditor.Visualization;
using Artemis.UI.Screens.Settings.Tabs.Devices;

namespace Artemis.UI.Ninject.Factories
{
    public interface IVmFactory
    {
    }

    public interface IModuleVmFactory : IVmFactory
    {
        ModuleRootViewModel Create(Module module);
    }

    public interface IDeviceSettingsVmFactory : IVmFactory
    {
        DeviceSettingsViewModel Create(ArtemisDevice device);
    }

    public interface IProfileEditorVmFactory : IVmFactory
    {
        ProfileEditorViewModel Create(ProfileModule module);
    }

    public interface IFolderVmFactory : IVmFactory
    {
        FolderViewModel Create(ProfileElement folder);
        FolderViewModel Create(TreeItemViewModel parent, ProfileElement folder);
    }

    public interface ILayerVmFactory : IVmFactory
    {
        LayerViewModel Create(TreeItemViewModel parent, ProfileElement folder);
    }

    public interface IProfileLayerVmFactory : IVmFactory
    {
        ProfileLayerViewModel Create(Layer layer);
    }

    public interface ILayerPropertyVmFactory : IVmFactory
    {
        LayerPropertyViewModel Create(BaseLayerProperty layerProperty, LayerPropertyViewModel parent);
    }

    public interface IPropertyTreeVmFactory : IVmFactory
    {
        PropertyTreeViewModel Create(LayerPropertiesViewModel layerPropertiesViewModel);
    }

    public interface IPropertyTimelineVmFactory : IVmFactory
    {
        PropertyTimelineViewModel Create(LayerPropertiesViewModel layerPropertiesViewModel);
    }

    public interface IPropertyTrackVmFactory : IVmFactory
    {
        PropertyTrackViewModel Create(PropertyTimelineViewModel propertyTimelineViewModel, LayerPropertyViewModel layerPropertyViewModel);
    }

    public interface IPropertyTrackKeyframeVmFactory : IVmFactory
    {
        PropertyTrackKeyframeViewModel Create(PropertyTrackViewModel propertyTrackViewModel, BaseKeyframe keyframe);
    }
}