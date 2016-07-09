using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using Artemis.DeviceProviders.Logitech.Utilities;
using Artemis.Managers;
using Artemis.Models;
using Artemis.Profiles.Layers.Models;
using Artemis.Utilities;

namespace Artemis.Modules.Effects.TypeWave
{
    public class TypeWaveModel : EffectModel
    {
        private readonly List<Wave> _waves;
        private Color _randomColor;

        public TypeWaveModel(MainManager mainManager, TypeWaveSettings settings) : base(mainManager, null)
        {
            Name = "TypeWave";
            _waves = new List<Wave>();
            _randomColor = Color.Red;
            Settings = settings;
            Initialized = false;
        }

        public TypeWaveSettings Settings { get; set; }

        public override void Dispose()
        {
            Initialized = false;
            MainManager.KeyboardHook.KeyDownCallback -= KeyboardHookOnKeyDownCallback;
        }

        private void KeyboardHookOnKeyDownCallback(KeyEventArgs e)
        {
            // More than 25 waves is pointless
            if (_waves.Count >= 25)
                return;

            var keyMatch = KeyMap.UsEnglishOrionKeys.FirstOrDefault(k => k.KeyCode == e.KeyCode);
            if (keyMatch == null)
                return;

            _waves.Add(Settings.IsRandomColors
                ? new Wave(new Point(keyMatch.PosX*KeyboardScale, keyMatch.PosY*KeyboardScale), 0, _randomColor)
                : new Wave(new Point(keyMatch.PosX*KeyboardScale, keyMatch.PosY*KeyboardScale), 0,
                    ColorHelpers.ToDrawingColor(Settings.WaveColor)));
        }

        public override void Enable()
        {
            Initialized = false;

            // Listener won't start unless the effect is active
            MainManager.KeyboardHook.KeyDownCallback += KeyboardHookOnKeyDownCallback;

            Initialized = true;
        }

        public override void Update()
        {
            if (Settings.IsRandomColors)
                _randomColor = ColorHelpers.ShiftColor(_randomColor, 25);

            for (var i = 0; i < _waves.Count; i++)
            {
                // TODO: Get from settings
                var fps = 25;

                _waves[i].Size += Settings.SpreadSpeed*KeyboardScale;

                if (Settings.IsShiftColors)
                    _waves[i].Color = ColorHelpers.ShiftColor(_waves[i].Color, Settings.ShiftColorSpeed);

                var decreaseAmount = 255/(Settings.TimeToLive/fps);
                _waves[i].Color = Color.FromArgb(
                    _waves[i].Color.A - decreaseAmount, _waves[i].Color.R,
                    _waves[i].Color.G,
                    _waves[i].Color.B);

                if (_waves[i].Color.A >= decreaseAmount)
                    continue;

                _waves.RemoveAt(i);
                i--;
            }
        }

        public override List<LayerModel> GetRenderLayers(bool keyboardOnly)
        {
            return null;
        }

        public override void Render(RenderFrame frame, bool keyboardOnly)
        {
            if (_waves.Count == 0)
                return;

            // Don't want a for-each, collection is changed in different thread
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

                using (var g = Graphics.FromImage(frame.KeyboardBitmap))
                {
                    g.FillPath(pthGrBrush, path);
                    pthGrBrush.FocusScales = new PointF(0.3f, 0.8f);

                    g.FillPath(pthGrBrush, path);
                    g.DrawEllipse(new Pen(pthGrBrush, 1), _waves[i].Point.X - _waves[i].Size/2,
                        _waves[i].Point.Y - _waves[i].Size/2, _waves[i].Size, _waves[i].Size);
                }
            }
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