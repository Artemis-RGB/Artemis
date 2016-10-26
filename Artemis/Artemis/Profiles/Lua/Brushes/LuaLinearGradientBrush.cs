using System.Windows.Media;
using MoonSharp.Interpreter;

namespace Artemis.Profiles.Lua.Brushes
{
    [MoonSharpUserData]
    public class LuaLinearGradientBrush : ILuaBrush
    {
        // ReSharper disable once SuggestBaseTypeForParameter
        public LuaLinearGradientBrush(LinearGradientBrush linearGradientBrush)
        {
            Brush = linearGradientBrush;
        }

        private void SetupBrush()
        {
            // TODO: Convert array of hex code and offset to gradient stop collection
            var gradientStop = new GradientStop();
            var collection = new GradientStopCollection();
            Brush = new LinearGradientBrush();
        }

        public Brush Brush { get; set; }
    }
}