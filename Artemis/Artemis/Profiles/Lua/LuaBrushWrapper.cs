using Artemis.Profiles.Lua.Brushes;
using MoonSharp.Interpreter;

namespace Artemis.Profiles.Lua
{
    [MoonSharpUserData]
    public class LuaBrushWrapper
    {
        public static LuaSolidColorBrush GetSolidColorBrush(string hexCode)
        {
            return new LuaSolidColorBrush(hexCode);
        }

        public static LuaSolidColorBrush GetLinearGradientBrush(string hexCode)
        {
            return new LuaSolidColorBrush(hexCode);
        }

        public static LuaSolidColorBrush GetRadialGradientBrush(string hexCode)
        {
            return new LuaSolidColorBrush(hexCode);
        }
    }
}