using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Input;
using Artemis.Utilities.Keyboard;
using MahApps.Metro.Controls;
using KeyEventArgs = System.Windows.Forms.KeyEventArgs;

namespace Artemis.Managers
{
    public static class KeybindManager
    {
        private static readonly Dictionary<HotKey, Action> HotKeys;

        static KeybindManager()
        {
            KeyboardHook.KeyDownCallback += KeyboardHookOnKeyDownCallback;
            HotKeys = new Dictionary<HotKey, Action>();
        }

        private static void KeyboardHookOnKeyDownCallback(KeyEventArgs e)
        {
            // Don't trigger if none of the modifiers are held down
            if (!e.Alt && !e.Control && !e.Shift)
                return;

            // Don't trigger if the key itself is a modifier
            if (e.KeyCode == Keys.LShiftKey || e.KeyCode == Keys.RShiftKey ||
                e.KeyCode == Keys.LControlKey || e.KeyCode == Keys.RControlKey ||
                e.KeyCode == Keys.LMenu || e.KeyCode == Keys.RMenu)
                return;

            // Create a WPF ModifierKeys enum
            var modifiers = ModifierKeys.None;
            if (e.Alt)
                modifiers = ModifierKeys.Alt;
            if (e.Control)
                if (modifiers == ModifierKeys.None)
                    modifiers = ModifierKeys.Control;
                else
                    modifiers |= ModifierKeys.Control;
            if (e.Shift)
                if (modifiers == ModifierKeys.None)
                    modifiers = ModifierKeys.Shift;
                else
                    modifiers |= ModifierKeys.Shift;

            // Create a HotKey object for comparison
            var hotKey = new HotKey(KeyInterop.KeyFromVirtualKey(e.KeyValue), modifiers);

            // If the hotkey is present, invoke the action related to it
            if (HotKeys.ContainsKey(hotKey))
                HotKeys[hotKey].Invoke();
        }

        /// <summary>
        ///     Registers a hotkey and executes the provided action when the hotkey is pressed
        /// </summary>
        /// <param name="hotKey">The hotkey to register</param>
        /// <param name="action">The action to invoke on press</param>
        /// <returns>Returns true if key registed, false if already in use</returns>
        public static bool RegisterHotkey(HotKey hotKey, Action action)
        {
            if (HotKeys.ContainsKey(hotKey))
                return false;

            HotKeys.Add(hotKey, action);
            return true;
        }

        /// <summary>
        ///     Unregister the given hotkey
        /// </summary>
        /// <param name="hotKey">The hotkey to unregister</param>
        /// <returns>Returns true if unregistered, false if not found</returns>
        public static bool UnregisterHotkey(HotKey hotKey)
        {
            if (!HotKeys.ContainsKey(hotKey))
                return false;

            HotKeys.Remove(hotKey);
            return true;
        }
    }
}