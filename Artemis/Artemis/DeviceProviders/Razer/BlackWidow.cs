using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using Artemis.DAL;
using Artemis.DeviceProviders.Logitech.Utilities;
using Artemis.DeviceProviders.Razer.Utilities;
using Artemis.Properties;
using Artemis.Settings;
using Corale.Colore.Core;
using Corale.Colore.Razer;
using Constants = Corale.Colore.Razer.Keyboard.Constants;

namespace Artemis.DeviceProviders.Razer
{
    public class BlackWidow : KeyboardProvider
    {
        private GeneralSettings _generalSettings;

        public BlackWidow()
        {
            Name = "Razer BlackWidow Chroma";
            Slug = "razer-blackwidow-chroma";
            CantEnableText = "Couldn't connect to your Razer BlackWidow Chroma.\n" +
                             "Please check your cables and try updating Razer Synapse.\n\n" +
                             "If needed, you can select a different keyboard in Artemis under settings.";

            Height = Constants.MaxRows;
            Width = Constants.MaxColumns;
            PreviewSettings = new PreviewSettings(new Thickness(0, -15, 0, 0), Resources.blackwidow);
            _generalSettings = SettingsProvider.Load<GeneralSettings>();
        }

        public override bool CanEnable()
        {
            if (!Chroma.SdkAvailable)
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

        public override KeyMatch? GetKeyPosition(Keys keyCode)
        {
            // TODO: Needs it's own keymap or a way to get it from the Chroma SDK
            switch (_generalSettings.Layout)
            {
                case "Qwerty":
                    return KeyMap.QwertyLayout.FirstOrDefault(k => k.KeyCode == keyCode);
                case "Qwertz":
                    return KeyMap.QwertzLayout.FirstOrDefault(k => k.KeyCode == keyCode);
                default:
                    return KeyMap.AzertyLayout.FirstOrDefault(k => k.KeyCode == keyCode);
            }
        }
    }
}