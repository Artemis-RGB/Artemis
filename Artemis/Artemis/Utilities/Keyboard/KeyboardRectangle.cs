using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Artemis.Utilities.Keyboard
{
    public class KeyboardRectangle
    {
        private readonly BackgroundWorker _blinkWorker = new BackgroundWorker {WorkerSupportsCancellation = true};
        private int _blinkDelay;
        private List<Color> _colors;
        private double _rotationProgress;

        /// <summary>
        ///     Represents a Rectangle on the keyboard which can be drawn to a Bitmap
        /// </summary>
        /// <param name="scale">The scale on which the rect should be rendered (Higher means smoother rotation)</param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="colors">An array of colors the ColorBlend will use</param>
        /// <param name="gradientMode"></param>
        public KeyboardRectangle(int scale, int x, int y, int width, int height, List<Color> colors,
            LinearGradientMode gradientMode)
        {
            Scale = scale;
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Colors = colors;
            GradientMode = gradientMode;

            Opacity = 255;
            Rotate = false;
            LoopSpeed = 1;
            Visible = true;

            _rotationProgress = 0;
            _blinkWorker.DoWork += BlinkWorker_DoWork;
        }

        public int Scale { get; set; }
        public byte Opacity { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

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

        public LinearGradientMode GradientMode { get; set; }

        public bool Rotate { get; set; }
        public double LoopSpeed { get; set; }

        public bool Visible { get; set; }

        public KeyboardRectangle Clone()
        {
            return (KeyboardRectangle) MemberwiseClone();
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
            if (!Visible || Height < 1 || Width < 1 || Colors.Count < 1)
                return;

            var colorBlend = new ColorBlend {Colors = Colors.ToArray()};
            if (Opacity < 255)
                for (var i = 0; i < colorBlend.Colors.Length; i++)
                    colorBlend.Colors[i] = Color.FromArgb(Opacity, colorBlend.Colors[i]);

            var devider = (float) Colors.Count - 1;
            var positions = new List<float>();
            for (var i = 0; i < Colors.Count; i++)
                positions.Add(i/devider);

            colorBlend.Positions = positions.ToArray();

            var baseRect = new Rectangle(X, Y, Width, Height);
            var brushRect = new Rectangle((int) _rotationProgress, Y, baseRect.Width*2, baseRect.Height*2);
            LinearGradientBrush baseBrush;
            if (Colors.Count > 5)
                baseBrush = new LinearGradientBrush(brushRect, Colors.First(), Colors.Skip(1).FirstOrDefault(),
                    GradientMode) {InterpolationColors = colorBlend};
            else
                baseBrush = new LinearGradientBrush(baseRect, Colors[0], Colors[1], GradientMode);

            g.FillRectangle(baseBrush, baseRect);
            if (!Rotate)
                return;

            _rotationProgress = _rotationProgress + LoopSpeed;
            if (_rotationProgress > Width)
                _rotationProgress = LoopSpeed;
        }
    }
}