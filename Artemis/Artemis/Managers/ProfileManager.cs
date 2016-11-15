using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Artemis.DAL;
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
        private GeneralSettings _generalSettings;

        public ProfileManager(ILogger logger, EffectManager effectManager, DeviceManager deviceManager,
            LoopManager loopManager)
        {
            _logger = logger;
            _effectManager = effectManager;
            _deviceManager = deviceManager;
            _loopManager = loopManager;
            _generalSettings = SettingsProvider.Load<GeneralSettings>();

            GameViewModels = new List<GameViewModel>();

            var profilePreviewTimer = new Timer(500);
            profilePreviewTimer.Elapsed += SetupProfilePreview;
            profilePreviewTimer.Start();

            _logger.Info("Intialized ProfileManager");
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
            if (string.IsNullOrEmpty(_generalSettings.LastKeyboard) || _deviceManager.ChangingKeyboard ||
                ProfilePreviewModel == null)
                return;

            lock (GameViewModels)
            {
                var activePreview = GameViewModels.FirstOrDefault(vm => vm.IsActive);

                if (activePreview == null)
                {
                    // Should not be active if no selected profile is set
                    if (_effectManager.ActiveEffect != ProfilePreviewModel)
                        return;

                    _logger.Debug("Loading last effect after profile preview");
                    var lastEffect = _effectManager.GetLastEffect();
                    if (lastEffect != null)
                        _effectManager.ChangeEffect(lastEffect);
                    else
                        _effectManager.ClearEffect();
                }
                else
                {
                    if (_effectManager.ActiveEffect != ProfilePreviewModel &&
                        !(_effectManager.ActiveEffect is GameModel))
                    {
                        _logger.Debug("Activate profile preview");
                        _effectManager.ChangeEffect(ProfilePreviewModel);
                    }

                    // LoopManager might be running, this method won't do any harm in that case.
                    _loopManager.StartAsync();

                    ProfilePreviewModel.ProfileViewModel = activePreview.ProfileEditor.ProfileViewModel;
                    if (!ReferenceEquals(ProfilePreviewModel.Profile, activePreview.ProfileEditor.SelectedProfile))
                        ProfilePreviewModel.Profile = activePreview.ProfileEditor.SelectedProfile;
                }
            }
        }
    }
}