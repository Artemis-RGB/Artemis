using System.Drawing;

namespace Artemis.KeyboardProviders
{
    public class KeyboardRegion
    {
        public KeyboardRegion(string regionName, Point topLeft, Point bottomRight)
        {
            RegionName = regionName;
            TopLeft = topLeft;
            BottomRight = bottomRight;
        }

        public string RegionName { get; set; }
        public Point TopLeft { get; set; }
        public Point BottomRight { get; set; }

        public Rectangle GetRectangle() => new Rectangle(TopLeft.X, TopLeft.Y, BottomRight.X - TopLeft.X, BottomRight.Y - TopLeft.Y);
    }
}