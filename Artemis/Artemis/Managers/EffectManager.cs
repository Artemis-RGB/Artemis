using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Artemis.Events;
using Artemis.Models;
using Artemis.Modules.Effects.ProfilePreview;
using Artemis.Settings;
using Caliburn.Micro;
using Ninject.Extensions.Logging;

namespace Artemis.Managers
{
    /// <summary>
    ///     Manages the effects
    /// </summary>
    public class EffectManager
    {
        private readonly IEventAggregator _events;
        private readonly KeyboardManager _keyboardManager;
        private readonly ILogger _logger;
        private EffectModel _activeEffect;
        private EffectModel _prePreviewEffect;

        public EffectManager(IEventAggregator events, ILogger logger, KeyboardManager keyboardManager)
        {
            _logger = logger;
            _logger.Info("Intializing EffectManager");

            _events = events;
            _keyboardManager = keyboardManager;

            EffectModels = new List<EffectModel>();

            var profilePreviewTimer = new Timer(500);
            profilePreviewTimer.Elapsed += SetupProfilePreview;
            profilePreviewTimer.Start();

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
                _events.PublishOnUIThread(new ActiveEffectChanged(value?.Name));
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
            get { return EffectModels.OfType<GameModel>().Where(g => g.Enabled); }
        }

        /// <summary>
        ///     Keeps track of profiles being previewed and sets up the active efffect accordingly
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetupProfilePreview(object sender, ElapsedEventArgs e)
        {
            if (_keyboardManager.ChangingKeyboard)
                return;
            
            // Make sure the preview model should still be active
            if (ActiveEffect is ProfilePreviewModel)
            {
                // Should not be active if no selected profile is set
                if (ProfilePreviewModel.SelectedProfile != null)
                    return;

                if (_prePreviewEffect != null)
                {
                    _logger.Debug("Change back effect after profile preview");
                    ChangeEffect(_prePreviewEffect);
                }
                else
                {
                    _logger.Debug("Clear effect after profile preview");
                    ClearEffect(); // TODO: This fails to lock
                }
            }
            // Else make sure preview model indeed shouldn't be active
            else
            {
                if (ProfilePreviewModel?.SelectedProfile == null)
                    return;

                _prePreviewEffect = ActiveEffect;
                _logger.Debug("Activate profile preview");
                ChangeEffect(ProfilePreviewModel);
            }
        }

        /// <summary>
        ///     Loads the last active effect from settings and enables it.
        /// </summary>
        /// <returns>Whether enabling was successful or not.</returns>
        public EffectModel GetLastEffect()
        {
            _logger.Debug("Getting last effect: {0}", General.Default.LastEffect);
            return General.Default.LastEffect == null
                ? null
                : EffectModels.FirstOrDefault(e => e.Name == General.Default.LastEffect);
        }

        /// <summary>
        ///     Disables the current effect and changes it to the provided effect.
        /// </summary>
        /// <param name="effectModel">The effect to activate</param>
        /// <param name="loopManager">Optionally pass the LoopManager to automatically start it, if it's not running.</param>
        public void ChangeEffect(EffectModel effectModel, LoopManager loopManager = null)
        {
            if (effectModel == null)
                throw new ArgumentNullException(nameof(effectModel));
            if (effectModel is OverlayModel)
                throw new ArgumentException("Can't set an Overlay effect as the active effect");

            if (_keyboardManager.ActiveKeyboard == null)
                _keyboardManager.EnableLastKeyboard();
            // If still null, no last keyboard, so stop.
            if (_keyboardManager.ActiveKeyboard == null)
                return;

            // Game models are only used if they are enabled
            var gameModel = effectModel as GameModel;
            if (gameModel != null)
                if (!gameModel.Enabled)
                    return;

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

                ActiveEffect = effectModel;
                ActiveEffect.Enable();

                if (ActiveEffect is GameModel || ActiveEffect is ProfilePreviewModel)
                    return;

                // Non-game effects are stored as the new LastEffect.
                General.Default.LastEffect = ActiveEffect?.Name;
                General.Default.Save();
            }

            if (loopManager != null && !loopManager.Running)
            {
                _logger.Debug("Starting LoopManager for effect change");
                loopManager.Start();
            }

            _logger.Debug("Changed active effect to: {0}", effectModel.Name);
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

                General.Default.LastEffect = null;
                General.Default.Save();
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
    }
}