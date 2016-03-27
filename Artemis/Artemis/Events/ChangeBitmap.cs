using System.Drawing;

namespace Artemis.Events
{
    public class ChangeBitmap
    {
        public ChangeBitmap(Bitmap bitmap)
        {
            Bitmap = bitmap;
        }

        public Bitmap Bitmap { get; private set; }

        public void ChangeTextMessage(Bitmap bitmap)
        {
            Bitmap = bitmap;
        }
    }
}