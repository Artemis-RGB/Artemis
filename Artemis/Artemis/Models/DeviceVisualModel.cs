using System.Drawing;
using System.Windows;
using System.Windows.Media;
using Artemis.DeviceProviders;
using Artemis.Profiles.Layers.Interfaces;
using Color = System.Drawing.Color;

namespace Artemis.Models
{
    public class DeviceVisualModel : DrawingVisual
    {
        private readonly int _x;
        private readonly KeyboardProvider _keyboard;

        public DeviceVisualModel(DrawType drawType, int x)
        {
            _x = x;

            DrawType = drawType;
            VisualBitmapScalingMode = BitmapScalingMode.LowQuality;
        }

        public DeviceVisualModel(KeyboardProvider keyboard, int x)
        {
            _x = x;
            _keyboard = keyboard;

            DrawType = DrawType.Keyboard;
            VisualBitmapScalingMode = BitmapScalingMode.LowQuality;
        }

        public DrawType DrawType { get; }

        public Rect RelativeRect => DrawType == DrawType.Keyboard
            ? new Rect(_x, 0, _keyboard.Width * 4, _keyboard.Height * 4)
            : new Rect(_x, 0, 20, 20);

        public Rect Rect => DrawType == DrawType.Keyboard
            ? new Rect(0, 0, _keyboard.Width * 4, _keyboard.Height * 4)
            : new Rect(0, 0, 20, 20);

        public Rectangle RelativeRectangle => DrawType == DrawType.Keyboard
            ? new Rectangle(_x, 0, _keyboard.Width * 4, _keyboard.Height * 4)
            : new Rectangle(_x, 0, 20, 20);

        public Bitmap GetBitmapFromFrame(Bitmap frame)
        {
            var bitmap = DrawType == DrawType.Keyboard
                ? new Bitmap(_keyboard.Width * 4, _keyboard.Height * 4)
                : new Bitmap(20, 20);
            bitmap.SetResolution(96, 96);

            using (var g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.Black);
                g.DrawImage(frame, new Rectangle(0, 0, bitmap.Width, bitmap.Height), RelativeRectangle,
                    GraphicsUnit.Pixel);
            }

            return bitmap;
        }
    }
}