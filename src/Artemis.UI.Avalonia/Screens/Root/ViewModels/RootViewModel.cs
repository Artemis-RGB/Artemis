using Artemis.Core.Services;
using Artemis.UI.Avalonia.Ninject.Factories;
using ReactiveUI;

namespace Artemis.UI.Avalonia.Screens.Root.ViewModels
{
    public class RootViewModel : ActivatableViewModelBase, IScreen
    {
        private readonly ICoreService _coreService;

        public RootViewModel(ICoreService coreService, ISidebarVmFactory sidebarVmFactory)
        {
            Router = new RoutingState();
            SidebarViewModel = sidebarVmFactory.SidebarViewModel(this);

            _coreService = coreService;
            _coreService.Initialize();
        }

        public SidebarViewModel SidebarViewModel { get; }

        /// <inheritdoc />
        public RoutingState Router { get; }
    }
}