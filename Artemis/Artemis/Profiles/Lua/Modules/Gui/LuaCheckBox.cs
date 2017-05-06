using System;
using System.Windows;
using System.Windows.Controls;
using Artemis.Managers;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

namespace Artemis.Profiles.Lua.Modules.Gui
{
    [MoonSharpUserData]
    public class LuaCheckBox
    {
        private readonly LuaManager _luaManager;

        public LuaCheckBox(LuaManager luaManager, string text, bool isChecked, double x, double y, double? width, double? height)
        {
            _luaManager = luaManager;

            CheckBox = new CheckBox {Content = text, IsChecked = isChecked};
            if (width != null)
                CheckBox.Width = width.Value;
            if (height != null)
                CheckBox.Height = height.Value;

            CheckBox.Click += CheckBoxOnClick;
        }

        [MoonSharpVisible(false)]
        public CheckBox CheckBox { get; }

        public string Text
        {
            get => CheckBox.Dispatcher.Invoke(() => (string) CheckBox.Content);
            set => CheckBox.Dispatcher.Invoke(() => CheckBox.Content = value);
        }

        public bool IsChecked
        {
            get => CheckBox.Dispatcher.Invoke(() => CheckBox.IsChecked != null && (bool) CheckBox.IsChecked);
            set => CheckBox.Dispatcher.Invoke(() => CheckBox.IsChecked = value);
        }

        private void CheckBoxOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            _luaManager.EventsModule.LuaInvoke(_luaManager.ProfileModel, () => OnClick(this));
        }

        public event EventHandler<EventArgs> Click;

        protected virtual void OnClick(LuaCheckBox checkBox)
        {
            Click?.Invoke(checkBox, null);
        }
    }
}
