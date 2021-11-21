using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Timers;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Shared;
using ReactiveUI;

namespace Artemis.UI.Screens.Debugger.Tabs.Performance
{
    public class PerformanceDebugViewModel : ActivatableViewModelBase, IRoutableViewModel
    {
        private readonly IPluginManagementService _pluginManagementService;

        public PerformanceDebugViewModel(IScreen hostScreen, IPluginManagementService pluginManagementService)
        {
            HostScreen = hostScreen;
            _pluginManagementService = pluginManagementService;

            Timer updateTimer = new(500);
            updateTimer.Elapsed += UpdateTimerOnElapsed;

            this.WhenActivated(disposables =>
            {
                Observable.FromEventPattern<PluginFeatureEventArgs>(x => pluginManagementService.PluginFeatureEnabled += x, x => pluginManagementService.PluginFeatureEnabled -= x)
                    .Subscribe(_ => Repopulate())
                    .DisposeWith(disposables);
                Observable.FromEventPattern<PluginFeatureEventArgs>(x => pluginManagementService.PluginFeatureDisabled += x, x => pluginManagementService.PluginFeatureDisabled -= x)
                    .Subscribe(_ => Repopulate())
                    .DisposeWith(disposables);
                Observable.FromEventPattern<PluginEventArgs>(x => pluginManagementService.PluginEnabled += x, x => pluginManagementService.PluginEnabled -= x)
                    .Subscribe(_ => Repopulate())
                    .DisposeWith(disposables);
                Observable.FromEventPattern<PluginEventArgs>(x => pluginManagementService.PluginDisabled += x, x => pluginManagementService.PluginDisabled -= x)
                    .Subscribe(_ => Repopulate())
                    .DisposeWith(disposables);

                PopulateItems();
                updateTimer.Start();

                Disposable.Create(() =>
                {
                    updateTimer.Stop();
                    Items.Clear();
                }).DisposeWith(disposables);
            });
        }

        public ObservableCollection<PerformanceDebugPluginViewModel> Items { get; } = new();
        
        public string UrlPathSegment => "performance";
        public IScreen HostScreen { get; }

        private void PopulateItems()
        {
            foreach (PerformanceDebugPluginViewModel performanceDebugPluginViewModel in _pluginManagementService.GetAllPlugins()
                .Where(p => p.IsEnabled && p.Profilers.Any(pr => pr.Measurements.Any()))
                .OrderBy(p => p.Info.Name)
                .Select(p => new PerformanceDebugPluginViewModel(p)))
                Items.Add(performanceDebugPluginViewModel);
        }

        private void Repopulate()
        {
            Items.Clear();
            PopulateItems();
        }

        private void UpdateTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            foreach (PerformanceDebugPluginViewModel viewModel in Items)
                viewModel.Update();
        }
    }
}