using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Artemis.Managers;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

namespace Artemis.Profiles.Lua.Modules.Gui
{
    [MoonSharpUserData]
    public class LuaSlider
    {
        private readonly LuaManager _luaManager;

        public LuaSlider(LuaManager luaManager, int interval, double intialValue, double minimum, double maximum, double x, double y, double? width, double? height)
        {
            _luaManager = luaManager;

            Slider = new Slider { Value = intialValue, TickFrequency = interval, Minimum = minimum, Maximum = maximum, TickPlacement = TickPlacement.BottomRight, IsSnapToTickEnabled = true};

            if (width != null)
                Slider.Width = width.Value;
            if (height != null)
                Slider.Height = height.Value;

            Slider.ValueChanged += SliderOnValueChanged;
        }

        [MoonSharpVisible(false)]
        public Slider Slider { get; }

        public double Value
        {
            get => Slider.Dispatcher.Invoke(() => (double) Slider.Value);
            set => Slider.Dispatcher.Invoke(() => Slider.Value = value);
        }

        public int Interval
        {
            get => Slider.Dispatcher.Invoke(() => (int) Slider.Interval);
            set => Slider.Dispatcher.Invoke(() => Slider.Interval = value);
        }

        private void SliderOnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> routedPropertyChangedEventArgs)
        {
            _luaManager.EventsModule.LuaInvoke(_luaManager.ProfileModel, () => OnValueChanged(this));
        }

        public event EventHandler<EventArgs> ValueChanged;

        protected virtual void OnValueChanged(LuaSlider slider)
        {
            ValueChanged?.Invoke(slider, null);
        }
    }
}