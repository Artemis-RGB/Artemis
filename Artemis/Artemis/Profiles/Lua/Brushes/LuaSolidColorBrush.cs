using System.Windows.Media;
using Artemis.Utilities;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

namespace Artemis.Profiles.Lua.Brushes
{
    [MoonSharpUserData]
    public class LuaSolidColorBrush : LuaBrush
    {
        private SolidColorBrush _brush;

        public LuaSolidColorBrush(SolidColorBrush solidColorBrush)
        {
            SolidColorBrush = solidColorBrush;
        }

        public LuaSolidColorBrush(string hexCode)
        {
            SolidColorBrush = new SolidColorBrush(new Color().FromHex(hexCode));
        }

        /// <summary>
        ///     The underlying brush
        /// </summary>
        [MoonSharpVisible(false)]
        public  SolidColorBrush SolidColorBrush
        {
            get { return _brush; }
            set
            {
                _brush = value;
                _brush.Freeze();
                Brush = _brush;
            }
        }

        /// <summary>
        ///     Gets or sets the brush's color using a hex notation
        /// </summary>
        public string Color
        {
            get { return SolidColorBrush.Color.ToHex(); }
            set { SolidColorBrush = new SolidColorBrush(new Color().FromHex(value)); }
        }
    }
}