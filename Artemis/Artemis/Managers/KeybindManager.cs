using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using Artemis.Models;
using Artemis.Utilities.Keyboard;
using MahApps.Metro.Controls;
using KeyEventArgs = System.Windows.Forms.KeyEventArgs;

namespace Artemis.Managers
{
    public static class KeybindManager
    {
        private static readonly List<KeybindModel> KeybindModels = new List<KeybindModel>();

        static KeybindManager()
        {
            KeyboardHook.KeyDownCallback += KeyboardHookOnKeyDownCallback;
        }

        private static void KeyboardHookOnKeyDownCallback(KeyEventArgs e)
        {
            // Don't trigger if the key itself is a modifier
            if (e.KeyCode == Keys.LShiftKey || e.KeyCode == Keys.RShiftKey ||
                e.KeyCode == Keys.LControlKey || e.KeyCode == Keys.RControlKey ||
                e.KeyCode == Keys.LMenu || e.KeyCode == Keys.RMenu)
                return;

            // Create a WPF ModifierKeys enum
            var modifiers = ModifierKeysFromBooleans(e.Alt, e.Control, e.Shift);

            // Create a HotKey object for comparison
            var hotKey = new HotKey(KeyInterop.KeyFromVirtualKey(e.KeyValue), modifiers);

            foreach (var keybindModel in KeybindModels)
                keybindModel.InvokeIfMatched(hotKey);
        }

        public static void AddOrUpdate(KeybindModel keybindModel)
        {
            var existing = KeybindModels.FirstOrDefault(k => k.Name == keybindModel.Name);
            if (existing != null)
                KeybindModels.Remove(existing);

            KeybindModels.Add(keybindModel);
        }

        public static void Remove(KeybindModel keybindModel)
        {
            if (KeybindModels.Contains(keybindModel))
                KeybindModels.Remove(keybindModel);
        }

        public static void Remove(string name)
        {
            var existing = KeybindModels.FirstOrDefault(k => k.Name == name);
            if (existing != null)
                KeybindModels.Remove(existing);
        }

        public static void Clear()
        {
            // TODO: Re-add future global keybinds here or just exclude them from the clear
            KeybindModels.Clear();
        }

        public static ModifierKeys ModifierKeysFromBooleans(bool alt, bool control, bool shift)
        {
            // Create a WPF ModifierKeys enum
            var modifiers = ModifierKeys.None;
            if (alt)
                modifiers = ModifierKeys.Alt;
            if (control)
                if (modifiers == ModifierKeys.None)
                    modifiers = ModifierKeys.Control;
                else
                    modifiers |= ModifierKeys.Control;
            if (shift)
                if (modifiers == ModifierKeys.None)
                    modifiers = ModifierKeys.Shift;
                else
                    modifiers |= ModifierKeys.Shift;

            return modifiers;
        }
    }
}