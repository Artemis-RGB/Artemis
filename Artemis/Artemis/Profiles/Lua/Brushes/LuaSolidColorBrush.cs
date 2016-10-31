using System.Windows.Media;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

namespace Artemis.Profiles.Lua.Brushes
{
    [MoonSharpUserData]
    public class LuaSolidColorBrush : LuaBrush
    {
        private SolidColorBrush _brush;

        public LuaSolidColorBrush(LuaColor luaColor)
        {
            SolidColorBrush = new SolidColorBrush(luaColor.Color);
        }

        /// <summary>
        ///     The underlying brush
        /// </summary>
        [MoonSharpVisible(false)]
        public SolidColorBrush SolidColorBrush
        {
            get { return _brush; }
            set
            {
                _brush = value;
                _brush.Freeze();
                Brush = _brush;
            }
        }

        public LuaColor Color
        {
            get { return new LuaColor(SolidColorBrush.Color); }
            set { SolidColorBrush = new SolidColorBrush(value.Color); }
        }
    }
}