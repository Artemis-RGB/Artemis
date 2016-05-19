using System.Threading;
using System.Windows.Media;
using CUE.NET;
using CUE.NET.Devices.Generic.Enums;
using CUE.NET.Devices.Mouse;
using CUE.NET.Exceptions;

namespace Artemis.DeviceProviders.Corsair
{
    internal class CorsairMice : DeviceProvider
    {
        private readonly CorsairRGB _corsairRgb;
        private CorsairMouse _mouse;

        public CorsairMice(CorsairRGB corsairRgb)
        {
            _corsairRgb = corsairRgb;
            UpdateCanUse();
        }

        private void UpdateCanUse()
        {
            if (!CanInitializeSdk())
            {
                CanUse = false;
                return;
            }

            if (CueSDK.ProtocolDetails == null)
                CueSDK.Initialize(true);

            _mouse = CueSDK.MouseSDK;
            
        }

        private static bool CanInitializeSdk()
        {
            // Try for about 10 seconds, in case CUE isn't started yet
            var tries = 0;
            while (tries < 9)
            {
                try
                {
                    if (CueSDK.ProtocolDetails == null)
                        CueSDK.Initialize();
                }
                catch (CUEException e)
                {
                    if (e.Error == CorsairError.ServerNotFound)
                    {
                        tries++;
                        Thread.Sleep(1000);
                        continue;
                    }
                }
                catch (WrapperException)
                {
                    CueSDK.Reinitialize();
                    return true;
                }

                return true;
            }

            return false;
        }

        public override void UpdateDevice(Brush brush)
        {
            if (!CanUse)
                return;
        }
    }
}