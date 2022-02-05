using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services.Interfaces;
using ReactiveUI;

namespace Artemis.UI.Screens.Plugins
{
    public class PluginPrerequisitesUninstallDialogViewModel : DialogViewModelBase<bool>
    {
        private readonly IPluginManagementService _pluginManagementService;
        private readonly List<IPrerequisitesSubject> _subjects;
        private readonly IWindowService _windowService;
        private PluginPrerequisiteViewModel? _activePrerequisite;
        private bool _canUninstall;
        private bool _isFinished;
        private CancellationTokenSource? _tokenSource;

        public PluginPrerequisitesUninstallDialogViewModel(List<IPrerequisitesSubject> subjects, string cancelLabel, IPrerequisitesVmFactory prerequisitesVmFactory, IWindowService windowService,
            IPluginManagementService pluginManagementService)
        {
            _subjects = subjects;
            _windowService = windowService;
            _pluginManagementService = pluginManagementService;

            CancelLabel = cancelLabel;
            Prerequisites = new ObservableCollection<PluginPrerequisiteViewModel>();
            foreach (PluginPrerequisite prerequisite in subjects.SelectMany(prerequisitesSubject => prerequisitesSubject.Prerequisites))
                Prerequisites.Add(prerequisitesVmFactory.PluginPrerequisiteViewModel(prerequisite, true));

            // Could be slow so take it off of the UI thread
            Task.Run(() => CanUninstall = Prerequisites.Any(p => p.PluginPrerequisite.IsMet()));

            this.WhenActivated(d =>
            {
                Disposable.Create(() =>
                {
                    _tokenSource?.Cancel();
                    _tokenSource?.Dispose();
                }).DisposeWith(d);
            });
        }

        public string CancelLabel { get; }
        public ObservableCollection<PluginPrerequisiteViewModel> Prerequisites { get; }

        public PluginPrerequisiteViewModel? ActivePrerequisite
        {
            get => _activePrerequisite;
            set => this.RaiseAndSetIfChanged(ref _activePrerequisite, value);
        }

        public bool CanUninstall
        {
            get => _canUninstall;
            set => this.RaiseAndSetIfChanged(ref _canUninstall, value);
        }

        public bool IsFinished
        {
            get => _isFinished;
            set => this.RaiseAndSetIfChanged(ref _isFinished, value);
        }

        public async Task Uninstall()
        {
            CanUninstall = false;

            // Disable all subjects that are plugins, this will disable their features too
            foreach (IPrerequisitesSubject prerequisitesSubject in _subjects)
            {
                if (prerequisitesSubject is PluginInfo pluginInfo) _pluginManagementService.DisablePlugin(pluginInfo.Plugin, true);
            }

            // Disable all subjects that are features if still required
            foreach (IPrerequisitesSubject prerequisitesSubject in _subjects)
            {
                if (prerequisitesSubject is not PluginFeatureInfo featureInfo) continue;

                // Disable the parent plugin if the feature is AlwaysEnabled
                if (featureInfo.AlwaysEnabled)
                    _pluginManagementService.DisablePlugin(featureInfo.Plugin, true);
                else if (featureInfo.Instance != null) _pluginManagementService.DisablePluginFeature(featureInfo.Instance, true);
            }

            _tokenSource = new CancellationTokenSource();

            try
            {
                foreach (PluginPrerequisiteViewModel pluginPrerequisiteViewModel in Prerequisites)
                {
                    pluginPrerequisiteViewModel.IsMet = pluginPrerequisiteViewModel.PluginPrerequisite.IsMet();
                    if (!pluginPrerequisiteViewModel.IsMet) continue;

                    ActivePrerequisite = pluginPrerequisiteViewModel;
                    await ActivePrerequisite.Uninstall(_tokenSource.Token);

                    // Wait after the task finished for the user to process what happened
                    if (pluginPrerequisiteViewModel != Prerequisites.Last()) await Task.Delay(1000);
                }

                if (Prerequisites.All(p => !p.IsMet))
                {
                    IsFinished = true;
                    return;
                }

                // This shouldn't be happening and the experience isn't very nice for the user (too lazy to make a nice UI for such an edge case)
                // but at least give some feedback
                Close(false);
                await _windowService.CreateContentDialog()
                    .WithTitle("Plugin prerequisites")
                    .WithContent("The plugin was not able to fully remove all prerequisites. \r\nPlease try again or contact the plugin creator.")
                    .ShowAsync();
                await Show(_windowService, _subjects);
            }
            catch (OperationCanceledException)
            {
                // ignored
            }
            finally
            {
                CanUninstall = true;
                _tokenSource.Dispose();
                _tokenSource = null;
            }
        }

        public void Accept()
        {
            Close(true);
        }

        public static async Task<object> Show(IWindowService windowService, List<IPrerequisitesSubject> subjects, string cancelLabel = "Cancel")
        {
            return await windowService.ShowDialogAsync<PluginPrerequisitesUninstallDialogViewModel, bool>(("subjects", subjects), ("cancelLabel", cancelLabel));
        }
    }
}