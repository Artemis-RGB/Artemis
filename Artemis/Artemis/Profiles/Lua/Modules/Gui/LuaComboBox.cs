using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using Artemis.Managers;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

namespace Artemis.Profiles.Lua.Modules.Gui
{
    [MoonSharpUserData]
    public class LuaComboBox
    {
        private readonly LuaManager _luaManager;

        public LuaComboBox(LuaManager luaManager, string value, List<string> values, double x, double y, double? width, double? height)
        {
            _luaManager = luaManager;

            ComboBox = new ComboBox {ItemsSource = new ObservableCollection<string>(values), SelectedItem = value};
            if (width != null)
                ComboBox.Width = (double) width;
            if (height != null)
                ComboBox.Height = (double) height;

            ComboBox.SelectionChanged += ComboBoxOnSelectionChanged;
        }

        [MoonSharpVisible(false)]
        public ComboBox ComboBox { get; }

        public string Value
        {
            get => ComboBox.Dispatcher.Invoke(() => (string) ComboBox.SelectedItem);
            set => ComboBox.Dispatcher.Invoke(() => ComboBox.SelectedItem = value);
        }

        public void SetValues(string[] values)
        {
            var collection = (ObservableCollection<string>) ComboBox.ItemsSource;
            collection.Clear();
            foreach (var value in values)
                collection.Add(value);
        }

        public void AddValue(string value)
        {
            ((ObservableCollection<string>) ComboBox.ItemsSource).Add(value);
        }

        public void RemoveValue(string value)
        {
            var collection = (ObservableCollection<string>) ComboBox.ItemsSource;
            if (collection.Contains(value))
                collection.Remove(value);
        }

        private void ComboBoxOnSelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            _luaManager.EventsModule.LuaInvoke(_luaManager.ProfileModel, () => OnSelectionChanged(this));
        }

        public event EventHandler<EventArgs> SelectionChanged;

        protected virtual void OnSelectionChanged(LuaComboBox comboBox)
        {
            SelectionChanged?.Invoke(comboBox, null);
        }
    }
}
