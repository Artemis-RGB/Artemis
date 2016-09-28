using System.Linq;
using System.Windows;
using System.Windows.Forms;
using Artemis.DeviceProviders.Logitech.Utilities;
using Artemis.Properties;

namespace Artemis.DeviceProviders.Logitech
{
    internal class G910 : LogitechKeyboard
    {
        public G910()
        {
            Name = "Logitech G910 RGB";
            Slug = "logitech-g910";
            CantEnableText = "Couldn't connect to your Logitech G910.\n" +
                             "Please check your cables and updating the Logitech Gaming Software\n" +
                             "A minimum version of 8.81.15 is required.\n\n" +
                             "If needed, you can select a different keyboard in Artemis under settings.";
            Height = 6;
            Width = 21;
            PreviewSettings = new PreviewSettings(540, 154, new Thickness(25, -80, 0, 0), Resources.g910);
        }

        public override KeyMatch? GetKeyPosition(Keys keyCode)
        {
            return KeyMap.QwertyLayout.FirstOrDefault(k => k.KeyCode == keyCode);
        }
    }
}