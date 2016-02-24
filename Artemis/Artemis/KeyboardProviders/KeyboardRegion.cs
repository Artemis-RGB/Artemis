using System.Drawing;

namespace Artemis.KeyboardProviders
{
    public class KeyboardRegion
    {
        public string RegionName { get; set; }
        public Point TopLeft { get; set; }
        public Point BottomRight { get; set; }

        public KeyboardRegion(string regionName, Point topLeft, Point bottomRight)
        {
            RegionName = regionName;
            TopLeft = topLeft;
            BottomRight = bottomRight;
        }
    }
}