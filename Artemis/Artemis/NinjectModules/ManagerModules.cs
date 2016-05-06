using Artemis.Managers;
using Ninject.Modules;

namespace Artemis.NinjectModules
{
    internal class ManagerModules : NinjectModule
    {
        public override void Load()
        {
            Bind<MainManager>().ToSelf().InSingletonScope();
            Bind<KeyboardManager>().ToSelf().InSingletonScope();
            Bind<EffectManager>().ToSelf().InSingletonScope();
        }
    }
}