using System.Drawing;
using System.Windows;
using Artemis.DeviceProviders.Razer.Utilities;
using Artemis.Properties;
using Corale.Colore.Core;
using Corale.Colore.Razer;
using Constants = Corale.Colore.Razer.Keyboard.Constants;

namespace Artemis.DeviceProviders.Razer
{
    public class BlackWidow : KeyboardProvider
    {
        public BlackWidow()
        {
            Name = "Razer BlackWidow Chroma";
            CantEnableText = "Couldn't connect to your Razer BlackWidow Chroma.\n" +
                             "Please check your cables and try updating Razer Synapse.\n\n" +
                             "If needed, you can select a different keyboard in Artemis under settings.";

            Height = Constants.MaxRows;
            Width = Constants.MaxColumns;
            PreviewSettings = new PreviewSettings(665, 175, new Thickness(0, -15, 0, 0), Resources.blackwidow);
        }

        public override bool CanEnable()
        {
            if (!Chroma.IsSdkAvailable())
                return false;

            // Some people have Synapse installed, but not a Chroma keyboard, deal with this
            var blackWidowFound = Chroma.Instance.Query(Devices.Blackwidow).Connected;
            var blackWidowTeFound = Chroma.Instance.Query(Devices.BlackwidowTe).Connected;
            return blackWidowFound || blackWidowTeFound;
        }

        public override void Enable()
        {
            Chroma.Instance.Initialize();
        }

        public override void Disable()
        {
            Chroma.Instance.Uninitialize();
        }

        public override void DrawBitmap(Bitmap bitmap)
        {
            var razerArray = RazerUtilities.BitmapColorArray(bitmap, Height, Width);
            Chroma.Instance.Keyboard.SetCustom(razerArray);
        }
    }
}