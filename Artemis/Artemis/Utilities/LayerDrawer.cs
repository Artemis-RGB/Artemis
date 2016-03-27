using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using Artemis.Models.Profiles;
using Artemis.Properties;

namespace Artemis.Utilities
{
    internal class LayerDrawer
    {
        private readonly LayerModel _layerModel;
        private Rectangle _rectangle;
        private double _rotationProgress;
        private Rectangle _userRectangle;

        public LayerDrawer(LayerModel layerModel)
        {
            _layerModel = layerModel;
            _rotationProgress = 0;
        }

        public void Draw(Graphics graphics)
        {
            _rectangle = new Rectangle(
                _layerModel.LayerCalculatedProperties.X,
                _layerModel.LayerCalculatedProperties.Y,
                _layerModel.LayerCalculatedProperties.Width,
                _layerModel.LayerCalculatedProperties.Height);
            _userRectangle = new Rectangle(
                _layerModel.LayerUserProperties.X,
                _layerModel.LayerUserProperties.Y,
                _layerModel.LayerUserProperties.Width,
                _layerModel.LayerUserProperties.Height);

            if (_layerModel.LayerType == LayerType.Ellipse)
                DrawEllipse(graphics);
            else if (_layerModel.LayerType == LayerType.Ellipse)
                DrawRectangle(graphics);

            // Update the rotation progress
            _rotationProgress = _rotationProgress + _layerModel.LayerCalculatedProperties.RotateSpeed;

            if (_layerModel.LayerCalculatedProperties.ContainedBrush && _rotationProgress > _rectangle.Width)
                _rotationProgress = _layerModel.LayerCalculatedProperties.RotateSpeed;
            else if (!_layerModel.LayerCalculatedProperties.ContainedBrush && _rotationProgress > _userRectangle.Width)
                _rotationProgress = _layerModel.LayerCalculatedProperties.RotateSpeed;
        }

        public BitmapImage GetPreviewImage()
        {
            _rectangle = new Rectangle(0, 0, 18, 18);
            _userRectangle = new Rectangle(0, 0, 18, 18);
            _layerModel.LayerCalculatedProperties.Opacity = 255;
            var brush = CreateGradientBrush(_layerModel.LayerUserProperties.Colors);
            var bitmap = new Bitmap(18, 18);

            using (var g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                if (_layerModel.LayerType == LayerType.Ellipse)
                {
                    g.FillEllipse(brush, _rectangle);
                    g.DrawEllipse(new Pen(Color.Black, 1), 0, 0, 17, 17);
                }
                else if (_layerModel.LayerType == LayerType.Rectangle)
                {
                    g.FillRectangle(brush, _rectangle);
                    g.DrawRectangle(new Pen(Color.Black, 1), 0, 0, 17, 17);
                }
                else
                    bitmap = Resources.folder;
            }

            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                return bitmapImage;
            }
        }

        public void DrawRectangle(Graphics graphics)
        {
        }

        public void DrawEllipse(Graphics graphics)
        {
        }

        private LinearGradientBrush CreateGradientBrush(List<Color> colors)
        {
            ColorBlend colorBlend;
            var props = _layerModel.LayerCalculatedProperties;
            // Create a ColorBlend
            if (colors.Count == 0)
            {
                colorBlend = new ColorBlend
                {
                    Colors = new[] {Color.Transparent, Color.Transparent},
                    Positions = new[] {0F, 1F}
                };
            }
            else if (colors.Count == 1)
            {
                colorBlend = new ColorBlend
                {
                    Colors = new[] {colors[0], colors[0]},
                    Positions = new[] {0F, 1F}
                };
            }
            else
            {
                colorBlend = props.Rotate
                    ? new ColorBlend {Colors = CreateTilebleColors(colors).ToArray()}
                    : new ColorBlend {Colors = colors.ToArray()};
            }

            // If needed, apply opacity to the colors in the blend
            if (props.Opacity < 255)
                for (var i = 0; i < colorBlend.Colors.Length; i++)
                    colorBlend.Colors[i] = Color.FromArgb(props.Opacity, colorBlend.Colors[i]);

            // Devide the colors over the colorblend
            var devider = (float) colorBlend.Colors.Length - 1;
            var positions = new List<float>();
            for (var i = 0; i < colorBlend.Colors.Length; i++)
                positions.Add(i/devider);

            // Apply the devided positions
            colorBlend.Positions = positions.ToArray();

            RectangleF rect;
            if (props.Rotate)
                rect = _layerModel.LayerCalculatedProperties.ContainedBrush
                    ? new Rectangle((int) _rotationProgress + _rectangle.X, _rectangle.Y, _rectangle.Width*2,
                        _rectangle.Height*2)
                    : new Rectangle((int) _rotationProgress + _userRectangle.X, _userRectangle.Y, _userRectangle.Width*2,
                        _userRectangle.Height*2);
            else
                rect = _layerModel.LayerCalculatedProperties.ContainedBrush
                    ? new Rectangle(_rectangle.X, _rectangle.Y, _rectangle.Width, _rectangle.Height)
                    : new Rectangle(_userRectangle.X, _userRectangle.Y, _userRectangle.Width, _userRectangle.Height);

            return new LinearGradientBrush(rect, Color.Transparent, Color.Transparent,
                _layerModel.LayerCalculatedProperties.GradientMode)
            {
                InterpolationColors = colorBlend
            };
        }

        private List<Color> CreateTilebleColors(List<Color> sourceColors)
        {
            // Create a list using the original colors
            var tilebleColors = new List<Color>(sourceColors);
            // Add the original colors again
            tilebleColors.AddRange(sourceColors);
            // Add the first color, smoothing the transition
            tilebleColors.Add(sourceColors.FirstOrDefault());
            return tilebleColors;
        }
    }
}