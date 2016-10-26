using System.Windows.Media;
using MoonSharp.Interpreter;

namespace Artemis.Profiles.Lua.Brushes
{
    [MoonSharpUserData]
    public class LuaRadialGradientBrush : ILuaBrush
    {
        // ReSharper disable once SuggestBaseTypeForParameter
        public LuaRadialGradientBrush(RadialGradientBrush radialGradientBrush)
        {
            Brush = radialGradientBrush;
        }

        public Brush Brush { get; set; }
    }
}