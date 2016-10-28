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

        public LuaSolidColorBrush GetSolidColorBrush(string hexCode)
        {
            return new LuaSolidColorBrush(hexCode);
        }
        
        public LuaLinearGradientBrush GetLinearGradientBrush(Table gradientColors,
            double startX = 0.5, double startY = 0.0, double endX = 0.5, double endY = 1.0)
        {
            return new LuaLinearGradientBrush(_script, gradientColors, startX, startY, endX, endY);
        }

        // TODO: Check default values
        public LuaRadialGradientBrush GetRadialGradientBrush(Table gradientColors,
            double centerX = 0.5, double centerY = 0.5, double originX = 0.5, double originY = 0.5)
        {
            return new LuaRadialGradientBrush(_script, gradientColors, centerX, centerY, originX, originY);
        }
    }
}