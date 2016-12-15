using Artemis.DeviceProviders;
using Artemis.DeviceProviders.Artemis;
using Artemis.DeviceProviders.CoolerMaster;
using Artemis.DeviceProviders.Corsair;
using Artemis.DeviceProviders.Logitech;
using Artemis.DeviceProviders.Razer;
using Ninject.Modules;

namespace Artemis.InjectionModules
{
    public class DeviceModules : NinjectModule
    {
        public override void Load()
        {
            // Keyboards
            Bind<DeviceProvider>().To<NoneKeyboard>().InSingletonScope();
            Bind<DeviceProvider>().To<CorsairKeyboard>().InSingletonScope();
            Bind<DeviceProvider>().To<G910>().InSingletonScope();
            Bind<DeviceProvider>().To<G810>().InSingletonScope();
            Bind<DeviceProvider>().To<BlackWidow>().InSingletonScope();
            Bind<DeviceProvider>().To<MasterkeysProL>().InSingletonScope();
            Bind<DeviceProvider>().To<MasterkeysProS>().InSingletonScope();

            // Mice
            Bind<DeviceProvider>().To<CorsairMouse>().InSingletonScope();

            // Headsets
            Bind<DeviceProvider>().To<CorsairHeadset>().InSingletonScope();

            // Mousemats
            Bind<DeviceProvider>().To<CorsairMousemat>().InSingletonScope();

            // Other
            Bind<DeviceProvider>().To<LogitechGeneric>().InSingletonScope();
        }
    }
}