using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Artemis.DAL;
using Artemis.Modules.Abstract;
using Artemis.Settings;
using Artemis.Utilities;
using Artemis.Utilities.ActiveWindowDetection;
using Ninject.Extensions.Logging;

namespace Artemis.Managers
{
    public class PreviewManager
    {
        private readonly DeviceManager _deviceManager;
        private readonly GeneralSettings _generalSettings;
        private readonly ILogger _logger;
        private readonly LoopManager _loopManager;
        private readonly ModuleManager _moduleManager;

        public PreviewManager(ILogger logger, ModuleManager moduleManager, DeviceManager deviceManager,
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

            _logger.Info("Intialized PreviewManager");
        }

        public List<ModuleViewModel> PreviewViewModules { get; set; }

        /// <summary>
        ///     Keeps track of profiles being previewed and sets up the active efffect accordingly
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetupProfilePreview(object sender, ElapsedEventArgs e)
        {
            if (string.IsNullOrEmpty(_generalSettings.LastKeyboard) || _deviceManager.ChangingKeyboard)
                return;

            // If Artemis doesn't have focus, don't preview unless overwritten by KeepActive
            var activePreview = PreviewViewModules.FirstOrDefault(vm => (vm.IsActive || vm.ProfileEditor.KeepActive) && vm.UsesProfileEditor && vm.ModuleModel.Settings.IsEnabled);
            if (activePreview != null && (activePreview.ProfileEditor.KeepActive || ActiveWindowHelper.MainWindowActive))
                EnsurePreviewActive(activePreview);
            else
                EnsurePreviewInactive();
        }

        private void EnsurePreviewActive(ModuleViewModel toBeActive)
        {
            // If the current module is the same as what should be active, don't do anything
            if (_moduleManager.ActiveModule != null && _moduleManager.ActiveModule == toBeActive.ModuleModel)
                return;

            _logger.Debug("Activate profile preview");
            _moduleManager.ChangeActiveModule(toBeActive.ModuleModel, null, false);

            // LoopManager might be running, this method won't do any harm in that case.
            _loopManager.StartAsync();
        }

        private void EnsurePreviewInactive()
        {
            // Check if the active module is being previewed, if so, that should no longer be happening
            if (_moduleManager.ActiveModule?.PreviewLayers == null)
                return;

            _logger.Debug("Deactivate profile preview");

            // Save the profile the editor was using
            var activePreview = PreviewViewModules.FirstOrDefault(p => p.IsModuleActive);
            activePreview?.ProfileEditor?.SaveSelectedProfile();

            var lastModule = _moduleManager.GetLastModule();
            if (lastModule != null && lastModule.Settings.IsEnabled)
                _moduleManager.ChangeActiveModule(lastModule);
            else
                _moduleManager.ClearActiveModule();
        }
    }
}