using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Artemis.Models.Profiles;

namespace Artemis.Utilities
{
    internal class LayerDrawer
    {
        private readonly LayerModel _layerModel;
        private double _animationProgress;
        private Rect _firstRect;
        private Rect _rectangle;
        private Rect _secondRect;

        public LayerDrawer(LayerModel layerModel, int scale)
        {
            Scale = scale;
            _layerModel = layerModel;
            _animationProgress = 0;
        }

        public int Scale { get; set; }

        public void Draw(DrawingContext c, bool update = true)
        {
            if (_layerModel.CalcProps.Brush == null || !_layerModel.Enabled)
                return;

            if (!update)
                _layerModel.CalcProps.Animation = LayerAnimation.None;

            UpdateAnimation();

            // Set up variables for this frame
            _rectangle = new Rect(_layerModel.CalcProps.X*Scale,
                _layerModel.CalcProps.Y*Scale, _layerModel.CalcProps.Width*Scale,
                _layerModel.CalcProps.Height*Scale);

            if (_layerModel.LayerType == LayerType.KeyboardRectangle)
                _layerModel.CalcProps.Brush.Dispatcher.Invoke(() => DrawRectangle(c));
            else if (_layerModel.LayerType == LayerType.KeyboardEllipse)
                _layerModel.CalcProps.Brush.Dispatcher.Invoke(() => DrawEllipse(c));
        }

        private void UpdateAnimation()
        {
            if (!_layerModel.Enabled)
                return;

            // Slide right animation
            if (_layerModel.CalcProps.Animation == LayerAnimation.SlideRight)
            {
                _firstRect = new Rect(new Point(_rectangle.X + _animationProgress, _rectangle.Y),
                    new Size(_rectangle.Width, _rectangle.Height));
                _secondRect = new Rect(new Point(_firstRect.X - _rectangle.Width, _rectangle.Y),
                    new Size(_rectangle.Width + 1, _rectangle.Height));

                if (_animationProgress > _layerModel.CalcProps.Width*4)
                    _animationProgress = 0;
            }

            // Slide left animation
            if (_layerModel.CalcProps.Animation == LayerAnimation.SlideLeft)
            {
                _firstRect = new Rect(new Point(_rectangle.X - _animationProgress, _rectangle.Y),
                    new Size(_rectangle.Width + 1, _rectangle.Height));
                _secondRect = new Rect(new Point(_firstRect.X + _rectangle.Width, _rectangle.Y),
                    new Size(_rectangle.Width, _rectangle.Height));

                if (_animationProgress > _layerModel.CalcProps.Width*4)
                    _animationProgress = 0;
            }

            // Slide down animation
            if (_layerModel.CalcProps.Animation == LayerAnimation.SlideDown)
            {
                _firstRect = new Rect(new Point(_rectangle.X, _rectangle.Y + _animationProgress),
                    new Size(_rectangle.Width, _rectangle.Height));
                _secondRect = new Rect(new Point(_firstRect.X, _firstRect.Y - _rectangle.Height),
                    new Size(_rectangle.Width, _rectangle.Height));

                if (_animationProgress > _layerModel.CalcProps.Height*4)
                    _animationProgress = 0;
            }

            // Slide up animation
            if (_layerModel.CalcProps.Animation == LayerAnimation.SlideUp)
            {
                _firstRect = new Rect(new Point(_rectangle.X, _rectangle.Y - _animationProgress),
                    new Size(_rectangle.Width, _rectangle.Height));
                _secondRect = new Rect(new Point(_firstRect.X, _firstRect.Y + _rectangle.Height),
                    new Size(_rectangle.Width, _rectangle.Height));

                if (_animationProgress > _layerModel.CalcProps.Height*4)
                    _animationProgress = 0;
            }

            // Pulse animation
            if (_layerModel.CalcProps.Animation == LayerAnimation.Pulse)
            {
                var opac = (Math.Sin(_animationProgress*Math.PI) + 1)*(_layerModel.UserProps.Opacity/2);
                _layerModel.CalcProps.Opacity = opac;
                if (_animationProgress > 2)
                    _animationProgress = 0;

                _animationProgress = _animationProgress + _layerModel.CalcProps.AnimationSpeed/2;
            }
            else
            {
                // Update the animation progress
                _animationProgress = _animationProgress + _layerModel.CalcProps.AnimationSpeed;
            }
        }

        public BitmapImage GetThumbnail()
        {
            if (_layerModel.UserProps.Brush == null)
                return null;

            _rectangle = new Rect(0, 0, 18, 18);

            //var bitmap = new Bitmap(18, 18);

            //using (var g = Graphics.FromImage(bitmap))
            //{
            //    g.SmoothingMode = SmoothingMode.AntiAlias;
            //    if (_layerModel.LayerType == LayerType.KeyboardEllipse)
            //    {
            //        g.FillEllipse(_layerModel.LayerUserProperties.Brush, _rectangle);
            //        g.DrawEllipse(new Pen(Color.Black, 1), 0, 0, 17, 17);
            //    }
            //    else if (_layerModel.LayerType == LayerType.KeyboardRectangle)
            //    {
            //        g.FillRectangle(_layerModel.LayerUserProperties.Brush, _rectangle);
            //        g.DrawRectangle(new Pen(Color.Black, 1), 0, 0, 17, 17);
            //    }
            //    else
            //        bitmap = Resources.folder;
            //}

            //using (var memory = new MemoryStream())
            //{
            //    bitmap.Save(memory, ImageFormat.Png);
            //    memory.Position = 0;

            //    var bitmapImage = new BitmapImage();
            //    bitmapImage.BeginInit();
            //    bitmapImage.StreamSource = memory;
            //    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            //    bitmapImage.EndInit();

            //    return bitmapImage;
            //}
            return null;
        }

        public void DrawRectangle(DrawingContext c)
        {
            var brush = _layerModel.CalcProps.Brush.CloneCurrentValue();
            brush.Opacity = _layerModel.CalcProps.Opacity;

            if (_layerModel.CalcProps.Animation == LayerAnimation.SlideDown ||
                _layerModel.CalcProps.Animation == LayerAnimation.SlideLeft ||
                _layerModel.CalcProps.Animation == LayerAnimation.SlideRight ||
                _layerModel.CalcProps.Animation == LayerAnimation.SlideUp)
            {
                c.PushClip(new RectangleGeometry(_rectangle));
                c.DrawRectangle(brush, null, _firstRect);
                c.DrawRectangle(brush, null, _secondRect);
                c.Pop();
            }
            else
            {
                c.DrawRectangle(brush, null, _rectangle);
            }
        }

        public void DrawEllipse(DrawingContext c)
        {
            c.DrawEllipse(_layerModel.CalcProps.Brush, null,
                new Point(_rectangle.Width/2, _rectangle.Height/2), _rectangle.Width, _rectangle.Height);
        }

        public void DrawGif(DrawingContext bmp)
        {
        }

        public void UpdateMouse()
        {
        }

        public void UpdateHeadset()
        {
        }
    }
}