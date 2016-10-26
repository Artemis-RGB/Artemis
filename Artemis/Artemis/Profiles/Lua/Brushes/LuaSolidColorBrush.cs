using System.Windows.Media;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

namespace Artemis.Profiles.Lua.Brushes
{
    [MoonSharpUserData]
    public class LuaSolidColorBrush : ILuaBrush
    {
        // ReSharper disable once SuggestBaseTypeForParameter
        public LuaSolidColorBrush(SolidColorBrush solidColorBrush)
        {
            Brush = solidColorBrush;
        }

        public LuaSolidColorBrush(string hexCode)
        {
            SetupBrush(hexCode);
        }

        public string HexCode
        {
            get
            {
                var c = ((SolidColorBrush) Brush).Color;
                return "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
            }
            set { SetupBrush(value); }
        }

        [MoonSharpVisible(false)]
        public Brush Brush { get; set; }

        private void SetupBrush(string hexCode)
        {
            var convertFromString = ColorConverter.ConvertFromString(hexCode);
            if (convertFromString != null)
            {
                var col = (Color) convertFromString;
                Brush = new SolidColorBrush(col);
                Brush.Freeze();
            }
            else
            {
                Brush = null;
            }
        }
    }
}