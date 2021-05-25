using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Shared.Services;
using Stylet;

namespace Artemis.UI.Screens.Plugins
{
    public class PluginPrerequisiteViewModel : Conductor<PluginPrerequisiteActionViewModel>.Collection.OneActive
    {
        private readonly bool _uninstall;
        private bool _installing;
        private bool _uninstalling;
        private bool _isMet;

        public PluginPrerequisiteViewModel(PluginPrerequisite pluginPrerequisite, bool uninstall)
        {
            _uninstall = uninstall;
            PluginPrerequisite = pluginPrerequisite;
        }

        public PluginPrerequisite PluginPrerequisite { get; }

        public bool Installing
        {
            get => _installing;
            set
            {
                SetAndNotify(ref _installing, value);
                NotifyOfPropertyChange(nameof(Busy));
            }
        }

        public bool Uninstalling
        {
            get => _uninstalling;
            set
            {
                SetAndNotify(ref _uninstalling, value);
                NotifyOfPropertyChange(nameof(Busy));
            }
        }

        public bool IsMet
        {
            get => _isMet;
            set => SetAndNotify(ref _isMet, value);
        }

        public bool Busy => Installing || Uninstalling;
        public int ActiveStemNumber => Items.IndexOf(ActiveItem) + 1;
        public bool HasMultipleActions => Items.Count > 1;

        public async Task Install(CancellationToken cancellationToken)
        {
            if (Busy)
                return;

            Installing = true;
            try
            {
                await PluginPrerequisite.Install(cancellationToken);
            }
            finally
            {
                Installing = false;
                IsMet = PluginPrerequisite.IsMet();
            }
        }

        public async Task Uninstall(CancellationToken cancellationToken)
        {
            if (Busy)
                return;

            Uninstalling = true;
            try
            {
                await PluginPrerequisite.Uninstall(cancellationToken);
            }
            finally
            {
                Uninstalling = false;
                IsMet = PluginPrerequisite.IsMet();
            }
        }

        private void PluginPrerequisiteOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PluginPrerequisite.CurrentAction))
                ActivateCurrentAction();
        }

        private void ActivateCurrentAction()
        {
            PluginPrerequisiteActionViewModel newActiveItem = Items.FirstOrDefault(i => i.Action == PluginPrerequisite.CurrentAction);
            if (newActiveItem == null)
                return;

            ActiveItem = newActiveItem;
            NotifyOfPropertyChange(nameof(ActiveStemNumber));
        }

        #region Overrides of Screen

        /// <inheritdoc />
        protected override void OnClose()
        {
            PluginPrerequisite.PropertyChanged -= PluginPrerequisiteOnPropertyChanged;
            base.OnClose();
        }

        /// <inheritdoc />
        protected override void OnInitialActivate()
        {
            PluginPrerequisite.PropertyChanged += PluginPrerequisiteOnPropertyChanged;
            // Could be slow so take it off of the UI thread
            Task.Run(() => IsMet = PluginPrerequisite.IsMet());

            Items.AddRange(!_uninstall
                ? PluginPrerequisite.InstallActions.Select(a => new PluginPrerequisiteActionViewModel(a))
                : PluginPrerequisite.UninstallActions.Select(a => new PluginPrerequisiteActionViewModel(a)));

            base.OnInitialActivate();
        }

        #endregion
    }
}