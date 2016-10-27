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
            Brush = solidColorBrush;
        }

        public LuaSolidColorBrush(string hexCode)
        {
            Brush = new SolidColorBrush(new Color().FromHex(hexCode));
        }

        /// <summary>
        ///     The underlying brush
        /// </summary>
        [MoonSharpVisible(false)]
        public new SolidColorBrush Brush
        {
            get { return _brush; }
            set
            {
                _brush = value;
                _brush.Freeze();
            }
        }

        /// <summary>
        ///     Gets or sets the brush's color using a hex notation
        /// </summary>
        public string Color
        {
            get { return Brush.Color.ToHex(); }
            set { Brush = new SolidColorBrush(new Color().FromHex(value)); }
        }
    }
}