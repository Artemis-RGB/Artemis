using System.Windows;
using System.Windows.Controls;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

namespace Artemis.Profiles.Lua.Modules.Gui
{
    [MoonSharpUserData]
    public class LuaLabel
    {
        public LuaLabel(string text, double x, double y, double fontSize = 12, int fontWeight = 400)
        {
            Label = new Label
            {
                Content = text,
                FontSize = fontSize,
                FontWeight = FontWeight.FromOpenTypeWeight(fontWeight)
            };
        }

        [MoonSharpVisible(false)]
        public Label Label { get; }

        public string Text
        {
            get => Label.Dispatcher.Invoke(() => (string) Label.Content);
            set => Label.Dispatcher.Invoke(() => Label.Content = value);
        }
    }
}
