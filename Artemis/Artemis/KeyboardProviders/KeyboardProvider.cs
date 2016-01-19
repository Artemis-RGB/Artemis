using System.Drawing;

namespace Artemis.KeyboardProviders
{
    public abstract class KeyboardProvider
    {
        public string Name { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }

        public abstract void Enable();
        public abstract void Disable();
        public abstract void DrawBitmap(Bitmap bitmap);
    }
}