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
        private PluginPrerequisiteViewModel _activePrerequisite;
        private CancellationTokenSource _tokenSource;
        private bool _showProgress;
        private bool _showIntro = true;
        private bool _showFailed;
        private bool _showInstall = true;
        private bool _canInstall;

        public PluginPrerequisitesInstallDialogViewModel(List<IPrerequisitesSubject> subjects, IPrerequisitesVmFactory prerequisitesVmFactory, IDialogService dialogService)
        {
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

        public bool ShowProgress
        {
            get => _showProgress;
            set => SetAndNotify(ref _showProgress, value);
        }

        public bool ShowIntro
        {
            get => _showIntro;
            set => SetAndNotify(ref _showIntro, value);
        }

        public bool ShowFailed
        {
            get => _showFailed;
            set => SetAndNotify(ref _showFailed, value);
        }

        public bool ShowInstall
        {
            get => _showInstall;
            set => SetAndNotify(ref _showInstall, value);
        }

        public bool CanInstall
        {
            get => _canInstall;
            set => SetAndNotify(ref _canInstall, value);
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
            ShowFailed = false;
            ShowIntro = false;
            ShowProgress = true;

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

                    if (!ActivePrerequisite.IsMet)
                    {
                        CanInstall = true;
                        ShowFailed = true;
                        ShowProgress = false;
                        return;
                    }

                    // Wait after the task finished for the user to process what happened
                    if (pluginPrerequisiteViewModel != Prerequisites.Last())
                        await Task.Delay(1000);
                }

                ShowInstall = false;
            }
            catch (OperationCanceledException)
            {
                // ignored
            }
            finally
            {
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