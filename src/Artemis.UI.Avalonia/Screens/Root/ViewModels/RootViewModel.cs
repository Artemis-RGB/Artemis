using System;
using System.Reactive;
using Artemis.Core.Services;
using ReactiveUI;

namespace Artemis.UI.Avalonia.Screens.Root.ViewModels
{
    public class RootViewModel : ViewModelBase, IScreen, IActivatableViewModel
    {
        private readonly ICoreService _coreService;

        public RootViewModel(ICoreService coreService, SidebarViewModel sidebarViewModel)
        {
            Router = new RoutingState();
            SidebarViewModel = sidebarViewModel;
            SidebarViewModel.Router = Router;

            _coreService = coreService;
            _coreService.Initialize();
        }

        public SidebarViewModel SidebarViewModel { get; }

        /// <inheritdoc />
        public ViewModelActivator Activator { get; } = new();

        /// <inheritdoc />
        public RoutingState Router { get; }
    }
}