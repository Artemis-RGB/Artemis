using Artemis.Core;
using Artemis.UI.Avalonia.Screens.Root.ViewModels;

namespace Artemis.UI.Avalonia.Ninject.Factories
{
    public interface IVmFactory
    {
    }

    public interface ISidebarVmFactory : IVmFactory
    {
        SidebarCategoryViewModel SidebarCategoryViewModel(ProfileCategory profileCategory);
        SidebarProfileConfigurationViewModel SidebarProfileConfigurationViewModel(ProfileConfiguration profileConfiguration);
    }
}