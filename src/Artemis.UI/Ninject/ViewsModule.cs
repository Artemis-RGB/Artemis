using Artemis.UI.ViewModels;
using Ninject.Modules;

namespace Artemis.UI.Ninject
{
    public class ViewsModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IMainViewModel>().To<MainViewModel>();
        }
    }
}