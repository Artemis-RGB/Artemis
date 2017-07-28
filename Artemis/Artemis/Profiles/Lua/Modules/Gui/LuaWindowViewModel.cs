using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Artemis.Managers;
using Caliburn.Micro;
using MoonSharp.Interpreter;

namespace Artemis.Profiles.Lua.Modules.Gui
{
    [MoonSharpUserData]
    public class LuaWindowViewModel : Screen
    {
        private readonly LuaManager _luaManager;

        public LuaWindowViewModel(LuaManager luaManager)
        {
            _luaManager = luaManager;

            Canvas = new Canvas();
            Content = new List<UIElement> {Canvas};
        }

        public Canvas Canvas { get; set; }
        public List<UIElement> Content { get; set; }

        public LuaLabel CreateLabel(string text, double x, double y, double fontSize = 12, int fontWeight = 400)
        {
            LuaLabel element = null;
            Execute.OnUIThread(() =>
            {
                element = new LuaLabel(text, x, y, fontSize, fontWeight);
                AddControl(element.Label, x, y);
            });

            return element;
        }

        public LuaButton CreateButton(string text, double x, double y, double? width, double? height)
        {
            LuaButton element = null;
            Execute.OnUIThread(() =>
            {
                element = new LuaButton(_luaManager, text, x, y, width, height);
                AddControl(element.Button, x, y);
            });

            return element;
        }

        public LuaTextBox CreateTextBox(string text, double x, double y, double? width, double? height)
        {
            LuaTextBox element = null;
            Execute.OnUIThread(() =>
            {
                element = new LuaTextBox(_luaManager, text, x, y, width, height);
                AddControl(element.TextBox, x, y);
            });

            return element;
        }

        public LuaComboBox CreateComboBox(string value, List<string> values, double x, double y, double? width, double? height)
        {
            LuaComboBox element = null;
            Execute.OnUIThread(() =>
            {
                element = new LuaComboBox(_luaManager, value, values, x, y, width, height);
                AddControl(element.ComboBox, x, y);
            });

            return element;
        }

        public LuaCheckBox CreateCheckBox(string text, bool isChecked, double x, double y, double? width, double? height)
        {
            LuaCheckBox element = null;
            Execute.OnUIThread(() =>
            {
                element = new LuaCheckBox(_luaManager, text, isChecked, x, y, width, height);
                AddControl(element.CheckBox, x, y);
            });

            return element;
        }

        public LuaSlider CreateSlider(int interval, double initialValue, double minimum, double maximum, double x, double y, double? width = 200.0, double? height = 20.0)
        {
            LuaSlider element = null;
            Execute.OnUIThread(() =>
            {
                element = new LuaSlider(_luaManager, interval, initialValue, minimum, maximum, x, y, width, height);
                AddControl(element.Slider, x, y);
            });

            return element;
        }

        private void AddControl(UIElement uiElement, double x, double y)
        {
            Canvas.Children.Add(uiElement);
            Canvas.SetLeft(uiElement, x);
            Canvas.SetTop(uiElement, y);
        }
    }
}
