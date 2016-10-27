using System.Windows.Media;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

namespace Artemis.Profiles.Lua.Brushes
{
    [MoonSharpUserData]
    public class LuaRadialGradientBrush : LuaBrush
    {
        private RadialGradientBrush _brush;

        public LuaRadialGradientBrush(RadialGradientBrush radialGradientBrush)
        {
            Brush = radialGradientBrush;
        }

        [MoonSharpVisible(false)]
        public new RadialGradientBrush Brush
        {
            get { return _brush; }
            set
            {
                _brush = value;
                _brush.Freeze();
            }
        }

        public Table Colors
        {
            get { throw new System.NotImplementedException(); }
            set { throw new System.NotImplementedException(); }
        }
    }
}