using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using MoonSharp.Interpreter;

namespace Artemis.Profiles.Lua.Modules.Brushes
{
    [MoonSharpUserData]
    public class LuaLinearGradientBrush : LuaBrush
    {
        public LuaLinearGradientBrush(Brush brush)
        {
            if (!(brush is LinearGradientBrush))
                throw new ArgumentException("Brush type must be LinearGradientBrush");

            Brush = brush;
        }

        public LuaLinearGradientBrush(Dictionary<LuaColor, double> gradientColors, double startX, double startY,
            double endX, double endY)
        {
            SetupBrush(gradientColors, startX, startY, endX, endY);
        }

        /// <summary>
        ///     Gets or sets the Brush's GradientStops using a LUA table
        /// </summary>
        public Dictionary<LuaColor, double> GradientColors
        {
            get
            {
                return ((LinearGradientBrush) Brush).GradientStops.ToDictionary(gs => new LuaColor(gs.Color),
                    gs => gs.Offset);
            }
            set
            {
                var updatedBrush = ((LinearGradientBrush) Brush).CloneCurrentValue();
                updatedBrush.GradientStops =
                    new GradientStopCollection(value.Select(gc => new GradientStop(gc.Key.Color, gc.Value)));

                Brush = updatedBrush;
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

            Brush = new LinearGradientBrush(collection, new Point(startX, startY), new Point(endX, endY));
        }
    }
}