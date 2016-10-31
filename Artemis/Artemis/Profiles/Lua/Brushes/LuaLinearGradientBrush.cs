using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

namespace Artemis.Profiles.Lua.Brushes
{
    [MoonSharpUserData]
    public class LuaLinearGradientBrush : LuaBrush
    {
        private LinearGradientBrush _brush;

        public LuaLinearGradientBrush(LinearGradientBrush linearGradientBrush)
        {
            LinearGradientBrush = linearGradientBrush;
        }

        public LuaLinearGradientBrush(Dictionary<LuaColor, double> gradientColors, double startX, double startY,
            double endX, double endY)
        {
            SetupBrush(gradientColors, startX, startY, endX, endY);
        }

        /// <summary>
        ///     The underlying brush
        /// </summary>
        [MoonSharpVisible(false)]
        public LinearGradientBrush LinearGradientBrush
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
        public Dictionary<LuaColor, double> GradientColors
        {
            get
            {
                return LinearGradientBrush.GradientStops.ToDictionary(gs => new LuaColor(gs.Color), gs => gs.Offset);
            }
            set
            {
                var updatedBrush = LinearGradientBrush.CloneCurrentValue();
                updatedBrush.GradientStops = new GradientStopCollection(value
                    .Select(gc => new GradientStop(gc.Key.Color, gc.Value)));

                LinearGradientBrush = updatedBrush;
            }
        }

        /// <summary>
        ///     Configures the brush according to the provided values usable in LUA
        /// </summary>
        /// <param name="gradientColors"></param>
        /// <param name="startX"></param>
        /// <param name="startY"></param>
        /// <param name="endX"></param>
        /// <param name="endY"></param>
        private void SetupBrush(Dictionary<LuaColor, double> gradientColors, double startX, double startY, double endX,
            double endY)
        {
            var collection = new GradientStopCollection(gradientColors
                .Select(gc => new GradientStop(gc.Key.Color, gc.Value)));

            LinearGradientBrush = new LinearGradientBrush(collection, new Point(startX, startY), new Point(endX, endY));
        }
    }
}