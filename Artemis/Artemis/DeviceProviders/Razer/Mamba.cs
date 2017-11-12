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
    public class Mamba : DeviceProvider
    {
        private GeneralSettings _generalSettings;
        public ILogger Logger { get; set; }
        public Mamba(ILogger logger)
        {
            Logger = logger;
            Type = DeviceType.Mouse;
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
            CanUse = Chroma.Instance.Query(Devices.Mamba).Connected || Chroma.Instance.Query(Devices.MambaTe).Connected;
            return CanUse;
        }

        public override void UpdateDevice(Bitmap bitmap)
        {
            if (Chroma.SdkAvailable && Chroma.Instance.Initialized)
            {
                var razerArray = RazerUtilities.BitmaptoMouseEffect(bitmap);
                Chroma.Instance.Mouse.SetCustom(razerArray);
            }
            return;
        }
    }
}
