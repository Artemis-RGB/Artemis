using System.Linq;
using System.Windows;
using System.Windows.Media;
using Artemis.Utilities;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

namespace Artemis.Profiles.Lua.Brushes
{
    [MoonSharpUserData]
    public class LuaLinearGradientBrush : LuaBrush
    {
        private readonly Script _script;
        private LinearGradientBrush _brush;

        public LuaLinearGradientBrush(LinearGradientBrush linearGradientBrush)
        {
            Brush = linearGradientBrush;
        }

        public LuaLinearGradientBrush(Script script, Table gradientColors, double startX = 0.5, double startY = 0.0,
            double endX = 0.5, double endY = 1.0)
        {
            _script = script;
            SetupBrush(gradientColors, startX, startY, endX, endY);
        }

        /// <summary>
        ///     The underlying brush
        /// </summary>
        [MoonSharpVisible(false)]
        public new LinearGradientBrush Brush
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
            get { return CreateGradientTable(); }
            set
            {
                var updatedBrush = Brush.CloneCurrentValue();
                updatedBrush.GradientStops = CreateGradientCollection(value);
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
        private void SetupBrush(Table gradientColors, double startX, double startY, double endX, double endY)
        {
            var collection = CreateGradientCollection(gradientColors);
            Brush = new LinearGradientBrush(collection, new Point(startX, startY), new Point(endX, endY));
        }

        /// <summary>
        ///     Maps a LUA table to a GradientStopsCollection
        /// </summary>
        /// <param name="gradientColors"></param>
        /// <returns></returns>
        private GradientStopCollection CreateGradientCollection(Table gradientColors)
        {
            var collection = new GradientStopCollection();
            foreach (var gradientColor in gradientColors.Values)
            {
                var pair = gradientColor.Table.Values.ToList();
                var hexCode = pair[0].String;
                var position = pair[1].Number;
                collection.Add(new GradientStop(new Color().FromHex(hexCode), position));
            }
            return collection;
        }

        /// <summary>
        ///     Maps the current brush's GradientStopsCollection to a LUA table
        /// </summary>
        /// <returns></returns>
        private Table CreateGradientTable()
        {
            var table = new Table(_script);
            foreach (var gradientStop in Brush.GradientStops)
            {
                var inner = new Table(_script);
                inner.Append(DynValue.NewString(gradientStop.Color.ToHex()));
                inner.Append(DynValue.NewNumber(gradientStop.Offset));
                table.Append(DynValue.NewTable(inner));
            }
            return table;
        }
    }
}