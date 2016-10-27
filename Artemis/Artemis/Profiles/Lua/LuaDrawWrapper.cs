using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using Artemis.Profiles.Lua.Brushes;
using MoonSharp.Interpreter;
using Pen = System.Windows.Media.Pen;

namespace Artemis.Profiles.Lua
{
    [MoonSharpUserData]
    public class LuaDrawWrapper
    {
        private readonly DrawingContext _ctx;

        public LuaDrawWrapper(DrawingContext ctx)
        {
            _ctx = ctx;
        }

        public void DrawCircle(LuaBrush luaBrush, double x, double y, double height, double width)
        {
            var center = new Point(x + width/2, y + height/2);
            _ctx.DrawEllipse(luaBrush.Brush, new Pen(), center, width, height);
        }
    }
}