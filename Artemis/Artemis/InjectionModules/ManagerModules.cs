using Artemis.Managers;
using Ninject.Modules;

namespace Artemis.InjectionModules
{
    public class ManagerModules : NinjectModule
    {
        public override void Load()
        {
            Bind<MainManager>().ToSelf().InSingletonScope();
            Bind<LoopManager>().ToSelf().InSingletonScope();
            Bind<DeviceManager>().ToSelf().InSingletonScope();
            Bind<ModuleManager>().ToSelf().InSingletonScope();
            Bind<PreviewManager>().ToSelf().InSingletonScope();
            Bind<LuaManager>().ToSelf().InSingletonScope();
        }
    }
}