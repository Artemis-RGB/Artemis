using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Media;
using Artemis.Models.Profiles;
using Artemis.Models.Profiles.Properties;
using Artemis.Properties;
using Pen = System.Windows.Media.Pen;
using Point = System.Windows.Point;
using Size = System.Windows.Size;

namespace Artemis.Utilities.Layers
{
    public static class Drawer
    {
        public static void Draw(DrawingContext c, KeyboardPropertiesModel props, KeyboardPropertiesModel applied)
        {
            if (applied.Brush == null)
                return;

            const int scale = 4;
            // Set up variables for this frame
            var rect = props.Contain
                ? new Rect(applied.X*scale, applied.Y*scale, applied.Width*scale, applied.Height*scale)
                : new Rect(props.X*scale, props.Y*scale, props.Width*scale, props.Height*scale);

            var s1 = new Rect();
            var s2 = new Rect();

            if (applied.Animation == LayerAnimation.SlideRight)
            {
                s1 = new Rect(new Point(rect.X + applied.AnimationProgress, rect.Y), new Size(rect.Width, rect.Height));
                s2 = new Rect(new Point(s1.X - rect.Width, rect.Y), new Size(rect.Width + 1, rect.Height));
            }
            if (applied.Animation == LayerAnimation.SlideLeft)
            {
                s1 = new Rect(new Point(rect.X - applied.AnimationProgress, rect.Y),
                    new Size(rect.Width + 1, rect.Height));
                s2 = new Rect(new Point(s1.X + rect.Width, rect.Y), new Size(rect.Width, rect.Height));
            }
            if (applied.Animation == LayerAnimation.SlideDown)
            {
                s1 = new Rect(new Point(rect.X, rect.Y + applied.AnimationProgress), new Size(rect.Width, rect.Height));
                s2 = new Rect(new Point(s1.X, s1.Y - rect.Height), new Size(rect.Width, rect.Height));
            }
            if (applied.Animation == LayerAnimation.SlideUp)
            {
                s1 = new Rect(new Point(rect.X, rect.Y - applied.AnimationProgress), new Size(rect.Width, rect.Height));
                s2 = new Rect(new Point(s1.X, s1.Y + rect.Height), new Size(rect.Width, rect.Height));
            }

            var clip = new Rect(applied.X*scale, applied.Y*scale, applied.Width*scale, applied.Height*scale);
            DrawRectangle(c, applied, clip, rect, s1, s2);
        }

        private static void DrawRectangle(DrawingContext c, KeyboardPropertiesModel properties, Rect clip,
            Rect rectangle,
            Rect slide1, Rect slide2)
        {
            var brush = properties.Brush.CloneCurrentValue();
            brush.Opacity = properties.Opacity;

            // Apply the pulse animation
            if (properties.Animation == LayerAnimation.Pulse)
                brush.Opacity = (Math.Sin(properties.AnimationProgress*Math.PI) + 1)*(properties.Opacity/2);
            else
                brush.Opacity = properties.Opacity;

            if (properties.Animation == LayerAnimation.Grow)
            {
                // Take an offset of 4 to allow layers to slightly leave their bounds
                var progress = (6.0 - properties.AnimationProgress)*10.0;
                if (progress < 0)
                {
                    brush.Opacity = 1 + (0.025*progress);
                    if (brush.Opacity < 0)
                        brush.Opacity = 0;
                    if (brush.Opacity > 1)
                        brush.Opacity = 1;
                }
                rectangle.Inflate(-rectangle.Width/100.0*progress, -rectangle.Height/100.0*progress);
                clip.Inflate(-clip.Width/100.0*progress, -clip.Height/100.0*progress);
            }

            c.PushClip(new RectangleGeometry(clip));
            // Most animation types can be drawn regularly
            if (properties.Animation == LayerAnimation.None ||
                properties.Animation == LayerAnimation.Grow ||
                properties.Animation == LayerAnimation.Pulse)
            {
                c.DrawRectangle(brush, null, rectangle);
            }
            // Sliding animations however, require offsetting two rects
            else
            {
                c.PushClip(new RectangleGeometry(clip));
                c.DrawRectangle(brush, null, slide1);
                c.DrawRectangle(brush, null, slide2);
                c.Pop();
            }
            c.Pop();
        }

        public static GifImage DrawGif(DrawingContext c, KeyboardPropertiesModel properties, GifImage gifImage,
            bool update = true)
        {
            if (string.IsNullOrEmpty(properties.GifFile))
                return null;
            if (!File.Exists(properties.GifFile))
                return null;

            const int scale = 4;

            // Only reconstruct GifImage if the underlying source has changed
            if (gifImage == null)
                gifImage = new GifImage(properties.GifFile);
            if (gifImage.Source != properties.GifFile)
                gifImage = new GifImage(properties.GifFile);

            var gifRect = new Rect(properties.X*scale, properties.Y*scale, properties.Width*scale,
                properties.Height*scale);

            var draw = update ? gifImage.GetNextFrame() : gifImage.GetFrame(0);
            c.DrawImage(ImageUtilities.BitmapToBitmapImage(new Bitmap(draw)), gifRect);
            return gifImage;
        }

        public static void UpdateMouse(LayerPropertiesModel properties)
        {
        }

        public static void UpdateHeadset(LayerPropertiesModel properties)
        {
        }

        public static ImageSource DrawThumbnail(LayerModel layerModel)
        {
            var thumbnailRect = new Rect(0, 0, 18, 18);
            var visual = new DrawingVisual();
            using (var c = visual.RenderOpen())
            {
                // Draw the appropiate icon or draw the brush
                if (layerModel.LayerType == LayerType.Folder)
                    c.DrawImage(ImageUtilities.BitmapToBitmapImage(Resources.folder), thumbnailRect);
                else if (layerModel.LayerType == LayerType.Headset)
                    c.DrawImage(ImageUtilities.BitmapToBitmapImage(Resources.headset), thumbnailRect);
                else if (layerModel.LayerType == LayerType.Mouse)
                    c.DrawImage(ImageUtilities.BitmapToBitmapImage(Resources.mouse), thumbnailRect);
                else if (layerModel.LayerType == LayerType.KeyboardGif)
                    c.DrawImage(ImageUtilities.BitmapToBitmapImage(Resources.gif), thumbnailRect);
                else if (layerModel.LayerType == LayerType.Keyboard && layerModel.Properties.Brush != null)
                    c.DrawRectangle(layerModel.Properties.Brush, new Pen(new SolidColorBrush(Colors.White), 1),
                        thumbnailRect);
            }

            var image = new DrawingImage(visual.Drawing);
            return image;
        }
    }
}