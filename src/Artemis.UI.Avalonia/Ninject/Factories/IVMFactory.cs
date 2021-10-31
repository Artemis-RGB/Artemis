using Artemis.Core;
using Artemis.UI.Avalonia.Screens.Device.ViewModels;
using Artemis.UI.Avalonia.Screens.Root.ViewModels;
using Artemis.UI.Avalonia.Screens.SurfaceEditor.ViewModels;
using ReactiveUI;

namespace Artemis.UI.Avalonia.Ninject.Factories
{
    public interface IVmFactory
    {
    }

    public interface IDeviceVmFactory : IVmFactory
    {
        DevicePropertiesViewModel DevicePropertiesViewModel(ArtemisDevice device);
    }

    public interface ISidebarVmFactory : IVmFactory
    {
        SidebarViewModel SidebarViewModel(IScreen hostScreen);
        SidebarCategoryViewModel SidebarCategoryViewModel(ProfileCategory profileCategory);
        SidebarProfileConfigurationViewModel SidebarProfileConfigurationViewModel(ProfileConfiguration profileConfiguration);
    }

    public interface SurfaceVmFactory : IVmFactory
    {
        SurfaceDeviceViewModel SurfaceDeviceViewModel(ArtemisDevice device);
        ListDeviceViewModel ListDeviceViewModel(ArtemisDevice device);
    }
}