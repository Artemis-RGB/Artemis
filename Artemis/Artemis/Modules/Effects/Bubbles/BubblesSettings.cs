using System.Windows.Media;
using Artemis.Models;

namespace Artemis.Modules.Effects.Bubbles
{
    public class BubblesSettings : EffectSettings
    {
        public BubblesSettings()
        {
            Load();
        }

        public bool IsRandomColors { get; set; }
        public Color BubbleColor { get; set; }
        public bool IsShiftColors { get; set; }
        public int ShiftColorSpeed { get; set; }
        public int BubbleSize { get; set; }
        public int MoveSpeed { get; set; }
        public int BubbleCount { get; set; }

        public sealed override void Load()
        {
            IsRandomColors = Bubbles.Default.IsRandomColors;
            BubbleColor = Bubbles.Default.BubbleColor;
            IsShiftColors = Bubbles.Default.IsShiftColors;
            ShiftColorSpeed = Bubbles.Default.ShiftColorSpeed;
            BubbleSize = Bubbles.Default.BubbleSize;
            MoveSpeed = Bubbles.Default.MoveSpeed;
            BubbleCount = Bubbles.Default.BubbleCount;
        }

        public sealed override void Save()
        {
            Bubbles.Default.IsRandomColors = IsRandomColors;
            Bubbles.Default.BubbleColor = BubbleColor;
            Bubbles.Default.IsShiftColors = IsShiftColors;
            Bubbles.Default.ShiftColorSpeed = ShiftColorSpeed;
            Bubbles.Default.BubbleSize = BubbleSize;
            Bubbles.Default.MoveSpeed = MoveSpeed;
            Bubbles.Default.BubbleCount = BubbleCount;

            Bubbles.Default.Save();
        }

        public sealed override void ToDefault()
        {
            IsRandomColors = true;
            BubbleColor = Color.FromArgb(255, 255, 0, 0);
            IsShiftColors = true;
            ShiftColorSpeed = 12;
            BubbleSize = 25;
            MoveSpeed = 4;
            BubbleCount = 14;
        }
    }
}
