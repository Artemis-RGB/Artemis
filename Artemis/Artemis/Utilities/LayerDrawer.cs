using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Artemis.Models.Profiles;

namespace Artemis.Utilities
{
    internal class LayerDrawer
    {
        private readonly LayerModel _layerModel;
        private Rect _rectangle;
        private double _rotationProgress;
        private Rect _userRectangle;

        public LayerDrawer(LayerModel layerModel, int scale)
        {
            Scale = scale;
            _layerModel = layerModel;
            _rotationProgress = 0;
        }

        public int Scale { get; set; }

        public void Draw(DrawingContext c)
        {
            if (_layerModel.LayerCalculatedProperties.Brush == null)
                return;
            if (!_layerModel.LayerCalculatedProperties.Brush.IsFrozen)
                return;

            // Set up variables for this frame
            _rectangle = new Rect(_layerModel.LayerCalculatedProperties.X*Scale,
                _layerModel.LayerCalculatedProperties.Y*Scale, _layerModel.LayerCalculatedProperties.Width*Scale,
                _layerModel.LayerCalculatedProperties.Height*Scale);
            _userRectangle = new Rect(_layerModel.LayerUserProperties.X*Scale,
                _layerModel.LayerUserProperties.Y*Scale, _layerModel.LayerUserProperties.Width*Scale,
                _layerModel.LayerUserProperties.Height*Scale);

            if (_layerModel.LayerType == LayerType.KeyboardRectangle)
                DrawRectangle(c);
            else if (_layerModel.LayerType == LayerType.KeyboardEllipse)
                DrawEllipse(c);

            // Update the rotation progress
            _rotationProgress = _rotationProgress + _layerModel.LayerCalculatedProperties.RotateSpeed;

            if (_layerModel.LayerCalculatedProperties.ContainedBrush && _rotationProgress > _rectangle.Width)
                _rotationProgress = _layerModel.LayerCalculatedProperties.RotateSpeed;
            else if (!_layerModel.LayerCalculatedProperties.ContainedBrush && _rotationProgress > _userRectangle.Width)
                _rotationProgress = _layerModel.LayerCalculatedProperties.RotateSpeed;
        }

        public BitmapImage GetThumbnail()
        {
            if (_layerModel.LayerUserProperties.Brush == null)
                return null;

            _rectangle = new Rect(0, 0, 18, 18);
            _userRectangle = new Rect(0, 0, 18, 18);

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
            c.DrawRectangle(_layerModel.LayerCalculatedProperties.Brush, null, _rectangle);
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