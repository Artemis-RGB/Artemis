using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Artemis.KeyboardProviders.Logitech.Utilities;
using Artemis.Models;
using Artemis.Utilities;
using Gma.System.MouseKeyHook;

namespace Artemis.Modules.Effects.TypeWave
{
    public class TypeWaveModel : EffectModel
    {
        private readonly List<Wave> _waves;
        private Color _randomColor;

        public TypeWaveModel(MainModel mainModel, TypeWaveSettings settings) : base(mainModel)
        {
            Name = "TypeWave";
            _waves = new List<Wave>();
            _randomColor = Color.Red;
            Settings = settings;
        }

        public TypeWaveSettings Settings { get; set; }

        public override void Dispose()
        {
            MainModel.KeyboardHook.Unsubscribe(HandleKeypress);
        }

        public override void Enable()
        {
            // Listener won't start unless the effect is active
            MainModel.KeyboardHook.Subscribe(HandleKeypress);
        }

        public override void Update()
        {
            if (Settings.IsRandomColors)
                _randomColor = ColorHelpers.ShiftColor(_randomColor, 25);

            for (var i = 0; i < _waves.Count; i++)
            {
                // TODO: Get from settings
                var fps = 25;

                _waves[i].Size += Settings.SpreadSpeed;

                if (Settings.IsShiftColors)
                    _waves[i].Color = ColorHelpers.ShiftColor(_waves[i].Color, Settings.ShiftColorSpeed);

                var decreaseAmount = 255/(Settings.TimeToLive/fps);
                _waves[i].Color = Color.FromArgb(_waves[i].Color.A - decreaseAmount, _waves[i].Color.R, _waves[i].Color.G,
                    _waves[i].Color.B);

                if (_waves[i].Color.A >= decreaseAmount)
                    continue;

                _waves.RemoveAt(i);
                i--;
            }
        }

        public override Bitmap GenerateBitmap()
        {
            if (_waves.Count == 0)
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
                for (var i = 0; i < _waves.Count; i++)
                {
                    if (_waves[i].Size == 0)
                        continue;
                    var path = new GraphicsPath();
                    path.AddEllipse(_waves[i].Point.X - _waves[i].Size/2, _waves[i].Point.Y - _waves[i].Size/2,
                        _waves[i].Size, _waves[i].Size);

                    var pthGrBrush = new PathGradientBrush(path)
                    {
                        SurroundColors = new[] {_waves[i].Color},
                        CenterColor = Color.Transparent
                    };

                    g.FillPath(pthGrBrush, path);
                    pthGrBrush.FocusScales = new PointF(0.3f, 0.8f);

                    g.FillPath(pthGrBrush, path);
                    g.DrawEllipse(new Pen(pthGrBrush, 1), _waves[i].Point.X - _waves[i].Size/2,
                        _waves[i].Point.Y - _waves[i].Size/2, _waves[i].Size, _waves[i].Size);
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

            _waves.Add(Settings.IsRandomColors
                ? new Wave(new Point(keyMatch.PosX, keyMatch.PosY), 0, _randomColor)
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