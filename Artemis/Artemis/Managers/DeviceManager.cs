using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Artemis.DeviceProviders;
using Artemis.Events;
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
    public class DeviceManager
    {
        private readonly IEventAggregator _events;
        private readonly ILogger _logger;

        public DeviceManager(IEventAggregator events, ILogger logger, List<DeviceProvider> deviceProviders)
        {
            _logger = logger;
            _logger.Info("Intializing DeviceManager");

            _events = events;

            KeyboardProviders = deviceProviders.Where(d => d.Type == DeviceType.Keyboard)
                .Cast<KeyboardProvider>().ToList();
            MiceProviders = deviceProviders.Where(d => d.Type == DeviceType.Mouse).ToList();
            HeadsetProviders = deviceProviders.Where(d => d.Type == DeviceType.Headset).ToList();

            _logger.Info("Intialized DeviceManager");
        }

        public List<DeviceProvider> HeadsetProviders { get; set; }

        public List<DeviceProvider> MiceProviders { get; set; }

        [Inject]
        public MetroDialogService DialogService { get; set; }

        public List<KeyboardProvider> KeyboardProviders { get; set; }

        public KeyboardProvider ActiveKeyboard { get; set; }

        public bool ChangingKeyboard { get; private set; }

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
                ChangingKeyboard = true;

                if (keyboardProvider == null)
                    throw new ArgumentNullException(nameof(keyboardProvider));

                if (ActiveKeyboard?.Name == keyboardProvider.Name)
                {
                    ChangingKeyboard = false;
                    return;
                }

                // Store the old keyboard so it can be used in the event we're raising later
                var oldKeyboard = ActiveKeyboard;

                var wasNull = false;
                if (ActiveKeyboard == null)
                {
                    wasNull = true;
                    ActiveKeyboard = keyboardProvider;
                }

                _logger.Debug("Enabling keyboard: {0}", keyboardProvider.Name);

                if (!wasNull)
                    ReleaseActiveKeyboard();
                bool asynchEnable = false;
                if (keyboardProvider.CanEnable())
                {
                    FinishEnableKeyboard(keyboardProvider, oldKeyboard);
                }
                else
                {
                    for (int i = 1; i <= 10; i++)
                    {
                        var thread = new Thread(
                        () =>
                        {
                            Thread.Sleep(500);
                            asynchEnable = keyboardProvider.CanEnable();
                        });
                        _logger.Warn("Failed enabling keyboard: {0}, re-attempt {1} of 10", keyboardProvider.Name, i);
                        thread.Start();
                        if (asynchEnable)
                            break;
                    }

                    if (!asynchEnable)
                    {
                        // Disable everything if there's no active keyboard found
                        DialogService.ShowErrorMessageBox(keyboardProvider.CantEnableText);
                        ActiveKeyboard = null;
                        General.Default.LastKeyboard = null;
                        General.Default.Save();
                        _logger.Warn("Failed enabling keyboard: {0}", keyboardProvider.Name);
                        ChangingKeyboard = false;
                        return;
                    }                   
                    FinishEnableKeyboard(keyboardProvider, oldKeyboard);
                }
                
            }
        }

        private void FinishEnableKeyboard(KeyboardProvider keyboardProvider, KeyboardProvider oldKeyboard)
        {
            ActiveKeyboard = keyboardProvider;
            ActiveKeyboard.Enable();

            General.Default.LastKeyboard = ActiveKeyboard.Name;
            General.Default.Save();

            EnableUsableDevices();

            ChangingKeyboard = false;
            _events.PublishOnUIThread(new ActiveKeyboardChanged(oldKeyboard, ActiveKeyboard));
            _logger.Debug("Enabled keyboard: {0}", keyboardProvider.Name);
        }

        private void EnableUsableDevices()
        {
            foreach (var mouseProvider in MiceProviders)
                mouseProvider.TryEnable();
            foreach (var headsetProvider in HeadsetProviders)
                headsetProvider.TryEnable();
        }

        /// <summary>
        ///     Releases the active keyboard
        /// </summary>
        /// <param name="save">Whether to save the LastKeyboard (making it null)</param>
        public void ReleaseActiveKeyboard(bool save = false)
        {
            lock (this)
            {
                if (ActiveKeyboard == null)
                    return;

                // Store the old keyboard so it can be used in the event we're raising later
                var oldKeyboard = ActiveKeyboard;

                var releaseName = ActiveKeyboard.Name;
                ActiveKeyboard.Disable();
                ActiveKeyboard = null;

                if (save)
                {
                    General.Default.LastKeyboard = null;
                    General.Default.Save();
                }

                _events.PublishOnUIThread(new ActiveKeyboardChanged(oldKeyboard, null));
                _logger.Debug("Released keyboard: {0}", releaseName);
            }
        }
    }
}