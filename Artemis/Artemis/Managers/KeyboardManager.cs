using System.Collections.Generic;
using System.Linq;
using Artemis.Events;
using Artemis.KeyboardProviders;
using Artemis.Settings;
using Caliburn.Micro;

namespace Artemis.Managers
{
    public class KeyboardManager
    {
        private readonly IEventAggregator _events;
        private readonly MainManager _mainManager;
        private KeyboardProvider _activeKeyboard;

        public KeyboardManager(MainManager mainManager, IEventAggregator events)
        {
            _mainManager = mainManager;
            _events = events;
            KeyboardProviders = ProviderHelper.GetKeyboardProviders();
        }

        public List<KeyboardProvider> KeyboardProviders { get; set; }

        public KeyboardProvider ActiveKeyboard
        {
            get { return _activeKeyboard; }
            set
            {
                _activeKeyboard = value;
                // Let the ViewModels know
                _events.PublishOnUIThread(new ActiveKeyboardChanged(value?.Name));
            }
        }

        /// <summary>
        ///     Enables the last keyboard according to the settings file
        /// </summary>
        public void EnableLastKeyboard()
        {
            if (General.Default.LastKeyboard == null)
                return;
            if (General.Default.LastKeyboard == "")
                return;

            var keyboard = KeyboardProviders.FirstOrDefault(k => k.Name == General.Default.LastKeyboard);
            EnableKeyboard(keyboard);
        }

        /// <summary>
        ///     Enables the given keyboard
        /// </summary>
        /// <param name="keyboardProvider"></param>
        public void EnableKeyboard(KeyboardProvider keyboardProvider)
        {
            ReleaseActiveKeyboard();

            if (keyboardProvider == null)
                return;

            if (ActiveKeyboard != null)
                if (keyboardProvider.Name == ActiveKeyboard.Name)
                    return;

            // Disable everything if there's no active keyboard found
            if (!keyboardProvider.CanEnable())
            {
                _mainManager.DialogService.ShowErrorMessageBox(keyboardProvider.CantEnableText);
                return;
            }

            ActiveKeyboard = keyboardProvider;
            ActiveKeyboard.Enable();

            General.Default.LastKeyboard = ActiveKeyboard.Name;
            General.Default.Save();
        }

        /// <summary>
        ///     Releases the active keyboard
        /// </summary>
        public void ReleaseActiveKeyboard()
        {
            if (ActiveKeyboard == null)
                return;

            ActiveKeyboard.Disable();
            ActiveKeyboard = null;
        }

        /// <summary>
        ///     Changes the active keyboard
        /// </summary>
        /// <param name="keyboardProvider"></param>
        public void ChangeKeyboard(KeyboardProvider keyboardProvider)
        {
            if (keyboardProvider == ActiveKeyboard)
                return;

            General.Default.LastKeyboard = keyboardProvider?.Name;
            General.Default.Save();

            _mainManager.Restart();
        }
    }
}