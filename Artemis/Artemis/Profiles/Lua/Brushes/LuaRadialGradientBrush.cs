using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

namespace Artemis.Profiles.Lua.Brushes
{
    [MoonSharpUserData]
    public class LuaRadialGradientBrush : LuaBrush
    {
        private RadialGradientBrush _brush;

        public LuaRadialGradientBrush(RadialGradientBrush brush)
        {
            RadialGradientBrush = brush;
        }

        public LuaRadialGradientBrush(Dictionary<LuaColor, double> gradientColors, double centerX,
            double centerY, double originX, double originY)
        {
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

        public override Brush Brush
        {
            get { return RadialGradientBrush; }
            set { RadialGradientBrush = (RadialGradientBrush) value; }
        }

        /// <summary>
        ///     Gets or sets the Brush's GradientStops using a LUA table
        /// </summary>
        public Dictionary<LuaColor, double> GradientColors
        {
            get
            {
                return RadialGradientBrush.GradientStops.ToDictionary(gs => new LuaColor(gs.Color), gs => gs.Offset);
            }
            set
            {
                var updatedBrush = RadialGradientBrush.CloneCurrentValue();
                updatedBrush.GradientStops = new GradientStopCollection(value
                    .Select(gc => new GradientStop(gc.Key.Color, gc.Value)));

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
        private void SetupBrush(Dictionary<LuaColor, double> gradientColors, double centerX, double centerY,
            double originX, double originY)
        {
            var collection = new GradientStopCollection(gradientColors
                .Select(gc => new GradientStop(gc.Key.Color, gc.Value)));

            RadialGradientBrush = new RadialGradientBrush(collection)
            {
                Center = new Point(centerX, centerY),
                GradientOrigin = new Point(originX, originY)
            };
        }
    }
}