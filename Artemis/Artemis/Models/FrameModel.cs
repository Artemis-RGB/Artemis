using System;
using System.Drawing;
using System.Windows;
using System.Windows.Media;
using Artemis.DeviceProviders;
using Artemis.Profiles.Layers.Interfaces;
using Artemis.Utilities;
using Color = System.Windows.Media.Color;
using Pen = System.Windows.Media.Pen;

namespace Artemis.Models
{
    public class FrameModel : IDisposable
    {
        private readonly Rect _rect;

        public FrameModel(KeyboardProvider keyboard, bool renderMice, bool renderHeadsets, bool renderGenerics,
            bool renderMousemats)
        {
            if (keyboard == null)
                return;

            // Setup bitmaps
            var x = 0;
            if (renderMice)
            {
                MouseModel = new DeviceVisualModel(DrawType.Mouse, x);
                x += 20;
            }
            if (renderHeadsets)
            {
                HeadsetModel = new DeviceVisualModel(DrawType.Headset, x);
                x += 20;
            }
            if (renderGenerics)
            {
                GenericModel = new DeviceVisualModel(DrawType.Generic, x);
                x += 20;
            }
            if (renderMousemats)
            {
                MousematModel = new DeviceVisualModel(DrawType.Mousemat, x);
                x += 20;
            }

            KeyboardModel = new DeviceVisualModel(keyboard, x);

            // If not rendering anything but keyboard, use keyboard height, else default to 40
            var height = 20;
            if (keyboard.Height * 4 > height)
                height = keyboard.Height * 4;
            _rect = new Rect(0, 0, x + keyboard.Width * 4, height);
        }

        public DeviceVisualModel MouseModel { get; }
        public DeviceVisualModel HeadsetModel { get; }
        public DeviceVisualModel GenericModel { get; }
        public DeviceVisualModel MousematModel { get; }
        public DeviceVisualModel KeyboardModel { get; }

        public Bitmap KeyboardBitmap { get; private set; }
        public Bitmap MouseBitmap { get; private set; }
        public Bitmap HeadsetBitmap { get; private set; }
        public Bitmap GenericBitmap { get; private set; }
        public Bitmap MousematBitmap { get; private set; }

        public void Dispose()
        {
            KeyboardBitmap?.Dispose();
            MouseBitmap?.Dispose();
            HeadsetBitmap?.Dispose();
            GenericBitmap?.Dispose();
            MousematBitmap?.Dispose();
        }

        public void RenderBitmaps()
        {
            var visual = new DrawingVisual();
            using (var c = visual.RenderOpen())
            {
                c.PushClip(new RectangleGeometry(_rect));
                c.DrawRectangle(new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)), null, _rect);

                if (MouseModel != null)
                    c.DrawRectangle(new VisualBrush(MouseModel), new Pen(), MouseModel.RelativeRect);
                if (HeadsetModel != null)
                    c.DrawRectangle(new VisualBrush(HeadsetModel), new Pen(), HeadsetModel.RelativeRect);
                if (GenericModel != null)
                    c.DrawRectangle(new VisualBrush(GenericModel), new Pen(), GenericModel.RelativeRect);
                if (MousematModel != null)
                    c.DrawRectangle(new VisualBrush(MousematModel), new Pen(), MousematModel.RelativeRect);
                c.DrawRectangle(new VisualBrush(KeyboardModel), new Pen(), KeyboardModel.RelativeRect);

                c.Pop();
            }

            var frameBitmap = ImageUtilities.DrawingVisualToBitmap(visual, _rect);
            if (MouseModel != null)
                MouseBitmap = MouseModel.GetBitmapFromFrame(frameBitmap);
            if (HeadsetModel != null)
                HeadsetBitmap = HeadsetModel.GetBitmapFromFrame(frameBitmap);
            if (GenericModel != null)
                GenericBitmap = GenericModel.GetBitmapFromFrame(frameBitmap);
            if (MousematModel != null)
                MousematBitmap = MousematModel.GetBitmapFromFrame(frameBitmap);
            KeyboardBitmap = KeyboardModel.GetBitmapFromFrame(frameBitmap);
        }
    }
}