using System.Windows.Media;
using Artemis.Utilities;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

namespace Artemis.Profiles.Lua.Modules.Brushes
{
    [MoonSharpUserData]
    public class LuaColor
    {
        public LuaColor(Color color)
        {
            Color = color;
        }

        public LuaColor(string hexCode)
        {
            Color = new Color().FromHex(hexCode);
        }

        public LuaColor(byte a, byte r, byte g, byte b)
        {
            Color = Color.FromArgb(a, r, g, b);
        }

        [MoonSharpVisible(false)]
        public Color Color { get; set; }

        public string HexCode
        {
            get { return Color.ToHex(); }
            set { Color = Color.FromHex(value); }
        }

        public byte A
        {
            get { return Color.A; }
            set { Color = Color.FromArgb(value, R, G, B); }
        }

        public byte R
        {
            get { return Color.R; }
            set { Color = Color.FromArgb(A, value, G, B); }
        }

        public byte G
        {
            get { return Color.G; }
            set { Color = Color.FromArgb(A, R, value, B); }
        }

        public byte B
        {
            get { return Color.B; }
            set { Color = Color.FromArgb(A, R, G, value); }
        }
    }
}