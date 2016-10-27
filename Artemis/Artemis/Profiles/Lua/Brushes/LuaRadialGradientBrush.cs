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
            Brush = radialGradientBrush;
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
        public new RadialGradientBrush Brush
        {
            get { return _brush; }
            set
            {
                _brush = value;
                _brush.Freeze();
            }
        }

        /// <summary>
        ///     Gets or sets the Brush's GradientStops using a LUA table
        /// </summary>
        public Table Colors
        {
            get { return LuaLinearGradientBrush.CreateGradientTable(_script, Brush.GradientStops); }
            set
            {
                var updatedBrush = Brush.CloneCurrentValue();
                updatedBrush.GradientStops = LuaLinearGradientBrush.CreateGradientCollection(value);
                Brush = updatedBrush;
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
            Brush = new RadialGradientBrush(collection)
            {
                Center = new Point(centerX, centerY),
                GradientOrigin = new Point(originX, originY)
            };
        }
    }
}