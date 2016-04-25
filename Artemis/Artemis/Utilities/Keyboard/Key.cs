using System.Windows.Forms;

namespace Artemis.Utilities.Keyboard
{
    public class Key
    {
        public Key(Keys keyCode, int posX, int posY)
        {
            KeyCode = keyCode;
            PosX = posX;
            PosY = posY;
        }

        public Keys KeyCode { get; set; }
        public int PosX { get; set; }
        public int PosY { get; set; }
    }
}