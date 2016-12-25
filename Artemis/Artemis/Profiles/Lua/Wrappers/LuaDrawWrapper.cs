using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using Artemis.Profiles.Lua.Modules.Brushes;
using MoonSharp.Interpreter;

namespace Artemis.Profiles.Lua.Wrappers
{
    /// <summary>
    ///     A wrapper that is provided to each OnDraw event to allow drawing in LUA
    /// </summary>
    [MoonSharpUserData]
    public class LuaDrawWrapper
    {
        private readonly DrawingContext _ctx;
        private readonly FontFamily _font;

        public LuaDrawWrapper(DrawingContext ctx)
        {
            _ctx = ctx;
            _font = new FontFamily(new Uri("pack://application:,,,/"), "./resources/#Silkscreen");
        }

        public void DrawEllipse(LuaBrush luaBrush, double x, double y, double height, double width)
        {
            x *= 4;
            y *= 4;
            height *= 4;
            width *= 4;

            var center = new Point(x + width / 2, y + height / 2);
            _ctx.DrawEllipse(luaBrush.Brush, new Pen(), center, width, height);
        }

        public void DrawLine(LuaBrush luaBrush, double startX, double startY, double endX, double endY,
            double thickness)
        {
            startX *= 4;
            startY *= 4;
            endX *= 4;
            endY *= 4;
            thickness *= 4;

            _ctx.DrawLine(new Pen(luaBrush.Brush, thickness), new Point(startX, startY), new Point(endX, endY));
        }

        public void DrawRectangle(LuaBrush luaBrush, double x, double y, double height, double width)
        {
            x *= 4;
            y *= 4;
            height *= 4;
            width *= 4;

            _ctx.DrawRectangle(luaBrush.Brush, new Pen(), new Rect(x, y, width, height));
        }

        public double DrawText(LuaBrush luaBrush, double x, double y, string text, int fontSize)
        {
            x *= 4;
            y *= 4;
            fontSize *= 4;

            var typeFace = new Typeface(_font, new FontStyle(), new FontWeight(), new FontStretch());
            var formatted = new FormattedText(text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, typeFace,
                fontSize, luaBrush.Brush);

            _ctx.DrawText(formatted, new Point(x, y));
            return formatted.Width / 4;
        }
    }
}