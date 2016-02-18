using System.Drawing;
using Artemis.KeyboardProviders.Razer.Utilities;
using Corale.Colore.Core;
using Corale.Colore.Razer.Keyboard;

namespace Artemis.KeyboardProviders.Razer
{
    public class BlackWidow : KeyboardProvider
    {
        public BlackWidow()
        {
            Name = "Razer BlackWidow Chroma";
        }

        public override bool CanEnable()
        {
            return Chroma.IsSdkAvailable();
        }

        public override void Enable()
        {
            Chroma.Instance.Initialize();
            Height = (int) Constants.MaxRows;
            Width = (int) Constants.MaxColumns;
        }

        public override void Disable()
        {
            Chroma.Instance.Uninitialize();
        }

        public override void DrawBitmap(Bitmap bitmap)
        {
            var razerArray = RazerUtilities.BitmapColorArray(bitmap, Height, Width);
            Chroma.Instance.Keyboard.SetGrid(razerArray);
        }
    }
}