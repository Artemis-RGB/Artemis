using System;
using System.Windows;
using System.Windows.Controls;
using Artemis.Managers;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

namespace Artemis.Profiles.Lua.Modules.Gui
{
    [MoonSharpUserData]
    class LuaSlider
    {
        private readonly LuaManager _luaManager;

        public LuaSlider(LuaManager luaManager, int interval, double intialValue, double x, double y, double? width, double? height)
        {
            _luaManager = luaManager;

            Slider = new Slider { Value = intialValue, Interval = interval };

            if (width != null)
                Slider.Width = width.Value;
            if (height != null)
                Slider.Height = height.Value;
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
    }
}