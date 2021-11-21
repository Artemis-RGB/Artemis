using Artemis.Core.Services;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.Root.Sidebar;
using Artemis.UI.Services.Interfaces;
using Artemis.UI.Shared;
using ReactiveUI;

namespace Artemis.UI.Screens.Root
{
    public class RootViewModel : ActivatableViewModelBase, IScreen
    {
        private readonly ICoreService _coreService;

        public RootViewModel(ICoreService coreService, IRegistrationService registrationService, ISidebarVmFactory sidebarVmFactory)
        {
            Router = new RoutingState();
            SidebarViewModel = sidebarVmFactory.SidebarViewModel(this);

            _coreService = coreService;
            _coreService.Initialize();

            registrationService.RegisterProviders();
        }

        public SidebarViewModel SidebarViewModel { get; }

        /// <inheritdoc />
        public RoutingState Router { get; }
    }
}