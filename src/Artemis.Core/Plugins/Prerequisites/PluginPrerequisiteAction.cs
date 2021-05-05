using System;
using System.Threading;
using System.Threading.Tasks;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents an action that must be taken to install or uninstall a plugin prerequisite
    /// </summary>
    public abstract class PluginPrerequisiteAction : CorePropertyChanged
    {
        private bool _progressIndeterminate;
        private bool _showProgressBar;
        private bool _showSubProgressBar;
        private string? _status;
        private bool _subProgressIndeterminate;

        /// <summary>
        ///     The base constructor for all plugin prerequisite actions
        /// </summary>
        /// <param name="name">The name of the action</param>
        protected PluginPrerequisiteAction(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        #region Implementation of IPluginPrerequisiteAction

        /// <summary>
        ///     Gets the name of the action
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Gets or sets the status of the action
        /// </summary>
        public string? Status
        {
            get => _status;
            set => SetAndNotify(ref _status, value);
        }

        /// <summary>
        ///     Gets or sets a boolean indicating whether the progress is indeterminate or not
        /// </summary>
        public bool ProgressIndeterminate
        {
            get => _progressIndeterminate;
            set => SetAndNotify(ref _progressIndeterminate, value);
        }

        /// <summary>
        ///     Gets or sets a boolean indicating whether the progress is indeterminate or not
        /// </summary>
        public bool SubProgressIndeterminate
        {
            get => _subProgressIndeterminate;
            set => SetAndNotify(ref _subProgressIndeterminate, value);
        }

        /// <summary>
        ///     Gets or sets a boolean indicating whether the progress bar should be shown
        /// </summary>
        public bool ShowProgressBar
        {
            get => _showProgressBar;
            set => SetAndNotify(ref _showProgressBar, value);
        }

        /// <summary>
        ///     Gets or sets a boolean indicating whether the sub progress bar should be shown
        /// </summary>
        public bool ShowSubProgressBar
        {
            get => _showSubProgressBar;
            set => SetAndNotify(ref _showSubProgressBar, value);
        }

        /// <summary>
        ///     Gets or sets the progress of the action (0 to 100)
        /// </summary>
        public PrerequisiteActionProgress Progress { get; } = new();

        /// <summary>
        ///     Gets or sets the sub progress of the action
        /// </summary>
        public PrerequisiteActionProgress SubProgress { get; } = new();

        /// <summary>
        ///     Called when the action must execute
        /// </summary>
        public abstract Task Execute(CancellationToken cancellationToken);

        #endregion
    }
}