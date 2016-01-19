using System.Drawing.Drawing2D;
using Artemis.Models;

namespace Artemis.Modules.Effects.Debug
{
    internal class DebugEffectSettings : EffectSettings
    {
        public DebugEffectSettings()
        {
            Load();
        }

        public int Width { get; set; }
        public int Height { get; set; }
        public int Spread { get; set; }
        public bool Rotate { get; set; }
        public LinearGradientMode Type { get; set; }

        public override sealed void Load()
        {
            ToDefault();
        }

        public override sealed void Save()
        {
        }

        public override sealed void ToDefault()
        {
            Width = 6;
            Height = 100;
            Spread = 0;
            Type = LinearGradientMode.Horizontal;
            Rotate = false;
        }
    }
}