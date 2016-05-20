using Artemis.Managers;
using Ninject.Modules;

namespace Artemis.InjectionModules
{
    internal class ManagerModules : NinjectModule
    {
        public override void Load()
        {
            Bind<MainManager>().ToSelf().InSingletonScope();
            Bind<DeviceManager>().ToSelf().InSingletonScope();
            Bind<EffectManager>().ToSelf().InSingletonScope();
            Bind<ProfileManager>().ToSelf().InSingletonScope();
        }
    }
}