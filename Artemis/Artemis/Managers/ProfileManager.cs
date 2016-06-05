﻿using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Artemis.Models;
using Artemis.Modules.Effects.ProfilePreview;
using Artemis.Settings;
using Artemis.ViewModels.Abstract;
using Ninject.Extensions.Logging;

namespace Artemis.Managers
{
    public class ProfileManager
    {
        private readonly DeviceManager _deviceManager;
        private readonly EffectManager _effectManager;
        private readonly ILogger _logger;
        private readonly LoopManager _loopManager;
        private EffectModel _prePreviewEffect;

        public ProfileManager(ILogger logger, EffectManager effectManager, DeviceManager deviceManager,
            LoopManager loopManager)
        {
            _logger = logger;
            _effectManager = effectManager;
            _deviceManager = deviceManager;
            _loopManager = loopManager;

            GameViewModels = new List<GameViewModel>();

            var profilePreviewTimer = new Timer(500);
            profilePreviewTimer.Elapsed += SetupProfilePreview;
            profilePreviewTimer.Start();
        }

        public ProfilePreviewModel ProfilePreviewModel { get; set; }

        public List<GameViewModel> GameViewModels { get; set; }

        /// <summary>
        ///     Keeps track of profiles being previewed and sets up the active efffect accordingly
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetupProfilePreview(object sender, ElapsedEventArgs e)
        {
            if (string.IsNullOrEmpty(General.Default.LastKeyboard) || _deviceManager.ChangingKeyboard ||
                ProfilePreviewModel == null)
                return;

            var activePreview = GameViewModels.FirstOrDefault(vm => vm.IsActive);
            if (activePreview == null)
            {
                // Should not be active if no selected profile is set
                if (_effectManager.ActiveEffect != ProfilePreviewModel)
                    return;

                if (_prePreviewEffect != null)
                {
                    _logger.Debug("Change back effect after profile preview");
                    _effectManager.ChangeEffect(_prePreviewEffect);
                }
                else
                {
                    _logger.Debug("Clear effect after profile preview");
                    _effectManager.ClearEffect();
                }
            }
            else
            {
                if (_effectManager.ActiveEffect != ProfilePreviewModel)
                {
                    _logger.Debug("Activate profile preview");
                    _prePreviewEffect = _effectManager.ActiveEffect;
                    _effectManager.ChangeEffect(ProfilePreviewModel);
                }

                // LoopManager might be running, this method won't do any harm in that case.
                _loopManager.Start();

                if (!ReferenceEquals(ProfilePreviewModel.SelectedProfile, activePreview.ProfileEditor.SelectedProfile))
                    ProfilePreviewModel.SelectedProfile = activePreview.ProfileEditor.SelectedProfile;
            }
        }
    }
}