using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Profile.LayerProperties.Attributes;
using Artemis.Core.Models.Surface;
using Artemis.Core.Plugins.Abstract;
using Artemis.UI.Screens.Module;
using Artemis.UI.Screens.Module.ProfileEditor;
using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties;
using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Abstract;
using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.LayerEffects;
using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Timeline;
using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Tree;
using Artemis.UI.Screens.Module.ProfileEditor.ProfileTree.TreeItem;
using Artemis.UI.Screens.Module.ProfileEditor.Visualization;
using Artemis.UI.Screens.Settings.Tabs.Devices;
using Stylet;

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
        LayerPropertyGroupViewModel LayerPropertyGroupViewModel(LayerPropertyGroup layerPropertyGroup, PropertyGroupDescriptionAttribute propertyGroupDescription);
        TreeViewModel TreeViewModel(LayerPropertiesViewModel layerPropertiesViewModel, BindableCollection<LayerPropertyGroupViewModel> layerPropertyGroups);
        EffectsViewModel EffectsViewModel(LayerPropertiesViewModel layerPropertiesViewModel);
        TimelineViewModel TimelineViewModel(LayerPropertiesViewModel layerPropertiesViewModel, BindableCollection<LayerPropertyGroupViewModel> layerPropertyGroups);
        TreePropertyGroupViewModel TreePropertyGroupViewModel(LayerPropertyBaseViewModel layerPropertyBaseViewModel);
        TimelinePropertyGroupViewModel TimelinePropertyGroupViewModel(LayerPropertyBaseViewModel layerPropertyBaseViewModel);
    }
}