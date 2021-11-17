using Artemis.Core.Services;
using Artemis.UI.Avalonia.Ninject.Factories;
using Artemis.UI.Avalonia.Services.Interfaces;
using Artemis.UI.Avalonia.Shared;
using ReactiveUI;

namespace Artemis.UI.Avalonia.Screens.Root.ViewModels
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