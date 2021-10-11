using Artemis.Core;
using Artemis.UI.Avalonia.Screens.Root.ViewModels;
using ReactiveUI;

namespace Artemis.UI.Avalonia.Ninject.Factories
{
    public interface IVmFactory
    {
    }

    public interface ISidebarVmFactory : IVmFactory
    {
        SidebarViewModel SidebarViewModel(IScreen hostScreen);
        SidebarCategoryViewModel SidebarCategoryViewModel(ProfileCategory profileCategory);
        SidebarProfileConfigurationViewModel SidebarProfileConfigurationViewModel(ProfileConfiguration profileConfiguration);
    }
}