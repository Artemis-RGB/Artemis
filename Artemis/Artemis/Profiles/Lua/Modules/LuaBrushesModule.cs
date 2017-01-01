using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Artemis.Managers;
using Artemis.Profiles.Lua.Modules.Brushes;
using Artemis.Utilities;
using MoonSharp.Interpreter;

namespace Artemis.Profiles.Lua.Modules
{
    [MoonSharpUserData]
    public class LuaBrushesModule : LuaModule
    {
        public LuaBrushesModule(LuaManager luaManager) : base(luaManager)
        {
        }

        public override string ModuleName => "Brushes";

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

        public LuaColor GetRelativeColor(Dictionary<LuaColor, double> gradientColors, double offset)
        {
            if (offset < 0)
                offset = 0;
            if (offset > 1)
                offset = 1;

            var gsc = new GradientStopCollection(gradientColors.Select(gc => new GradientStop(gc.Key.Color, gc.Value)));
            var b = gsc.First(w => Math.Abs(w.Offset - gsc.Min(m => m.Offset)) < 0.01);
            var a = gsc.First(w => Math.Abs(w.Offset - gsc.Max(m => m.Offset)) < 0.01);

            foreach (var gs in gsc)
            {
                if (gs.Offset < offset && gs.Offset > b.Offset)
                    b = gs;
                if (gs.Offset > offset && gs.Offset < a.Offset)
                    a = gs;
            }

            var color = new Color
            {
                ScA = (float) ((offset - b.Offset) * (a.Color.ScA - b.Color.ScA) / (a.Offset - b.Offset) + b.Color.ScA),
                ScR = (float) ((offset - b.Offset) * (a.Color.ScR - b.Color.ScR) / (a.Offset - b.Offset) + b.Color.ScR),
                ScG = (float) ((offset - b.Offset) * (a.Color.ScG - b.Color.ScG) / (a.Offset - b.Offset) + b.Color.ScG),
                ScB = (float) ((offset - b.Offset) * (a.Color.ScB - b.Color.ScB) / (a.Offset - b.Offset) + b.Color.ScB)
            };

            return new LuaColor(color);
        }
    }
}