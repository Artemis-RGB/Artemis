using System;
using System.Windows.Media;
using MoonSharp.Interpreter;

namespace Artemis.Profiles.Lua.Modules.Brushes
{
    [MoonSharpUserData]
    public class LuaSolidColorBrush : LuaBrush
    {
        public LuaSolidColorBrush(Brush brush)
        {
            if (!(brush is SolidColorBrush))
                throw new ArgumentException("Brush type must be SolidColorBrush");

            Brush = brush;
        }

        public LuaSolidColorBrush(LuaColor luaColor)
        {
            Brush = new SolidColorBrush(luaColor.Color);
        }

        public LuaColor Color
        {
            get { return new LuaColor(((SolidColorBrush) Brush).Color); }
            set { Brush = new SolidColorBrush(value.Color); }
        }
    }
}