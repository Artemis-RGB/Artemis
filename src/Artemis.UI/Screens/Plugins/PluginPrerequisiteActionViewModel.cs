using System;
using System.ComponentModel;
using Artemis.Core;
using Stylet;

namespace Artemis.UI.Screens.Plugins
{
    public class PluginPrerequisiteActionViewModel : Screen
    {
        private bool _showProgressBar;
        private bool _showSubProgressBar;

        public PluginPrerequisiteActionViewModel(PluginPrerequisiteAction action)
        {
            Action = action;
        }

        public PluginPrerequisiteAction Action { get; }

        public bool ShowProgressBar
        {
            get => _showProgressBar;
            set => SetAndNotify(ref _showProgressBar, value);
        }

        public bool ShowSubProgressBar
        {
            get => _showSubProgressBar;
            set => SetAndNotify(ref _showSubProgressBar, value);
        }

        private void ActionOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Action.ProgressIndeterminate) || e.PropertyName == nameof(Action.SubProgressIndeterminate))
                UpdateProgress();
        }

        private void ProgressReported(object? sender, EventArgs e)
        {
            UpdateProgress();
        }
        
        private void UpdateProgress()
        {
            ShowSubProgressBar = Action.SubProgress.Percentage != 0 || Action.SubProgressIndeterminate;
            ShowProgressBar = ShowSubProgressBar || Action.Progress.Percentage != 0 || Action.ProgressIndeterminate;
        }

        #region Overrides of Screen

        /// <inheritdoc />
        protected override void OnInitialActivate()
        {
            Action.Progress.ProgressReported += ProgressReported;
            Action.SubProgress.ProgressReported += ProgressReported;
            Action.PropertyChanged += ActionOnPropertyChanged;
            base.OnInitialActivate();
        }

        /// <inheritdoc />
        protected override void OnClose()
        {
            Action.Progress.ProgressReported -= ProgressReported;
            Action.SubProgress.ProgressReported -= ProgressReported;
            Action.PropertyChanged -= ActionOnPropertyChanged;
            base.OnClose();
        }

        #endregion
    }
}