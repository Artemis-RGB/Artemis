using System.Collections.Generic;
using Artemis.Utilities;
using MoonSharp.Interpreter;

namespace Artemis.Profiles.Lua.Brushes
{
    [MoonSharpUserData]
    public class LuaBrushWrapper
    {
        public LuaColor GetColor(string hexCode)
        {
            return new LuaColor(hexCode);
        }

        public LuaColor GetColor(byte a, byte r, byte g, byte b)
        {
            return new LuaColor(a, r, g, b);
        }

        public LuaColor GetRandomColor()
        {
            return new LuaColor(ColorHelpers.GetRandomRainbowMediaColor());
        }

        public LuaSolidColorBrush GetSolidColorBrush(LuaColor color)
        {
            return new LuaSolidColorBrush(color);
        }

        public LuaLinearGradientBrush GetLinearGradientBrush(Dictionary<LuaColor, double> gradientColors,
            double startX = 0.5, double startY = 0.0, double endX = 0.5, double endY = 1.0)
        {
            return new LuaLinearGradientBrush(gradientColors, startX, startY, endX, endY);
        }

        // TODO: Check default values
        public LuaRadialGradientBrush GetRadialGradientBrush(Dictionary<LuaColor, double> gradientColors,
            double centerX = 0.5, double centerY = 0.5, double originX = 0.5, double originY = 0.5)
        {
            return new LuaRadialGradientBrush(gradientColors, centerX, centerY, originX, originY);
        }
    }
}