using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Artemis.DeviceProviders;

namespace Artemis.Utilities.Keyboard
{
    // TODO: Obsolete
    public class KeyboardRectangle
    {
        private readonly BackgroundWorker _blinkWorker = new BackgroundWorker {WorkerSupportsCancellation = true};
        private readonly KeyboardProvider _keyboard;
        private int _blinkDelay;
        private double _rotationProgress;

        /// <summary>
        ///     Represents a Rectangle on the keyboard which can be drawn to a Bitmap.
        ///     By default, a rectangle is the entire keyboard's size.
        /// </summary>
        /// <param name="keyboard">The keyboard this rectangle will be used for</param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="colors">An array of colors the ColorBlend will use</param>
        /// <param name="gradientMode"></param>
        public KeyboardRectangle(KeyboardProvider keyboard, int x, int y, List<Color> colors,
            LinearGradientMode gradientMode)
        {
            _keyboard = keyboard;
            _rotationProgress = 0;
            _blinkWorker.DoWork += BlinkWorker_DoWork;

            Scale = 4;
            X = x;
            Y = y;
            Width = keyboard.Width*Scale;
            Height = keyboard.Height*Scale;
            Visible = true;
            Opacity = 255;

            ContainedBrush = true;
            GradientMode = gradientMode;
            Rotate = false;
            LoopSpeed = 1;
            Colors = colors;
        }

        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool Visible { get; set; }
        public byte Opacity { get; set; } // TODO: Remove

        /// <summary>
        ///     Sets wether or not the colors should be contained within the rectangle, or span the entire keyboard.
        /// </summary>
        public bool ContainedBrush { get; set; }

        /// <summary>
        ///     Used when ContainedBrush is set to false to make sure the colors span the entire keyboard on a higher scale.
        /// </summary>
        public int Scale { get; set; }

        /// <summary>
        ///     Determines what grientmode to use in the LinearGradientBrush.
        /// </summary>
        public LinearGradientMode GradientMode { get; set; }

        /// <summary>
        ///     Wether or not to rotate the colors over the brush.
        /// </summary>
        public bool Rotate { get; set; }

        /// <summary>
        ///     What speed to ratate the colors on.
        /// </summary>
        public double LoopSpeed { get; set; }

        /// <summary>
        ///     Colors used on the brush.
        /// </summary>
        public List<Color> Colors { get; set; }

        public void StartBlink(int delay)
        {
            _blinkDelay = delay;

            if (!_blinkWorker.IsBusy)
                _blinkWorker.RunWorkerAsync();
        }

        public void StartBlink(int delay, int time)
        {
            StartBlink(delay);
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(delay);
                StopBlink();
            });
        }

        public void StopBlink()
        {
            if (_blinkWorker.IsBusy)
                _blinkWorker.CancelAsync();
        }

        private void BlinkWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!_blinkWorker.CancellationPending)
            {
                Thread.Sleep(_blinkDelay);
                Visible = !Visible;
            }
            Visible = true;
        }

        public void Draw(Graphics g)
        {
            if (!Visible || Height < 1 || Width < 1 || !Colors.Any())
                return;

            var brush = CreateBrush();
            var baseRect = new Rectangle(X, Y, Width, Height);

            g.FillRectangle(brush, baseRect);
            if (!Rotate)
                return;

            _rotationProgress = _rotationProgress + LoopSpeed;
            if (ContainedBrush && _rotationProgress > Width)
                _rotationProgress = LoopSpeed;
            else if (!ContainedBrush && _rotationProgress > _keyboard.Width*Scale)
                _rotationProgress = LoopSpeed;
        }

        private LinearGradientBrush CreateBrush()
        {
            var colorBlend = CreateColorBlend();
            RectangleF rect;
            if (Rotate)
                rect = ContainedBrush
                    ? new Rectangle((int) _rotationProgress, Y, Width*2, Height*2)
                    : new Rectangle((int) _rotationProgress, 0, _keyboard.Width*Scale*2, _keyboard.Height*Scale*2);
            else
                rect = ContainedBrush
                    ? new Rectangle(X, Y, Width, Height)
                    : new Rectangle(0, 0, _keyboard.Width*Scale, _keyboard.Height*Scale);

            return new LinearGradientBrush(rect, Color.Transparent, Color.Transparent, GradientMode)
            {
                InterpolationColors = colorBlend
            };
        }

        private ColorBlend CreateColorBlend()
        {
            if (Colors.Count == 1)
                return new ColorBlend {Colors = new[] {Colors[0], Colors[0]}, Positions = new[] {0F, 1F}};
            var colorBlend = Rotate
                ? new ColorBlend {Colors = CreateTilebleColors(Colors).ToArray()}
                : new ColorBlend {Colors = Colors.ToArray()};

            // If needed, apply opacity to the colors in the blend
            if (Opacity < 255)
                for (var i = 0; i < colorBlend.Colors.Length; i++)
                    colorBlend.Colors[i] = Color.FromArgb(Opacity, colorBlend.Colors[i]);

            // Devide the colors over the colorblend
            var devider = (float) colorBlend.Colors.Length - 1;
            var positions = new List<float>();
            for (var i = 0; i < colorBlend.Colors.Length; i++)
                positions.Add(i/devider);

            // Apply the devided positions
            colorBlend.Positions = positions.ToArray();
            return colorBlend;
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