using System;
using System.Windows;
using System.Windows.Controls;
using Artemis.Managers;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

namespace Artemis.Profiles.Lua.Modules.Gui
{
    [MoonSharpUserData]
    public class LuaButton
    {
        private readonly LuaManager _luaManager;

        public LuaButton(LuaManager luaManager, string text, double x, double y, double? width, double? height)
        {
            _luaManager = luaManager;

            Button = new Button {Content = text};
            if (width != null)
                Button.Width = width.Value;
            if (height != null)
                Button.Height = height.Value;

            Button.Click += ButtonOnClick;
        }

        [MoonSharpVisible(false)]
        public Button Button { get; }

        public string Text
        {
            get => Button.Dispatcher.Invoke(() => (string) Button.Content);
            set => Button.Dispatcher.Invoke(() => Button.Content = value);
        }

        private void ButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            _luaManager.EventsModule.LuaInvoke(_luaManager.ProfileModel, () => OnClick(this));
        }

        public event EventHandler<EventArgs> Click;

        protected virtual void OnClick(LuaButton button)
        {
            Click?.Invoke(button, null);
        }
    }
}
