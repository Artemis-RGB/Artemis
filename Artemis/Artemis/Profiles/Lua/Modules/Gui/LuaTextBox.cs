using System;
using System.Windows.Controls;
using Artemis.Managers;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

namespace Artemis.Profiles.Lua.Modules.Gui
{
    [MoonSharpUserData]
    public class LuaTextBox
    {
        private readonly LuaManager _luaManager;

        public LuaTextBox(LuaManager luaManager, string text, double x, double y, double? width, double? height)
        {
            _luaManager = luaManager;

            TextBox = new TextBox {Text = text};
            if (width != null)
                TextBox.Width = width.Value;
            if (height != null)
                TextBox.Height = height.Value;

            TextBox.TextChanged += TextBoxOnTextChanged;
        }

        [MoonSharpVisible(false)]
        public TextBox TextBox { get; }

        public string Text
        {
            get => TextBox.Dispatcher.Invoke(() => TextBox.Text);
            set => TextBox.Dispatcher.Invoke(() => TextBox.Text = value);
        }

        private void TextBoxOnTextChanged(object sender, TextChangedEventArgs textChangedEventArgs)
        {
            _luaManager.EventsModule.LuaInvoke(_luaManager.ProfileModel, () => OnTextChanged(this));
        }

        public event EventHandler<EventArgs> TextChanged;

        protected virtual void OnTextChanged(LuaTextBox button)
        {
            TextChanged?.Invoke(button, null);
        }
    }
}
