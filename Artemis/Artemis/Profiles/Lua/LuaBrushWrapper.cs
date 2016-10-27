using System.Windows.Media;
using Artemis.Profiles.Lua.Brushes;
using MoonSharp.Interpreter;

namespace Artemis.Profiles.Lua
{
    [MoonSharpUserData]
    public class LuaBrushWrapper
    {
        private readonly Script _script;

        public LuaBrushWrapper(Script script)
        {
            _script = script;
        }

        public LuaRadialGradientBrush GetSolidColorBrush(string hexCode)
        {
            return new LuaRadialGradientBrush(new RadialGradientBrush());
        }

        public LuaLinearGradientBrush GetLinearGradientBrush(Table gradientColors,
            double startX = 0.5, double startY = 0.0, double endX = 0.5, double endY = 1.0)
        {
            return new LuaLinearGradientBrush(_script, gradientColors, startX, startY, endX, endY);
        }

        public LuaSolidColorBrush GetRadialGradientBrush(string hexCode)
        {
            return new LuaSolidColorBrush(hexCode);
        }
    }
}