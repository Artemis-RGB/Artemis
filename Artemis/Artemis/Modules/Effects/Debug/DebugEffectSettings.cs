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
        public bool Rotate { get; set; }
        public int Scale { get; set; }
        public LinearGradientMode Type { get; set; }

        public sealed override void Load()
        {
            ToDefault();
        }

        public sealed override void Save()
        {
        }

        public sealed override void ToDefault()
        {
            Width = 84;
            Height = 24;
            Scale = 4;
            Type = LinearGradientMode.Horizontal;
            Rotate = true;
        }
    }
}