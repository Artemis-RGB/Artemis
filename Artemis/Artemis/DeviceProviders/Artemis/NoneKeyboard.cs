using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using Artemis.Properties;

namespace Artemis.DeviceProviders.Artemis
{
    public class NoneKeyboard : KeyboardProvider
    {
        public NoneKeyboard()
        {
            Name = "None";
            Slug = "none";
            CantEnableText = "Waaaaah, this should not be happening!";
            Height = 1;
            Width = 1;
            PreviewSettings = new PreviewSettings(984, 375, new Thickness(0, 0, 0, 0), Resources.none);
        }

        public override void Disable()
        {
        }

        public override bool CanEnable()
        {
            return true;
        }

        public override void Enable()
        {
        }

        public override void DrawBitmap(Bitmap bitmap)
        {
        }

        public override KeyMatch? GetKeyPosition(Keys keyCode)
        {
            return null;
        }
    }
}