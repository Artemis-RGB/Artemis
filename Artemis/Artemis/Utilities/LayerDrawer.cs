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

        public void Draw(DrawingContext c)
        {
            if (_layerModel.LayerCalculatedProperties.Brush == null)
                return;

            // Set up variables for this frame
            _rectangle = new Rect(_layerModel.LayerCalculatedProperties.X*Scale,
                _layerModel.LayerCalculatedProperties.Y*Scale, _layerModel.LayerCalculatedProperties.Width*Scale,
                _layerModel.LayerCalculatedProperties.Height*Scale);

            if (_layerModel.LayerType == LayerType.KeyboardRectangle)
                _layerModel.LayerCalculatedProperties.Brush.Dispatcher.Invoke(() => DrawRectangle(c));
            else if (_layerModel.LayerType == LayerType.KeyboardEllipse)
                _layerModel.LayerCalculatedProperties.Brush.Dispatcher.Invoke(() => DrawEllipse(c));

            UpdateAnimation();
        }

        private void UpdateAnimation()
        {
            if (_layerModel.LayerCalculatedProperties.Animation == LayerAnimation.SlideRight)
            {
                _firstRect = new Rect(new Point(_rectangle.X + _animationProgress, _rectangle.Y),
                    new Size(_rectangle.Width, _rectangle.Height));
                _secondRect = new Rect(new Point(_firstRect.X - _rectangle.Width, _rectangle.Y),
                    new Size(_rectangle.Width + 1, _rectangle.Height));

                if (_animationProgress > _layerModel.LayerCalculatedProperties.Width*4)
                    _animationProgress = 0;
            }
            if (_layerModel.LayerCalculatedProperties.Animation == LayerAnimation.SlideLeft)
            {
                _firstRect = new Rect(new Point(_rectangle.X - _animationProgress, _rectangle.Y),
                    new Size(_rectangle.Width + 1, _rectangle.Height));
                _secondRect = new Rect(new Point(_firstRect.X + _rectangle.Width, _rectangle.Y),
                    new Size(_rectangle.Width , _rectangle.Height));

                if (_animationProgress > _layerModel.LayerCalculatedProperties.Width*4)
                    _animationProgress = 0;
            }
            if (_layerModel.LayerCalculatedProperties.Animation == LayerAnimation.SlideDown)
            {
                _firstRect = new Rect(new Point(_rectangle.X, _rectangle.Y + _animationProgress),
                    new Size(_rectangle.Width, _rectangle.Height));
                _secondRect = new Rect(new Point(_firstRect.X, _firstRect.Y - _rectangle.Height),
                    new Size(_rectangle.Width, _rectangle.Height));

                if (_animationProgress > _layerModel.LayerCalculatedProperties.Height * 4)
                    _animationProgress = 0;
            }
            if (_layerModel.LayerCalculatedProperties.Animation == LayerAnimation.SlideUp)
            {
                _firstRect = new Rect(new Point(_rectangle.X, _rectangle.Y - _animationProgress),
                    new Size(_rectangle.Width, _rectangle.Height));
                _secondRect = new Rect(new Point(_firstRect.X, _firstRect.Y + _rectangle.Height),
                    new Size(_rectangle.Width, _rectangle.Height));

                if (_animationProgress > _layerModel.LayerCalculatedProperties.Height * 4)
                    _animationProgress = 0;
            }

            // Update the rotation progress
            _animationProgress = _animationProgress + _layerModel.LayerCalculatedProperties.AnimationSpeed;
        }

        public BitmapImage GetThumbnail()
        {
            if (_layerModel.LayerUserProperties.Brush == null)
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
            _layerModel.LayerCalculatedProperties.Brush.Opacity = _layerModel.LayerCalculatedProperties.Opacity;

            if (_layerModel.LayerCalculatedProperties.Animation == LayerAnimation.SlideDown ||
                _layerModel.LayerCalculatedProperties.Animation == LayerAnimation.SlideLeft ||
                _layerModel.LayerCalculatedProperties.Animation == LayerAnimation.SlideRight ||
                _layerModel.LayerCalculatedProperties.Animation == LayerAnimation.SlideUp)
            {
                c.PushClip(new RectangleGeometry(_rectangle));
                c.DrawRectangle(_layerModel.LayerCalculatedProperties.Brush, null, _firstRect);
                c.DrawRectangle(_layerModel.LayerCalculatedProperties.Brush, null, _secondRect);
                c.Pop();
            }
            else
            {
                c.DrawRectangle(_layerModel.LayerCalculatedProperties.Brush, null, _rectangle);
            }
        }

        public void DrawEllipse(DrawingContext c)
        {
            c.DrawEllipse(_layerModel.LayerCalculatedProperties.Brush, null,
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