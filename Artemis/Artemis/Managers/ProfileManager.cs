using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Artemis.DAL;
using Artemis.Modules.Abstract;
using Artemis.Settings;
using Ninject;
using Ninject.Extensions.Logging;

namespace Artemis.Managers
{
    public class ProfileManager
    {
        private readonly ILogger _logger;
        private readonly ModuleManager _moduleManager;
        private readonly DeviceManager _deviceManager;
        private readonly LoopManager _loopManager;
        private readonly GeneralSettings _generalSettings;

        public ProfileManager(ILogger logger, ModuleManager moduleManager, DeviceManager deviceManager,
            LoopManager loopManager)
        {
            _logger = logger;
            _moduleManager = moduleManager;
            _deviceManager = deviceManager;
            _loopManager = loopManager;
            _generalSettings = SettingsProvider.Load<GeneralSettings>();

            PreviewViewModules = new List<ModuleViewModel>();

            var profilePreviewTimer = new Timer(500);
            profilePreviewTimer.Elapsed += SetupProfilePreview;
            profilePreviewTimer.Start();

            _logger.Info("Intialized ProfileManager");
        }

        public List<ModuleViewModel> PreviewViewModules { get; set; }

        /// <summary>
        ///     Keeps track of profiles being previewed and sets up the active efffect accordingly
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetupProfilePreview(object sender, ElapsedEventArgs e)
        {
//            if (string.IsNullOrEmpty(_generalSettings.LastKeyboard) || _deviceManager.ChangingKeyboard)
//                return;
//
//            var activePreview = PreviewViewModules.FirstOrDefault(vm => vm.IsActive);
//            if (activePreview == null)
//            {
//                // Should not be active if no selected profile is set
//                if (_moduleManager.ActiveModule != _profilePreviewModel)
//                    return;
//
//                _logger.Debug("Loading last module after profile preview");
//                var lastModule = _moduleManager.GetLastModule();
//                if (lastModule != null)
//                    _moduleManager.ChangeActiveModule(lastModule);
//                else
//                    _moduleManager.ClearActiveModule();
//            }
//            else
//            {
//                if (_moduleManager.ActiveModule != null && _moduleManager.ActiveModule != _profilePreviewModel &&
//                    _moduleManager.ActiveModule != activePreview.ModuleModel)
//                {
//                    _logger.Debug("Activate profile preview");
//                    _moduleManager.ChangeActiveModule(_profilePreviewModel);
//                }
//
//                // LoopManager might be running, this method won't do any harm in that case.
//                _loopManager.StartAsync();
//
//                // Can safely spam this, it won't do anything if they are equal
//                _profilePreviewModel.ProfileViewModel = activePreview.ProfileEditor.ProfileViewModel;
//                _profilePreviewModel.ChangeProfile(activePreview.ModuleModel.ProfileModel);
//            }
        }
    }
}