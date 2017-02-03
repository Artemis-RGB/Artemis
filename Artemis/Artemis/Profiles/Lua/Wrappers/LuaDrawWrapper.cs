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
        private static readonly FontFamily Font = new FontFamily(new Uri("pack://application:,,,/"),
            "./resources/#Silkscreen");

        private readonly DrawingContext _ctx;
        private readonly int _scale;

        public LuaDrawWrapper(DrawingContext ctx, string updateType)
        {
            _ctx = ctx;
            _scale = updateType == "keyboard" ? 4 : 2;
        }

        public void DrawEllipse(LuaBrush luaBrush, double x, double y, double height, double width)
        {
            x *= _scale;
            y *= _scale;
            height *= _scale;
            width *= _scale;

            var center = new Point(x + width / 2, y + height / 2);
            _ctx.DrawEllipse(luaBrush.Brush, new Pen(), center, width, height);
        }

        public void DrawLine(LuaBrush luaBrush, double startX, double startY, double endX, double endY,
            double thickness)
        {
            startX *= _scale;
            startY *= _scale;
            endX *= _scale;
            endY *= _scale;
            thickness *= _scale;

            _ctx.DrawLine(new Pen(luaBrush.Brush, thickness), new Point(startX, startY), new Point(endX, endY));
        }

        public void DrawRectangle(LuaBrush luaBrush, double x, double y, double height, double width)
        {
            x *= _scale;
            y *= _scale;
            height *= _scale;
            width *= _scale;

            _ctx.DrawRectangle(luaBrush.Brush, new Pen(), new Rect(x, y, width, height));
        }

        public double DrawText(LuaBrush luaBrush, double x, double y, string text, int fontSize)
        {
            x *= _scale;
            y *= _scale;
            fontSize *= _scale;

            var typeFace = new Typeface(Font, new FontStyle(), new FontWeight(), new FontStretch());
            var formatted = new FormattedText(text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, typeFace,
                fontSize, luaBrush.Brush);

            _ctx.DrawText(formatted, new Point(x, y));
            return formatted.Width / _scale;
        }
    }
}