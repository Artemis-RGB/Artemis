using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Events;
using Artemis.KeyboardProviders;
using Artemis.Settings;
using Caliburn.Micro;
using Ninject;
using NLog;
using LogManager = NLog.LogManager;

namespace Artemis.Managers
{
    public class KeyboardManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IEventAggregator _events;
        private KeyboardProvider _activeKeyboard;

        public KeyboardManager(IEventAggregator events)
        {
            Logger.Info("Intializing KeyboardManager");

            _events = events;
            KeyboardProviders = ProviderHelper.GetKeyboardProviders();

            Logger.Info("Intialized KeyboardManager");
        }

        [Inject]
        public Lazy<MainManager> MainManager { get; set; }

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

        public bool CanDisable { get; set; }

        /// <summary>
        ///     Enables the last keyboard according to the settings file
        /// </summary>
        public void EnableLastKeyboard()
        {
            Logger.Debug("Enabling last keyboard: {0}", General.Default.LastKeyboard);
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
            Logger.Debug("Enabling keyboard: {0}", keyboardProvider?.Name);
            ReleaseActiveKeyboard();

            if (keyboardProvider == null)
                return;

            if (ActiveKeyboard != null)
                if (keyboardProvider.Name == ActiveKeyboard.Name)
                    return;

            // Disable everything if there's no active keyboard found
            if (!keyboardProvider.CanEnable())
            {
                MainManager.Value.DialogService.ShowErrorMessageBox(keyboardProvider.CantEnableText);
                General.Default.LastKeyboard = null;
                General.Default.Save();
                return;
            }

            CanDisable = false;
            ActiveKeyboard = keyboardProvider;
            keyboardProvider.Enable();

            General.Default.LastKeyboard = ActiveKeyboard.Name;
            General.Default.Save();
            CanDisable = true;
        }

        /// <summary>
        ///     Releases the active keyboard, if CanDisable is true
        /// </summary>
        public void ReleaseActiveKeyboard()
        {
            if (ActiveKeyboard == null || !CanDisable)
                return;

            ActiveKeyboard.Disable();
            Logger.Debug("Released keyboard: {0}", ActiveKeyboard?.Name);
            ActiveKeyboard = null;
        }

        /// <summary>
        ///     Changes the active keyboard
        /// </summary>
        /// <param name="keyboardProvider"></param>
        public void ChangeKeyboard(KeyboardProvider keyboardProvider)
        {
            Logger.Debug("Changing active keyboard");
            if (keyboardProvider == ActiveKeyboard)
                return;

            General.Default.LastKeyboard = keyboardProvider?.Name;
            General.Default.Save();

            Logger.Debug("Restarting for keyboard change");
            MainManager.Value.Restart();
        }
    }
}