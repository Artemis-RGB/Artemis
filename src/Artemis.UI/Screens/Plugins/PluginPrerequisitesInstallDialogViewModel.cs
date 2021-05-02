using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Shared.Services;
using MaterialDesignThemes.Wpf;
using Stylet;

namespace Artemis.UI.Screens.Plugins
{
    public class PluginPrerequisitesInstallDialogViewModel : DialogViewModelBase
    {
        private readonly IDialogService _dialogService;
        private readonly List<IPrerequisitesSubject> _subjects;
        private PluginPrerequisiteViewModel _activePrerequisite;
        private bool _canInstall;
        private bool _isFinished;
        private CancellationTokenSource _tokenSource;

        public PluginPrerequisitesInstallDialogViewModel(List<IPrerequisitesSubject> subjects, IPrerequisitesVmFactory prerequisitesVmFactory, IDialogService dialogService)
        {
            _subjects = subjects;
            _dialogService = dialogService;

            Prerequisites = new BindableCollection<PluginPrerequisiteViewModel>();
            foreach (IPrerequisitesSubject prerequisitesSubject in subjects)
                Prerequisites.AddRange(prerequisitesSubject.Prerequisites.Select(p => prerequisitesVmFactory.PluginPrerequisiteViewModel(p, false)));

            foreach (PluginPrerequisiteViewModel pluginPrerequisiteViewModel in Prerequisites)
                pluginPrerequisiteViewModel.ConductWith(this);
        }

        public BindableCollection<PluginPrerequisiteViewModel> Prerequisites { get; }

        public PluginPrerequisiteViewModel ActivePrerequisite
        {
            get => _activePrerequisite;
            set => SetAndNotify(ref _activePrerequisite, value);
        }

        public bool CanInstall
        {
            get => _canInstall;
            set => SetAndNotify(ref _canInstall, value);
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

        public async void Install()
        {
            CanInstall = false;
            _tokenSource = new CancellationTokenSource();

            try
            {
                foreach (PluginPrerequisiteViewModel pluginPrerequisiteViewModel in Prerequisites)
                {
                    pluginPrerequisiteViewModel.IsMet = pluginPrerequisiteViewModel.PluginPrerequisite.IsMet();
                    if (pluginPrerequisiteViewModel.IsMet)
                        continue;

                    ActivePrerequisite = pluginPrerequisiteViewModel;
                    await ActivePrerequisite.Install(_tokenSource.Token);

                    // Wait after the task finished for the user to process what happened
                    if (pluginPrerequisiteViewModel != Prerequisites.Last())
                        await Task.Delay(1000);
                }

                if (Prerequisites.All(p => p.IsMet))
                {
                    IsFinished = true;
                    return;
                }

                // This shouldn't be happening and the experience isn't very nice for the user (too lazy to make a nice UI for such an edge case)
                // but at least give some feedback
                Session?.Close(false);
                await _dialogService.ShowConfirmDialog(
                    "Plugin prerequisites",
                    "All prerequisites are installed but some still aren't met. \r\nPlease try again or contact the plugin creator.",
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
                CanInstall = true;
                _tokenSource.Dispose();
                _tokenSource = null;
            }
        }

        public void Accept()
        {
            Session?.Close(true);
        }

        public static Task<object> Show(IDialogService dialogService, List<IPrerequisitesSubject> subjects)
        {
            return dialogService.ShowDialog<PluginPrerequisitesInstallDialogViewModel>(new Dictionary<string, object> {{"subjects", subjects}});
        }

        #region Overrides of Screen

        /// <inheritdoc />
        protected override void OnInitialActivate()
        {
            CanInstall = false;
            Task.Run(() => CanInstall = Prerequisites.Any(p => !p.PluginPrerequisite.IsMet()));

            base.OnInitialActivate();
        }

        #endregion
    }
}