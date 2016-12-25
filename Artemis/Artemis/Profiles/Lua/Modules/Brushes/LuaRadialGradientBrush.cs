using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using MoonSharp.Interpreter;

namespace Artemis.Profiles.Lua.Modules.Brushes
{
    [MoonSharpUserData]
    public class LuaRadialGradientBrush : LuaBrush
    {
        public LuaRadialGradientBrush(Brush brush)
        {
            if (!(brush is RadialGradientBrush))
                throw new ArgumentException("Brush type must be RadialGradientBrush");

            Brush = brush;
        }

        public LuaRadialGradientBrush(Dictionary<LuaColor, double> gradientColors, double centerX,
            double centerY, double originX, double originY)
        {
            SetupBrush(gradientColors, centerX, centerY, originX, originY);
        }

        /// <summary>
        ///     Gets or sets the Brush's GradientStops using a LUA table
        /// </summary>
        public Dictionary<LuaColor, double> GradientColors
        {
            get
            {
                return ((RadialGradientBrush) Brush).GradientStops.ToDictionary(gs => new LuaColor(gs.Color),
                    gs => gs.Offset);
            }
            set
            {
                var updatedBrush = ((RadialGradientBrush) Brush).CloneCurrentValue();
                updatedBrush.GradientStops =
                    new GradientStopCollection(value.Select(gc => new GradientStop(gc.Key.Color, gc.Value)));

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
        private void SetupBrush(Dictionary<LuaColor, double> gradientColors, double centerX, double centerY,
            double originX, double originY)
        {
            var collection = new GradientStopCollection(gradientColors
                .Select(gc => new GradientStop(gc.Key.Color, gc.Value)));

            Brush = new RadialGradientBrush(collection)
            {
                Center = new Point(centerX, centerY),
                GradientOrigin = new Point(originX, originY)
            };
        }
    }
}