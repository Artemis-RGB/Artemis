using System;
using System.Linq;
using Artemis.UI.Ninject;
using Artemis.UI.ViewModels;
using Ninject;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;

namespace Artemis.UI
{
    public class AppBootstrapper : ReactiveObject, IScreen
    {
        public AppBootstrapper()
        {
            Router = new RoutingState();
            Kernel = new StandardKernel(new ViewsModule());

            // Configure the Ninject kernel and set it up as the default for ReactiveUI
            SetupNinject(Kernel);

            // Update the title on view change
            this.WhenAnyValue(x => x.Router.CurrentViewModel)
                .Subscribe(o => o.Subscribe(vm => { ViewTitle = vm != null ? vm.UrlPathSegment : ""; }));

            // Setup the sidebar that lives throughout the app
            SidebarViewModel = Kernel.Get<ISidebarViewModel>();
            // Navigate to the opening page of the application
            Router.Navigate.Execute(Kernel.Get<IMainViewModel>());
        }

        public IKernel Kernel { get; set; }
        public ISidebarViewModel SidebarViewModel { get; }
        public RoutingState Router { get; }

        [Reactive]
        public string ViewTitle { get; set; }

        private void SetupNinject(IKernel kernel)
        {
            // Bind the main screen to IScreen
            kernel.Bind<IScreen>().ToConstant(this);

            // Set up NInject to do DI
            var customResolver = new FuncDependencyResolver(
                (service, contract) =>
                {
                    if (contract != null) return kernel.GetAll(service, contract);
                    var items = kernel.GetAll(service);
                    var list = items.ToList();
                    return list;
                },
                (factory, service, contract) =>
                {
                    var binding = kernel.Bind(service).ToMethod(_ => factory());
                    if (contract != null) binding.Named(contract);
                });
            Locator.Current = customResolver;
        }
    }
}