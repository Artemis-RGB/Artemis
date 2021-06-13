using System;
using System.Linq;
using System.Windows.Input;
using Artemis.Core;
using Artemis.Core.Services;
using Stylet;

namespace Artemis.UI.Screens.Sidebar.Dialogs.ProfileEdit
{
    public class ProfileConfigurationHotkeyViewModel : Screen
    {
        private readonly bool _isDisableHotkey;
        private readonly ProfileConfiguration _profileConfiguration;
        private string _hint;
        private string _hotkeyDisplay;

        public ProfileConfigurationHotkeyViewModel(ProfileConfiguration profileConfiguration, bool isDisableHotkey)
        {
            _profileConfiguration = profileConfiguration;
            _isDisableHotkey = isDisableHotkey;

            UpdateHotkeyDisplay();
        }

        public ProfileConfigurationHotkey Hotkey => _isDisableHotkey ? _profileConfiguration.DisableHotkey : _profileConfiguration.EnableHotkey;

        public string Hint
        {
            get => _hint;
            set => SetAndNotify(ref _hint, value);
        }

        public string HotkeyDisplay
        {
            get => _hotkeyDisplay;
            set
            {
                if (!SetAndNotify(ref _hotkeyDisplay, value)) return;
                if (value == null && Hotkey != null)
                {
                    Hotkey.Key = null;
                    Hotkey.Modifiers = null;
                }
            }
        }

        public void UpdateHotkeyDisplay()
        {
            string display = null;
            if (Hotkey?.Modifiers != null)
                display = string.Join("+", Enum.GetValues<KeyboardModifierKey>().Skip(1).Where(m => Hotkey.Modifiers.Value.HasFlag(m)));
            if (Hotkey?.Key != null)
                display = string.IsNullOrEmpty(display) ? Hotkey.Key.ToString() : $"{display}+{Hotkey.Key}";

            HotkeyDisplay = display;
            if (_profileConfiguration.HotkeyMode == ProfileConfigurationHotkeyMode.EnableDisable)
                Hint = _isDisableHotkey ? "Disable hotkey" : "Enable hotkey";
            else
                Hint = "Toggle hotkey";
        }

        public void TextBoxKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key >= Key.LeftShift && e.Key <= Key.RightAlt)
                return;

            if (_isDisableHotkey)
            {
                _profileConfiguration.DisableHotkey ??= new ProfileConfigurationHotkey();
                _profileConfiguration.DisableHotkey.Key = (KeyboardKey?) e.Key;
                _profileConfiguration.DisableHotkey.Modifiers = (KeyboardModifierKey?) Keyboard.Modifiers;
            }
            else
            {
                _profileConfiguration.EnableHotkey ??= new ProfileConfigurationHotkey();
                _profileConfiguration.EnableHotkey.Key = (KeyboardKey?) e.Key;
                _profileConfiguration.EnableHotkey.Modifiers = (KeyboardModifierKey?) Keyboard.Modifiers;
            }

            e.Handled = true;
            UpdateHotkeyDisplay();
        }
    }
}