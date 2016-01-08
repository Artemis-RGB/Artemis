using System.Drawing;

namespace Artemis.KeyboardProviders.Corsair
{
    internal class K70 : KeyboardProvider
    {
        public K70()
        {
            Name = "Corsair Gaming K70 RGB";
        }

        public override void Enable()
        {
        }

        public override void Disable()
        {
        }

        public override void DrawBitmap(Bitmap bitmap)
        {
            // TODO: Convert bitmap to something tasty for CUE.NET
        }
    }
}