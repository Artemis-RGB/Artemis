using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using Artemis.DAL;
using Artemis.Managers;
using Artemis.Modules.Abstract;
using Artemis.Utilities;
using Point = System.Windows.Point;

namespace Artemis.Modules.General.Bubbles
{
    public class BubblesModel : ModuleModel
    {
        #region Constructors

        public BubblesModel(DeviceManager deviceManager, LuaManager luaManager) : base(deviceManager, luaManager)
        {
            Settings = SettingsProvider.Load<BubblesSettings>();
        }

        #endregion

        #region Properties & Fields

        public override string Name => "Bubbles";
        public override bool IsOverlay => false;
        public override bool IsBoundToProcess => false;

        private static readonly Random Random = new Random();
        private readonly List<Bubble> _bubbles = new List<Bubble>();
        private int _scale;

        #endregion

        #region Methods

        public override void Enable()
        {
            var bubbleSettings = (BubblesSettings) Settings;
            _scale = bubbleSettings.Smoothness;

            var rect = DeviceManager.ActiveKeyboard.KeyboardRectangle(_scale);
            var scaleFactor = bubbleSettings.Smoothness / 25.0;

            for (var i = 0; i < bubbleSettings.BubbleCount; i++)
            {
                var color = bubbleSettings.IsRandomColors
                    ? ColorHelpers.GetRandomRainbowColor()
                    : ColorHelpers.ToDrawingColor(bubbleSettings.BubbleColor);
                // -bubbleSettings.MoveSpeed because we want to spawn at least one move away from borders
                var initialPositionX = (rect.Width - bubbleSettings.BubbleSize * scaleFactor * 2 -
                                        bubbleSettings.MoveSpeed * scaleFactor) *
                                       Random.NextDouble() + bubbleSettings.BubbleSize * scaleFactor;
                var initialPositionY = (rect.Height - bubbleSettings.BubbleSize * scaleFactor * 2 -
                                        bubbleSettings.MoveSpeed * scaleFactor) *
                                       Random.NextDouble() + bubbleSettings.BubbleSize * scaleFactor;
                var initialDirectionX = bubbleSettings.MoveSpeed * scaleFactor * Random.NextDouble() *
                                        (Random.Next(1) == 0 ? -1 : 1);
                var initialDirectionY = (bubbleSettings.MoveSpeed * scaleFactor - Math.Abs(initialDirectionX)) *
                                        (Random.Next(1) == 0 ? -1 : 1);

                _bubbles.Add(new Bubble(color, (int) Math.Round(bubbleSettings.BubbleSize * scaleFactor),
                    new Point(initialPositionX, initialPositionY), new Vector(initialDirectionX, initialDirectionY)));
            }

            IsInitialized = true;
        }

        public override void Dispose()
        {
            _bubbles.Clear();
            IsInitialized = false;
        }

        public override void Update()
        {
            var bubbleSettings = (BubblesSettings) Settings;
            var keyboardRectangle = DeviceManager.ActiveKeyboard.KeyboardRectangle(_scale);
            foreach (var bubble in _bubbles)
            {
                if (bubbleSettings.IsShiftColors)
                    bubble.Color = ColorHelpers.ShiftColor(bubble.Color,
                        bubbleSettings.IsRandomColors
                            ? (int) Math.Round(bubbleSettings.ShiftColorSpeed * Random.NextDouble())
                            : bubbleSettings.ShiftColorSpeed);

                bubble.CheckCollision(keyboardRectangle);
                bubble.Move();
            }
        }

        public override void Render(RenderFrame frame, bool keyboardOnly)
        {
            using (var g = Graphics.FromImage(frame.KeyboardBitmap))
            {
                foreach (var bubble in _bubbles)
                    bubble.Draw(g);
            }
        }

        #endregion
    }
}