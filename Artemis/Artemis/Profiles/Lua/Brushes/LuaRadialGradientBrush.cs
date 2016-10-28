using System.Windows;
using System.Windows.Media;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

namespace Artemis.Profiles.Lua.Brushes
{
    [MoonSharpUserData]
    public class LuaRadialGradientBrush : LuaBrush
    {
        private readonly Script _script;
        private RadialGradientBrush _brush;

        public LuaRadialGradientBrush(Script script, RadialGradientBrush radialGradientBrush)
        {
            _script = script;
            RadialGradientBrush = radialGradientBrush;
        }

        public LuaRadialGradientBrush(Script script, Table gradientColors,
            double centerX, double centerY, double originX, double originY)
        {
            _script = script;
            SetupBrush(gradientColors, centerX, centerY, originX, originY);
        }

        /// <summary>
        ///     The underlying brush
        /// </summary>
        [MoonSharpVisible(false)]
        public RadialGradientBrush RadialGradientBrush
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
        ///     Gets or sets the Brush's GradientStops using a LUA table
        /// </summary>
        public Table Colors
        {
            get { return LuaLinearGradientBrush.CreateGradientTable(_script, RadialGradientBrush.GradientStops); }
            set
            {
                var updatedBrush = RadialGradientBrush.CloneCurrentValue();
                updatedBrush.GradientStops = LuaLinearGradientBrush.CreateGradientCollection(value);
                RadialGradientBrush = updatedBrush;
            }
        }

        /// <summary>
        ///     Configures the brush according to the provided values usable in LUA
        /// </summary>
        /// <param name="gradientColors"></param>
        /// <param name="centerX"></param>
        /// <param name="centerY"></param>
        /// <param name="originX"></param>
        /// <param name="originY"></param>
        private void SetupBrush(Table gradientColors, double centerX, double centerY, double originX, double originY)
        {
            var collection = LuaLinearGradientBrush.CreateGradientCollection(gradientColors);
            RadialGradientBrush = new RadialGradientBrush(collection)
            {
                Center = new Point(centerX, centerY),
                GradientOrigin = new Point(originX, originY)
            };
        }
    }
}