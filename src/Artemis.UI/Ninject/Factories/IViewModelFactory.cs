using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Surface;
using Artemis.Core.Plugins.Abstract;
using Artemis.UI.Screens.Module;
using Artemis.UI.Screens.Module.ProfileEditor;
using Artemis.UI.Screens.Module.ProfileEditor.ProfileTree.TreeItem;
using Artemis.UI.Screens.Settings.Tabs.Devices;

namespace Artemis.UI.Ninject.Factories
{
    public interface IViewModelFactory
    {
    }

    public interface IModuleViewModelFactory : IViewModelFactory
    {
        ModuleRootViewModel Create(Module module);
    }

    public interface IDeviceSettingsViewModelFactory : IViewModelFactory
    {
        DeviceSettingsViewModel Create(ArtemisDevice device);
    }

    public interface IProfileEditorViewModelFactory : IViewModelFactory
    {
        ProfileEditorViewModel Create(ProfileModule module);
    }

    public interface IFolderViewModelFactory : IViewModelFactory
    {
        FolderViewModel Create(ProfileElement folder);
        FolderViewModel Create(TreeItemViewModel parent, ProfileElement folder);
    }

    public interface ILayerViewModelFactory : IViewModelFactory
    {
        LayerViewModel Create(TreeItemViewModel parent, ProfileElement folder);
    }
}