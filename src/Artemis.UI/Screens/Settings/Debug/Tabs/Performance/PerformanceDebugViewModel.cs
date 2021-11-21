using System.Linq;
using System.Timers;
using Artemis.Core;
using Artemis.Core.Services;
using Stylet;

namespace Artemis.UI.Screens.Settings.Debug.Tabs.Performance
{
    public class PerformanceDebugViewModel : Conductor<PerformanceDebugPluginViewModel>.Collection.AllActive
    {
        private readonly IPluginManagementService _pluginManagementService;
        private readonly Timer _updateTimer;

        public PerformanceDebugViewModel(IPluginManagementService pluginManagementService)
        {
            _pluginManagementService = pluginManagementService;
            _updateTimer = new Timer(500);

            DisplayName = "PERFORMANCE";
            _updateTimer.Elapsed += UpdateTimerOnElapsed;
        }

        private void UpdateTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
                foreach (PerformanceDebugPluginViewModel viewModel in Items)
                    viewModel.Update();
        }

        private void FeatureToggled(object sender, PluginFeatureEventArgs e)
        {
            Items.Clear();
            PopulateItems();
        }

        private void PluginToggled(object sender, PluginEventArgs e)
        {
            Items.Clear();
            PopulateItems();
        }

        private void PopulateItems()
        {
            Items.AddRange(_pluginManagementService.GetAllPlugins()
                .Where(p => p.IsEnabled && p.Profilers.Any(pr => pr.Measurements.Any()))
                .OrderBy(p => p.Info.Name)
                .Select(p => new PerformanceDebugPluginViewModel(p)));
        }

        #region Overrides of Screen

        /// <inheritdoc />
        protected override void OnActivate()
        {
            PopulateItems();
            _updateTimer.Start();
            _pluginManagementService.PluginDisabled += PluginToggled;
            _pluginManagementService.PluginDisabled += PluginToggled;
            _pluginManagementService.PluginFeatureEnabled += FeatureToggled;
            _pluginManagementService.PluginFeatureDisabled += FeatureToggled;
            base.OnActivate();
        }

        /// <inheritdoc />
        protected override void OnDeactivate()
        {
            _updateTimer.Stop();
            _pluginManagementService.PluginDisabled -= PluginToggled;
            _pluginManagementService.PluginDisabled -= PluginToggled;
            _pluginManagementService.PluginFeatureEnabled -= FeatureToggled;
            _pluginManagementService.PluginFeatureDisabled -= FeatureToggled;
            Items.Clear();
            base.OnDeactivate();
        }

        #endregion
    }
}