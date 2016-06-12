using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows;
using Artemis.Managers;
using Artemis.Models;
using Artemis.Models.Profiles;
using Artemis.Utilities;
using Brush = System.Windows.Media.Brush;

namespace Artemis.Modules.Effects.Bubbles
{
    public class BubblesModel : EffectModel
    {
        #region Properties & Fields

        private static readonly Random _random = new Random();

        private const int SCALE = 25;

        private readonly List<Bubble> _bubbles = new List<Bubble>();
        private Bitmap _bitmap;

        public BubblesSettings Settings { get; }

        #endregion

        #region Constructors

        public BubblesModel(MainManager mainManager, BubblesSettings settings)
            : base(mainManager, null)
        {
            Name = "Bubbles";
            Settings = settings;
            Initialized = false;
        }

        #endregion

        #region Methods

        public override void Enable()
        {
            Rect rect = MainManager.DeviceManager.ActiveKeyboard.KeyboardRectangle(SCALE);
            _bitmap = MainManager.DeviceManager.ActiveKeyboard.KeyboardBitmap(SCALE);

            for (int i = 0; i < Settings.BubbleCount; i++)
            {
                Color color = Settings.IsRandomColors ? ColorHelpers.GetRandomRainbowColor() : ColorHelpers.ToDrawingColor(Settings.BubbleColor);
                // -Settings.MoveSpeed because we want to spawn at least one move away from borders
                double initialPositionX = ((rect.Width - (Settings.BubbleSize * 2) - Settings.MoveSpeed) * _random.NextDouble()) + Settings.BubbleSize;
                double initialPositionY = ((rect.Height - (Settings.BubbleSize * 2) - Settings.MoveSpeed) * _random.NextDouble()) + Settings.BubbleSize;
                double initialDirectionX = (Settings.MoveSpeed * _random.NextDouble()) * (_random.Next(1) == 0 ? -1 : 1);
                double initialDirectionY = (Settings.MoveSpeed - Math.Abs(initialDirectionX)) * (_random.Next(1) == 0 ? -1 : 1);

                _bubbles.Add(new Bubble(color, Settings.BubbleSize, new System.Windows.Point(initialPositionX, initialPositionY), new Vector(initialDirectionX, initialDirectionY)));
            }

            Initialized = true;
        }

        public override void Dispose()
        {
            _bitmap?.Dispose();
            _bubbles.Clear();
            Initialized = false;
        }

        public override void Update()
        {
            Rect keyboardRectangle = MainManager.DeviceManager.ActiveKeyboard.KeyboardRectangle(SCALE);
            foreach (Bubble bubble in _bubbles)
            {
                if (Settings.IsShiftColors)
                    bubble.Color = ColorHelpers.ShiftColor(bubble.Color, Settings.IsRandomColors ? (int)Math.Round(Settings.ShiftColorSpeed * _random.NextDouble()) : Settings.ShiftColorSpeed);

                bubble.CheckCollision(keyboardRectangle);
                bubble.Move();
            }
        }

        public override void Render(out Bitmap keyboard, out Brush mouse, out Brush headset, bool renderMice, bool renderHeadsets)
        {
            keyboard = _bitmap;
            mouse = null;
            headset = null;
            
            using (Graphics g = Graphics.FromImage(keyboard))
            {
                g.Clear(Color.Transparent);
                g.SmoothingMode = SmoothingMode.None;

                foreach (Bubble bubble in _bubbles)
                    bubble.Draw(g);
            }
        }

        public override List<LayerModel> GetRenderLayers(bool renderMice, bool renderHeadsets)
        {
            return null;
        }

        #endregion
    }
}
