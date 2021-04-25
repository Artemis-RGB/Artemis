using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.UI.Exceptions;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Shared.Services;
using MaterialDesignThemes.Wpf;
using Stylet;

namespace Artemis.UI.Screens.Plugins
{
    public class PluginPrerequisitesDialogViewModel : DialogViewModelBase
    {
        private PluginPrerequisiteViewModel _activePrerequisite;
        private bool _canInstall;
        private bool _isFinished;
        private CancellationTokenSource _tokenSource;

        public PluginPrerequisitesDialogViewModel(object pluginOrFeature, IPrerequisitesVmFactory prerequisitesVmFactory)
        {
            // Constructor overloading doesn't work very well with Kernel.Get<T> :(
            if (pluginOrFeature is Plugin plugin)
            {
                Plugin = plugin;
                Prerequisites = new BindableCollection<PluginPrerequisiteViewModel>(plugin.Prerequisites.Select(prerequisitesVmFactory.PluginPrerequisiteViewModel));
            }
            else if (pluginOrFeature is PluginFeature feature)
            {
                Feature = feature;
                Prerequisites = new BindableCollection<PluginPrerequisiteViewModel>(feature.Prerequisites.Select(prerequisitesVmFactory.PluginPrerequisiteViewModel));
            }
            else
                throw new ArtemisUIException($"Expected plugin or feature to be passed to {nameof(PluginPrerequisitesDialogViewModel)}");

            foreach (PluginPrerequisiteViewModel pluginPrerequisiteViewModel in Prerequisites)
                pluginPrerequisiteViewModel.ConductWith(this);
        }


        public PluginFeature Feature { get; }
        public Plugin Plugin { get; }
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
                    ActivePrerequisite = pluginPrerequisiteViewModel;
                    ActivePrerequisite.IsMet = await ActivePrerequisite.PluginPrerequisite.IsMet();
                    if (ActivePrerequisite.IsMet)
                        continue;

                    await ActivePrerequisite.Install(_tokenSource.Token);

                    // Wait after the task finished for the user to process what happened
                    if (pluginPrerequisiteViewModel != Prerequisites.Last())
                        await Task.Delay(1000);
                }

                if (Prerequisites.All(p => p.IsMet))
                    IsFinished = true;
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

        #region Overrides of Screen

        /// <inheritdoc />
        protected override void OnInitialActivate()
        {
            base.OnInitialActivate();
            CanInstall = Prerequisites.Any(p => !p.IsMet);
        }

        #endregion
    }
}