using Artemis.Settings;
using Artemis.DAL;
using System;
using System.Drawing;
using Corale.Colore.Core;
using Corale.Colore.Razer;
using Ninject.Extensions.Logging;
using Artemis.DeviceProviders.Razer.Utilities;

namespace Artemis.DeviceProviders.Razer
{
    public class Firefly : DeviceProvider
    {
        private GeneralSettings _generalSettings;
        public ILogger Logger { get; set; }
        public Firefly(ILogger logger)
        {
            Logger = logger;
            Type = DeviceType.Mousemat;
            _generalSettings = SettingsProvider.Load<GeneralSettings>();
        }

        public override void Disable()
        {
            throw new NotSupportedException("Can only disable a keyboard");
        }

        public override bool TryEnable()
        {
            if (!Chroma.SdkAvailable)
                return false;


            // Some people have Synapse installed, but not a Chroma keyboard, deal with this
            Chroma.Instance.Initialize();
            CanUse = Chroma.Instance.Query(Devices.Firefly).Connected;
            return CanUse;
        }

        public override void UpdateDevice(Bitmap bitmap)
        {
            if (Chroma.SdkAvailable && Chroma.Instance.Initialized)
            {
                var razerArray = RazerUtilities.BitmaptoMousePadEffect(bitmap);
                Chroma.Instance.Mousepad.SetCustom(razerArray);
            }
            return;
        }
    }
}
