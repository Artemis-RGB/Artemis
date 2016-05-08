using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Artemis.Managers;
using Artemis.Models;
using Artemis.Utilities.Keyboard;
using Kaliko.ImageLibrary;
using Kaliko.ImageLibrary.Filters;

namespace Artemis.Modules.Effects.AmbientLightning
{
    internal class AmbientLightningEffectModel : EffectModel
    {
        private KeyboardRectangle _botRect;
        private List<Color> _colors;
        private List<Rectangle> _rectangles;
        private ScreenCapture _screenCapturer;
        private KeyboardRectangle _topRect;

        public AmbientLightningEffectModel(MainManager mainManager, AmbientLightningEffectSettings settings)
            : base(mainManager)
        {
            Name = "Ambient Lightning";
            Settings = settings;
            Scale = 4;
            Initialized = false;
        }

        public int Scale { get; set; }

        public AmbientLightningEffectSettings Settings { get; set; }

        public KeyboardRectangle KeyboardRectangle { get; set; }

        public override void Dispose()
        {
            Initialized = false;

            _screenCapturer.Dispose();
            _screenCapturer = null;
        }

        public override void Enable()
        {
            Initialized = false;

            _colors = new List<Color>();
            _screenCapturer = new ScreenCapture();
            _topRect = new KeyboardRectangle(MainManager.KeyboardManager.ActiveKeyboard, 0, 0, new List<Color>(),
                LinearGradientMode.Horizontal) {Height = MainManager.KeyboardManager.ActiveKeyboard.Height*Scale/2};
            _botRect = new KeyboardRectangle(MainManager.KeyboardManager.ActiveKeyboard, 0, 0, new List<Color>(),
                LinearGradientMode.Horizontal);

            Initialized = true;
        }

        public override void Update()
        {
            var capture = _screenCapturer.Capture();
            if (capture == null)
                return;

            _rectangles = new List<Rectangle>();
            // Analise the result
            // Chop the screen into 2 rows and 3 columns
            var resolution = Screen.PrimaryScreen.Bounds;
            var blockWidth = resolution.Width/3;
            var blockHeight = resolution.Height/2;
            var colorIndex = 0;
            for (var row = 0; row < 2; row++)
            {
                for (var column = 0; column < 3; column++)
                {
                    var blockBase = new Point(blockWidth*column, blockHeight*row);
                    var samples = new List<Color>();
                    // For each block, take samples
                    for (var blockRow = 0; blockRow < 6; blockRow++)
                    {
                        for (var blockColumn = 0; blockColumn < 6; blockColumn++)
                        {
                            var x = blockWidth/6*blockColumn + blockWidth/6/4 + blockBase.X;
                            var y = blockHeight/6*blockRow + blockHeight/6/4 + blockBase.Y;
                            samples.Add(_screenCapturer.GetColor(capture, new Point(x, y)));
                        }
                    }

                    // Take the average of the samples
                    var averageR = samples.Sum(s => s.R)/samples.Count;
                    var averageG = samples.Sum(s => s.G)/samples.Count;
                    var averageB = samples.Sum(s => s.B)/samples.Count;

                    if (_colors.Count <= colorIndex)
                        _colors.Add(Color.FromArgb(255, averageR, averageG, averageB));
                    else
                        _colors[colorIndex] = Color.FromArgb(255, (averageR + _colors[colorIndex].R*5)/6,
                            (averageG + _colors[colorIndex].G*5)/6, (averageB + _colors[colorIndex].B*5)/6);
                    colorIndex++;
                }
            }

            // Put the resulting colors in 6 rectangles, their size differs per keyboard
            var rectWidth = MainManager.KeyboardManager.ActiveKeyboard.Width/3*Scale;
            var rectHeight = MainManager.KeyboardManager.ActiveKeyboard.Height/2*Scale;
            for (var row = 0; row < 2; row++)
            {
                for (var column = 0; column < 3; column++)
                {
                    var rectBase = new Point(rectWidth*column, rectHeight*row);
                    _rectangles.Add(new Rectangle(rectBase.X, rectBase.Y, rectWidth, rectHeight));
                }
            }
        }

        public override Bitmap GenerateBitmap()
        {
            var bitmap = MainManager.KeyboardManager.ActiveKeyboard.KeyboardBitmap(Scale);
            using (var g = Graphics.FromImage(bitmap))
            {
                var i = 0;
                foreach (var rectangle in _rectangles)
                {
                    g.FillRectangle(new SolidBrush(_colors[i]), rectangle);
                    i++;
                }
            }

            var test = new KalikoImage(bitmap);
            test.ApplyFilter(new GaussianBlurFilter(8f));
            var ms = new MemoryStream();
            test.SaveBmp(ms);
            ms.Position = 0;
            return new Bitmap(ms);
        }
    }
}