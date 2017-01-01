using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Events;
using Artemis.Models;
using Artemis.Modules.Effects.ProfilePreview;
using Artemis.Settings;
using Ninject.Extensions.Logging;

namespace Artemis.Managers
{
    /// <summary>
    ///     Manages the effects
    /// </summary>
    public class EffectManager
    {
        private readonly DeviceManager _deviceManager;
        private readonly ILogger _logger;
        private EffectModel _activeEffect;
        private LoopManager _waitLoopManager;
        private EffectModel _waitEffect;
        private readonly GeneralSettings _generalSettings;

        public EffectManager(ILogger logger, DeviceManager deviceManager, List<EffectModel> effectModels,
            List<GameModel> gameModels, List<OverlayModel> overlayModels)
        {
            _generalSettings = DAL.SettingsProvider.Load<GeneralSettings>();
            _logger = logger;
            _deviceManager = deviceManager;

            var models = new List<EffectModel>();
            // Add regular effects
            models.AddRange(effectModels);
            // Add overlays
            models.AddRange(overlayModels);
            // Add games, exclude WoW if needed
            models.AddRange(_generalSettings.GamestatePort != 62575
                ? gameModels.Where(e => e.Name != "WoW").Where(e => e.Name != "LightFX")
                : gameModels.Where(e => e.Name != "LightFX"));

            EffectModels = models;
            _logger.Info("Intialized EffectManager");
        }

        public ProfilePreviewModel ProfilePreviewModel { get; set; }

        /// <summary>
        ///     Holds all the effects the program has
        /// </summary>
        public List<EffectModel> EffectModels { get; set; }

        public EffectModel ActiveEffect
        {
            get { return _activeEffect; }
            private set
            {
                _activeEffect = value;
                RaiseEffectChangedEvent(new EffectChangedEventArgs(value));
            }
        }

        /// <summary>
        ///     Returns all enabled overlays
        /// </summary>
        public IEnumerable<OverlayModel> EnabledOverlays
        {
            get { return EffectModels.OfType<OverlayModel>().Where(o => o.Enabled); }
        }

        /// <summary>
        ///     Returns all enabled games
        /// </summary>
        public IEnumerable<GameModel> EnabledGames
        {
            get { return EffectModels.OfType<GameModel>().Where(g => g.Enabled && g.Settings.Enabled); }
        }

        public event EventHandler<EffectChangedEventArgs> OnEffectChangedEvent;

        /// <summary>
        ///     Loads the last active effect from settings and enables it.
        /// </summary>
        /// <returns>Whether enabling was successful or not.</returns>
        public EffectModel GetLastEffect()
        {
            _logger.Debug("Getting last effect: {0}", _generalSettings.LastEffect);
            return _generalSettings.LastEffect == null
                ? null
                : EffectModels.FirstOrDefault(e => e.Name == _generalSettings.LastEffect);
        }

        /// <summary>
        ///     Disables the current effect and changes it to the provided effect.
        /// </summary>
        /// <param name="effectModel">The effect to activate</param>
        /// <param name="loopManager">Optionally pass the LoopManager to automatically start it, if it's not running.</param>
        public void ChangeEffect(EffectModel effectModel, LoopManager loopManager = null)
        {
            if (_waitEffect != null)
            {
                _logger.Debug("Stopping effect because a change is already queued");
                return;
            }

            if (effectModel == null)
                throw new ArgumentNullException(nameof(effectModel));
            if (effectModel is OverlayModel)
                throw new ArgumentException("Can't set an Overlay effect as the active effect");

            if (_deviceManager.ActiveKeyboard == null)
            {
                _logger.Debug("Stopping effect change until keyboard is enabled");
                _waitEffect = effectModel;
                _waitLoopManager = loopManager;
                _deviceManager.OnKeyboardChangedEvent += DeviceManagerOnOnKeyboardChangedEvent;
                _deviceManager.EnableLastKeyboard();
                return;
            }

            // Game models are only used if they are enabled
            var gameModel = effectModel as GameModel;
            if (gameModel != null)
                if (!gameModel.Enabled)
                {
                    _logger.Debug("Cancelling effect change, provided game not enabled");
                    return;
                }


            var wasNull = false;
            if (ActiveEffect == null)
            {
                wasNull = true;
                ActiveEffect = effectModel;
            }

            lock (ActiveEffect)
            {
                if (!wasNull)
                    ActiveEffect.Dispose();
                lock (effectModel)
                {
                    ActiveEffect = effectModel;
                    ActiveEffect.Enable();
                    if (!ActiveEffect.Initialized)
                    {
                        _logger.Debug("Cancelling effect change, couldn't initialize the effect ({0})", effectModel.Name);
                        ActiveEffect = null;
                        return;
                    }
                }
            }

            if (loopManager != null && !loopManager.Running)
            {
                _logger.Debug("Starting LoopManager for effect change");
                loopManager.StartAsync();
            }

            _logger.Debug("Changed active effect to: {0}", effectModel.Name);

            if (ActiveEffect is GameModel || ActiveEffect is ProfilePreviewModel)
                return;

            // Non-game effects are stored as the new LastEffect.
            _generalSettings.LastEffect = ActiveEffect?.Name;
            _generalSettings.Save();
        }

        private void DeviceManagerOnOnKeyboardChangedEvent(object sender, KeyboardChangedEventArgs e)
        {
            _deviceManager.OnKeyboardChangedEvent -= DeviceManagerOnOnKeyboardChangedEvent;
            _logger.Debug("Resuming effect change");

            var effect = _waitEffect;
            _waitEffect = null;
            var loopManager = _waitLoopManager;
            _waitLoopManager = null;

            ChangeEffect(effect, loopManager);
        }


        /// <summary>
        ///     Clears the current effect
        /// </summary>
        public void ClearEffect()
        {
            if (ActiveEffect == null)
                return;

            lock (ActiveEffect)
            {
                ActiveEffect.Dispose();
                ActiveEffect = null;

                _generalSettings.LastEffect = null;
                _generalSettings.Save();
            }

            _logger.Debug("Cleared active effect");
        }

        /// <summary>
        ///     Disables the given game
        /// </summary>
        /// <param name="activeEffect"></param>
        public void DisableGame(EffectModel activeEffect)
        {
            _logger.Debug("Disabling game: {0}", activeEffect?.Name);
            if (GetLastEffect() == null)
                ClearEffect();
            else
                ChangeEffect(GetLastEffect());
        }

        /// <summary>
        ///     Disables the current ActiveEffect if it's a game that is disabled.
        /// </summary>
        public void DisableInactiveGame()
        {
            if (!(ActiveEffect is GameModel))
                return;
            if (EnabledGames.Contains(ActiveEffect))
                return;

            DisableGame(ActiveEffect);
        }

        protected virtual void RaiseEffectChangedEvent(EffectChangedEventArgs e)
        {
            var handler = OnEffectChangedEvent;
            handler?.Invoke(this, e);
        }
    }
}