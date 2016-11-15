using System.Windows.Media;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

namespace Artemis.Profiles.Lua.Brushes
{
    [MoonSharpUserData]
    public abstract class LuaBrush
    {
        private Brush _brush;

        [MoonSharpVisible(false)]
        public Brush Brush
        {
            get { return _brush; }
            set
            {
                value.Freeze();
                _brush = value;
            }
        }
    }
}