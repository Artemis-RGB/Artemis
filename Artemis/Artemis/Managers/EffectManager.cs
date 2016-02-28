using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Events;
using Artemis.Models;
using Artemis.Settings;
using Caliburn.Micro;

namespace Artemis.Managers
{
    public class EffectManager
    {
        private readonly MainManager _mainManager;
        private readonly IEventAggregator _events;

        public EffectManager(MainManager mainManager, IEventAggregator events)
        {
            _mainManager = mainManager;
            _events = events;

            EffectModels = new List<EffectModel>();
        }

        public List<EffectModel> EffectModels { get; set; }
        public EffectModel ActiveEffect { get; private set; }

        public IEnumerable<OverlayModel> EnabledOverlays
        {
            get { return EffectModels.OfType<OverlayModel>().Where(o => o.Enabled); }
        }

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
            if (General.Default.LastEffect == null)
                return null;

            var effect = EffectModels.FirstOrDefault(e => e.Name == General.Default.LastEffect);

            // Fall back to the first effect found, in case settings are messed up
            return effect ?? EffectModels.First();
        }

        /// <summary>
        ///     Disables the current effect and changes it to the provided effect.
        /// </summary>
        /// <param name="effectModel"></param>
        public void ChangeEffect(EffectModel effectModel)
        {
            if (effectModel is OverlayModel)
                throw new ArgumentException("Can't set an Overlay effect as the active effect");

            // Game models are only used if they are enabled
            var gameModel = effectModel as GameModel;
            if (gameModel != null)
                if (!gameModel.Enabled)
                    return;

            if (ActiveEffect != null)
                if (effectModel.Name == ActiveEffect.Name)
                    return;

            // If the main manager is running, pause it and safely change the effect
            if (_mainManager.Running)
            {
                ChangeEffectWithPause(effectModel);
                return;
            }

            // If it's not running, change the effect and start it afterwards.
            ActiveEffect = effectModel;
            ActiveEffect.Enable();

            _mainManager.Start(effectModel);

            if (ActiveEffect is GameModel)
                return;

            // Non-game effects are stored as the new LastEffect.
            General.Default.LastEffect = ActiveEffect.Name;
            General.Default.Save();

            // Let the ViewModels know
            _events.PublishOnUIThread(new ActiveEffectChanged(ActiveEffect.Name));
        }

        private void ChangeEffectWithPause(EffectModel effectModel)
        {
            _mainManager.Pause(effectModel);
            _mainManager.PauseCallback += MainManagerOnPauseCallback;
        }

        private void MainManagerOnPauseCallback(EffectModel callbackEffect)
        {
            
        }

        /// <summary>
        ///     Clears the current effect
        /// </summary>
        public void ClearEffect()
        {
            if (ActiveEffect == null)
                return;

            ActiveEffect.Dispose();
            ActiveEffect = null;

            General.Default.LastEffect = null;
            General.Default.Save();
        }

        /// <summary>
        ///     Disables the given game
        /// </summary>
        /// <param name="activeEffect"></param>
        public void DisableGame(EffectModel activeEffect)
        {
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