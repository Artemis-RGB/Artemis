using System.Windows.Media;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

namespace Artemis.Profiles.Lua.Brushes
{
    [MoonSharpUserData]
    public abstract class LuaBrush
    {
        [MoonSharpVisible(false)]
        public virtual Brush Brush { get; set; }
    }
}