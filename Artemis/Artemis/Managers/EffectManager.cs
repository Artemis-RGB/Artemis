using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Events;
using Artemis.Models;
using Artemis.Modules.Effects.ProfilePreview;
using Artemis.Settings;
using Caliburn.Micro;
using NLog;
using LogManager = NLog.LogManager;

namespace Artemis.Managers
{
    /// <summary>
    ///     Manages the effects
    /// </summary>
    public class EffectManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IEventAggregator _events;
        private readonly KeyboardManager _keyboardManager;
        private EffectModel _activeEffect;

        public EffectManager(IEventAggregator events, KeyboardManager keyboardManager)
        {
            Logger.Info("Intializing EffectManager");
            _events = events;
            _keyboardManager = keyboardManager;

            EffectModels = new List<EffectModel>();
            Logger.Info("Intialized EffectManager");
        }

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
        ///     Loads the last active effect from settings and enables it.
        /// </summary>
        /// <returns>Whether enabling was successful or not.</returns>
        public EffectModel GetLastEffect()
        {
            Logger.Debug("Getting last effect: {0}", General.Default.LastEffect);
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

            lock (_keyboardManager.ActiveKeyboard)
            {
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
            }

            if (loopManager != null && !loopManager.Running)
            {
                loopManager.Start();
            }

            Logger.Debug($"Changed active effect to: {effectModel.Name}");
        }


        /// <summary>
        ///     Clears the current effect
        /// </summary>
        public void ClearEffect()
        {
            lock (_keyboardManager.ActiveKeyboard)
            {
                lock (ActiveEffect)
                {
                    ActiveEffect.Dispose();
                    ActiveEffect = null;

                    General.Default.LastEffect = null;
                    General.Default.Save();
                }
            }

            Logger.Debug("Cleared active effect");
        }

        /// <summary>
        ///     Disables the given game
        /// </summary>
        /// <param name="activeEffect"></param>
        public void DisableGame(EffectModel activeEffect)
        {
            Logger.Debug($"Disabling game: {activeEffect?.Name}");
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