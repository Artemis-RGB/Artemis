using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Artemis.KeyboardProviders;

namespace Artemis.Utilities.Keyboard
{
    public class KeyboardRectangle
    {
        private readonly BackgroundWorker _blinkWorker = new BackgroundWorker {WorkerSupportsCancellation = true};
        private readonly KeyboardProvider _keyboard;
        private int _blinkDelay;
        private List<Color> _colors;
        private double _rotationProgress;

        /// <summary>
        ///     Represents a Rectangle on the keyboard which can be drawn to a Bitmap.
        ///     By default, a rectangle is the entire keyboard's size.
        /// </summary>
        /// <param name="keyboard">The keyboard this rectangle will be used for</param>
        /// <param name="scale">The scale on which the rect should be rendered (Higher means smoother rotation)</param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="colors">An array of colors the ColorBlend will use</param>
        /// <param name="gradientMode"></param>
        public KeyboardRectangle(KeyboardProvider keyboard, int scale, int x, int y, List<Color> colors,
            LinearGradientMode gradientMode)
        {
            _keyboard = keyboard;
            Scale = scale;
            X = x;
            Y = y;
            Width = keyboard.Width*Scale;
            Height = keyboard.Height*Scale;
            Colors = colors;
            GradientMode = gradientMode;

            Opacity = 255;
            Rotate = false;
            LoopSpeed = 1;
            Visible = true;
            ContainedBrush = false;

            _rotationProgress = 0;
            _blinkWorker.DoWork += BlinkWorker_DoWork;
        }

        public bool ContainedBrush { get; set; }
        public int Scale { get; set; }
        public byte Opacity { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public LinearGradientMode GradientMode { get; set; }
        public bool Rotate { get; set; }
        public double LoopSpeed { get; set; }
        public bool Visible { get; set; }

        public List<Color> Colors
        {
            get { return _colors; }
            set
            {
                _colors = value;

                // Make the list tilable so that we can loop it
                _colors.AddRange(value);
                _colors.Add(value.FirstOrDefault());
            }
        }

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
            var rect = ContainedBrush
                ? new Rectangle((int) _rotationProgress, Y, Width, Height)
                : new Rectangle((int) _rotationProgress, 0, _keyboard.Width*Scale, _keyboard.Height*Scale);

            if (Colors.Count > 5)
                return new LinearGradientBrush(rect, Colors[0], Colors[1], GradientMode)
                {
                    InterpolationColors = colorBlend
                };

            return Colors.Count > 1
                ? new LinearGradientBrush(rect, Colors[0], Colors[1], GradientMode)
                : new LinearGradientBrush(rect, Colors[0], Colors[0], GradientMode);
        }

        private ColorBlend CreateColorBlend()
        {
            var colorBlend = new ColorBlend {Colors = Colors.ToArray()};

            // If needed, apply opacity to the colors in the blend
            if (Opacity < 255)
                for (var i = 0; i < colorBlend.Colors.Length; i++)
                    colorBlend.Colors[i] = Color.FromArgb(Opacity, colorBlend.Colors[i]);

            // Devide the colors over the colorblend
            var devider = (float) Colors.Count - 1;
            var positions = new List<float>();
            for (var i = 0; i < Colors.Count; i++)
                positions.Add(i/devider);

            // Apply the devided positions
            colorBlend.Positions = positions.ToArray();

            return colorBlend;
        }
    }
}