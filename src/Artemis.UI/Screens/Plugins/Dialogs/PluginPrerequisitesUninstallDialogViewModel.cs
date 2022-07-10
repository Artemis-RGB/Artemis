using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using ReactiveUI;
using ContentDialogButton = Artemis.UI.Shared.Services.Builders.ContentDialogButton;

namespace Artemis.UI.Screens.Plugins
{
    public class PluginPrerequisitesUninstallDialogViewModel : ContentDialogViewModelBase
    {
        private readonly IPluginManagementService _pluginManagementService;
        private readonly List<IPrerequisitesSubject> _subjects;
        private readonly IWindowService _windowService;
        private PluginPrerequisiteViewModel? _activePrerequisite;
        private bool _canUninstall;
        private CancellationTokenSource? _tokenSource;

        public PluginPrerequisitesUninstallDialogViewModel(List<IPrerequisitesSubject> subjects,
            IPrerequisitesVmFactory prerequisitesVmFactory,
            IWindowService windowService,
            IPluginManagementService pluginManagementService)
        {
            _subjects = subjects;
            _windowService = windowService;
            _pluginManagementService = pluginManagementService;

            Prerequisites = new ObservableCollection<PluginPrerequisiteViewModel>();
            foreach (PluginPrerequisite prerequisite in subjects.SelectMany(prerequisitesSubject => prerequisitesSubject.PlatformPrerequisites))
                Prerequisites.Add(prerequisitesVmFactory.PluginPrerequisiteViewModel(prerequisite, true));
            Uninstall = ReactiveCommand.CreateFromTask(ExecuteUninstall, this.WhenAnyValue(vm => vm.CanUninstall));

            // Could be slow so take it off of the UI thread
            Dispatcher.UIThread.Post(() => CanUninstall = Prerequisites.Any(p => p.PluginPrerequisite.IsMet()), DispatcherPriority.Background);

            this.WhenActivated(d =>
            {
                Disposable.Create(() =>
                {
                    _tokenSource?.Cancel();
                    _tokenSource?.Dispose();
                    _tokenSource = null;
                }).DisposeWith(d);
            });
        }

        public ReactiveCommand<Unit, Unit> Uninstall { get; }
        public ObservableCollection<PluginPrerequisiteViewModel> Prerequisites { get; }

        public PluginPrerequisiteViewModel? ActivePrerequisite
        {
            get => _activePrerequisite;
            set => RaiseAndSetIfChanged(ref _activePrerequisite, value);
        }

        public bool CanUninstall
        {
            get => _canUninstall;
            set => RaiseAndSetIfChanged(ref _canUninstall, value);
        }

        private async Task ExecuteUninstall()
        {
            ContentDialogClosingDeferral? deferral = null;
            if (ContentDialog != null)
                ContentDialog.Closing += (_, args) => deferral = args.GetDeferral();

            CanUninstall = false;

            // Disable all subjects that are plugins, this will disable their features too
            foreach (IPrerequisitesSubject prerequisitesSubject in _subjects)
            {
                if (prerequisitesSubject is PluginInfo pluginInfo)
                    _pluginManagementService.DisablePlugin(pluginInfo.Plugin, true);
            }

            // Disable all subjects that are features if still required
            foreach (IPrerequisitesSubject prerequisitesSubject in _subjects)
            {
                if (prerequisitesSubject is not PluginFeatureInfo featureInfo) continue;

                // Disable the parent plugin if the feature is AlwaysEnabled
                if (featureInfo.AlwaysEnabled)
                    _pluginManagementService.DisablePlugin(featureInfo.Plugin, true);
                else if (featureInfo.Instance != null)
                    _pluginManagementService.DisablePluginFeature(featureInfo.Instance, true);
            }

            _tokenSource?.Dispose();
            _tokenSource = new CancellationTokenSource();

            try
            {
                foreach (PluginPrerequisiteViewModel pluginPrerequisiteViewModel in Prerequisites)
                {
                    pluginPrerequisiteViewModel.IsMet = pluginPrerequisiteViewModel.PluginPrerequisite.IsMet();
                    if (!pluginPrerequisiteViewModel.IsMet)
                        continue;

                    ActivePrerequisite = pluginPrerequisiteViewModel;
                    await ActivePrerequisite.Uninstall(_tokenSource.Token);

                    // Wait after the task finished for the user to process what happened
                    if (pluginPrerequisiteViewModel != Prerequisites.Last())
                        await Task.Delay(250);
                    else
                        await Task.Delay(1000);
                }

                if (deferral != null)
                    deferral.Complete();
                else
                    ContentDialog?.Hide(ContentDialogResult.Primary);
                
                // This shouldn't be happening and the experience isn't very nice for the user (too lazy to make a nice UI for such an edge case)
                // but at least give some feedback
                if (Prerequisites.Any(p => p.IsMet))
                {
                    await _windowService.CreateContentDialog()
                        .WithTitle("Plugin prerequisites")
                        .WithContent("The plugin was not able to fully remove all prerequisites. \r\nPlease try again or contact the plugin creator.")
                        .ShowAsync();
                    await Show(_windowService, _subjects);
                }
            }
            catch (OperationCanceledException)
            {
                // ignored
            }
            finally
            {
                CanUninstall = true;
            }
        }

        public static async Task Show(IWindowService windowService, List<IPrerequisitesSubject> subjects, string cancelLabel = "Cancel")
        {
            await windowService.CreateContentDialog()
                .WithTitle("Plugin prerequisites")
                .WithViewModel(out PluginPrerequisitesUninstallDialogViewModel vm, ("subjects", subjects))
                .WithCloseButtonText(cancelLabel)
                .HavingPrimaryButton(b => b.WithText("Uninstall").WithCommand(vm.Uninstall))
                .WithDefaultButton(ContentDialogButton.Primary)
                .ShowAsync();
        }
    }
}