using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Shared.Services;
using MaterialDesignThemes.Wpf;
using Stylet;

namespace Artemis.UI.Screens.Plugins
{
    public class PluginPrerequisitesUninstallDialogViewModel : DialogViewModelBase
    {
        private readonly IDialogService _dialogService;
        private readonly IPluginManagementService _pluginManagementService;
        private readonly List<IPrerequisitesSubject> _subjects;
        private PluginPrerequisiteViewModel _activePrerequisite;
        private bool _canUninstall;
        private bool _isFinished;
        private CancellationTokenSource _tokenSource;

        public PluginPrerequisitesUninstallDialogViewModel(List<IPrerequisitesSubject> subjects, string cancelLabel, IPrerequisitesVmFactory prerequisitesVmFactory,
            IDialogService dialogService, IPluginManagementService pluginManagementService)
        {
            _subjects = subjects;
            _dialogService = dialogService;
            _pluginManagementService = pluginManagementService;

            CancelLabel = cancelLabel;
            Prerequisites = new BindableCollection<PluginPrerequisiteViewModel>();
            foreach (IPrerequisitesSubject prerequisitesSubject in subjects)
                Prerequisites.AddRange(prerequisitesSubject.Prerequisites.Select(p => prerequisitesVmFactory.PluginPrerequisiteViewModel(p, true)));

            foreach (PluginPrerequisiteViewModel pluginPrerequisiteViewModel in Prerequisites)
                pluginPrerequisiteViewModel.ConductWith(this);
        }

        public string CancelLabel { get; }
        public BindableCollection<PluginPrerequisiteViewModel> Prerequisites { get; }

        public PluginPrerequisiteViewModel ActivePrerequisite
        {
            get => _activePrerequisite;
            set => SetAndNotify(ref _activePrerequisite, value);
        }

        public bool CanUninstall
        {
            get => _canUninstall;
            set => SetAndNotify(ref _canUninstall, value);
        }

        public bool IsFinished
        {
            get => _isFinished;
            set => SetAndNotify(ref _isFinished, value);
        }

        #region Overrides of DialogViewModelBase

        /// <inheritdoc />
        public override void OnDialogClosed(object sender, DialogClosingEventArgs e)
        {
            _tokenSource?.Cancel();
            base.OnDialogClosed(sender, e);
        }

        #endregion

        public async void Uninstall()
        {
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
                if (prerequisitesSubject is not PluginFeatureInfo featureInfo) 
                    continue;

                // Disable the parent plugin if the feature is AlwaysEnabled
                if (featureInfo.AlwaysEnabled)
                    _pluginManagementService.DisablePlugin(featureInfo.Plugin, true);
                else if (featureInfo.Instance != null) 
                    _pluginManagementService.DisablePluginFeature(featureInfo.Instance, true);
            }

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
                        await Task.Delay(1000);
                }

                if (Prerequisites.All(p => !p.IsMet))
                {
                    IsFinished = true;
                    return;
                }

                // This shouldn't be happening and the experience isn't very nice for the user (too lazy to make a nice UI for such an edge case)
                // but at least give some feedback
                Session?.Close(false);
                await _dialogService.ShowConfirmDialog(
                    "Plugin prerequisites",
                    "The plugin was not able to fully remove all prerequisites. \r\nPlease try again or contact the plugin creator.",
                    "Confirm",
                    ""
                );
                await Show(_dialogService, _subjects);
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
            Session?.Close(true);
        }

        public static Task<object> Show(IDialogService dialogService, List<IPrerequisitesSubject> subjects, string cancelLabel = "CANCEL")
        {
            return dialogService.ShowDialog<PluginPrerequisitesUninstallDialogViewModel>(new Dictionary<string, object>
            {
                {"subjects", subjects},
                {"cancelLabel", cancelLabel},
            });
        }

        #region Overrides of Screen

        /// <inheritdoc />
        protected override void OnInitialActivate()
        {
            CanUninstall = false;
            // Could be slow so take it off of the UI thread
            Task.Run(() => CanUninstall = Prerequisites.Any(p => p.PluginPrerequisite.IsMet()));

            base.OnInitialActivate();
        }

        #endregion
    }
}