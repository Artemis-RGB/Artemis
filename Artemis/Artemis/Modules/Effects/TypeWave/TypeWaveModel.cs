using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Artemis.KeyboardProviders.Logitech.Utilities;
using Artemis.Models;
using Artemis.Utilities;
using Open.WinKeyboardHook;

namespace Artemis.Modules.Effects.TypeWave
{
    public class TypeWaveModel : EffectModel
    {
        public TypeWaveModel(MainModel mainModel, TypeWaveSettings settings) : base(mainModel)
        {
            Name = "TypeWave";
            Waves = new List<Wave>();
            Settings = settings;

            // KeyboardIntercepter won't start untill the effect is active
            KeyboardInterceptor = new KeyboardInterceptor();
        }

        public TypeWaveSettings Settings { get; set; }
        public List<Wave> Waves { get; set; }
        public KeyboardInterceptor KeyboardInterceptor { get; set; }

        public override void Dispose()
        {
            KeyboardInterceptor.KeyUp -= HandleKeypress;
            KeyboardInterceptor.StopCapturing();
        }

        public override void Enable()
        {
            KeyboardInterceptor.StartCapturing();
            KeyboardInterceptor.KeyUp += HandleKeypress;
        }

        public override void Update()
        {
            for (var i = 0; i < Waves.Count; i++)
            {
                // TODO: Get from settings
                var fps = 25;

                Waves[i].Size += Settings.SpreadSpeed;

                if (Settings.IsShiftColors)
                    Waves[i].Color = ColorHelpers.ShiftColor(Waves[i].Color, Settings.ShiftColorSpeed);

                var decreaseAmount = 255/(Settings.TimeToLive/fps);
                Waves[i].Color = Color.FromArgb(Waves[i].Color.A - decreaseAmount, Waves[i].Color.R, Waves[i].Color.G,
                    Waves[i].Color.B);

                if (Waves[i].Color.A >= decreaseAmount)
                    continue;

                Waves.RemoveAt(i);
                i--;
            }
        }

        public override Bitmap GenerateBitmap()
        {
            if (Waves.Count == 0)
                return null;

            var bitmap = new Bitmap(21, 6);
            using (var g = Graphics.FromImage(bitmap))
            {
                // TODO: Might implement a user-defined background color, but looks ugly most of the time
                //g.Clear(Color.FromArgb(100, 255, 0, 255));
                g.Clear(Color.Transparent);
                g.SmoothingMode = SmoothingMode.HighQuality;

                // Don't want a foreach, collection is changed in different thread
                // ReSharper disable once ForCanBeConvertedToForeach
                for (var i = 0; i < Waves.Count; i++)
                {
                    if (Waves[i].Size == 0)
                        continue;
                    var path = new GraphicsPath();
                    path.AddEllipse(Waves[i].Point.X - Waves[i].Size/2, Waves[i].Point.Y - Waves[i].Size/2,
                        Waves[i].Size, Waves[i].Size);

                    var pthGrBrush = new PathGradientBrush(path)
                    {
                        SurroundColors = new[] {Waves[i].Color},
                        CenterColor = Color.Transparent
                    };

                    g.FillPath(pthGrBrush, path);
                    pthGrBrush.FocusScales = new PointF(0.3f, 0.8f);

                    g.FillPath(pthGrBrush, path);
                    g.DrawEllipse(new Pen(pthGrBrush, 1), Waves[i].Point.X - Waves[i].Size/2,
                        Waves[i].Point.Y - Waves[i].Size/2, Waves[i].Size, Waves[i].Size);
                }
            }
            return bitmap;
        }

        private void HandleKeypress(object sender, KeyEventArgs e)
        {
            Task.Factory.StartNew(() => KeyPressTask(e));
        }

        private void KeyPressTask(KeyEventArgs e)
        {
            var keyMatch = KeyMap.UsEnglishOrionKeys.FirstOrDefault(k => k.KeyCode == e.KeyCode);
            if (keyMatch == null)
                return;

            Waves.Add(Settings.IsRandomColors
                ? new Wave(new Point(keyMatch.PosX, keyMatch.PosY), 0, ColorHelpers.GetRandomRainbowColor())
                : new Wave(new Point(keyMatch.PosX, keyMatch.PosY), 0,
                    ColorHelpers.ToDrawingColor(Settings.WaveColor)));
        }
    }

    public class Wave
    {
        public Wave(Point point, int size, Color color)
        {
            Point = point;
            Size = size;
            Color = color;
        }

        public Point Point { get; set; }
        public int Size { get; set; }
        public Color Color { get; set; }
    }
}