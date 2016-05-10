using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Events;
using Artemis.KeyboardProviders;
using Artemis.Services;
using Artemis.Settings;
using Caliburn.Micro;
using Ninject;
using Ninject.Extensions.Logging;

namespace Artemis.Managers
{
    /// <summary>
    ///     Manages the keyboard providers
    /// </summary>
    public class KeyboardManager
    {
        private readonly IEventAggregator _events;
        private readonly ILogger _logger;
        private KeyboardProvider _activeKeyboard;

        public KeyboardManager(IEventAggregator events, ILogger logger, List<KeyboardProvider> keyboardProviders)
        {
            _logger = logger;
            _logger.Info("Intializing KeyboardManager");

            _events = events;
            KeyboardProviders = keyboardProviders;

            _logger.Info("Intialized KeyboardManager");
        }

        [Inject]
        public MetroDialogService DialogService { get; set; }

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
            _logger.Debug("Getting last keyboard: {0}", General.Default.LastKeyboard);
            if (string.IsNullOrEmpty(General.Default.LastKeyboard))
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
            lock (this)
            {
                if (keyboardProvider == null)
                    throw new ArgumentNullException(nameof(keyboardProvider));

                if (ActiveKeyboard?.Name == keyboardProvider.Name)
                    return;

                var wasNull = false;
                if (ActiveKeyboard == null)
                {
                    wasNull = true;
                    ActiveKeyboard = keyboardProvider;
                }

                _logger.Debug("Enabling keyboard: {0}", keyboardProvider.Name);

                if (!wasNull)
                    ReleaseActiveKeyboard();

                // Disable everything if there's no active keyboard found
                if (!keyboardProvider.CanEnable())
                {
                    DialogService.ShowErrorMessageBox(keyboardProvider.CantEnableText);
                    ActiveKeyboard = null;
                    General.Default.LastKeyboard = null;
                    General.Default.Save();
                    _logger.Warn("Failed enabling keyboard: {0}", keyboardProvider.Name);
                    return;
                }

                ActiveKeyboard = keyboardProvider;
                ActiveKeyboard.Enable();

                General.Default.LastKeyboard = ActiveKeyboard.Name;
                General.Default.Save();
                _logger.Debug("Enabled keyboard: {0}", keyboardProvider.Name);
            }
        }

        /// <summary>
        ///     Releases the active keyboard, if CanDisable is true
        /// </summary>
        public void ReleaseActiveKeyboard()
        {
            lock (this)
            {
                if (ActiveKeyboard == null)
                    return;

                var releaseName = ActiveKeyboard.Name;
                ActiveKeyboard.Disable();
                ActiveKeyboard = null;
                _logger.Debug("Released keyboard: {0}", releaseName);
            }
        }
    }
}