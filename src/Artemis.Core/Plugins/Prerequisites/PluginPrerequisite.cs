using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a prerequisite for a <see cref="Plugin" /> or <see cref="PluginFeature" />
    /// </summary>
    public abstract class PluginPrerequisite : CorePropertyChanged
    {
        private PluginPrerequisiteAction? _currentAction;

        /// <summary>
        ///     Gets the name of the prerequisite
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        ///     Gets the description of the prerequisite
        /// </summary>
        public abstract string Description { get; }

        /// <summary>
        ///     Gets a list of actions to execute when <see cref="Install" /> is called
        /// </summary>
        public abstract List<PluginPrerequisiteAction> InstallActions { get; }

        /// <summary>
        ///     Gets a list of actions to execute when <see cref="Uninstall" /> is called
        /// </summary>
        public abstract List<PluginPrerequisiteAction> UninstallActions { get; }

        /// <summary>
        ///     Gets or sets the action currently being executed
        /// </summary>
        public PluginPrerequisiteAction? CurrentAction
        {
            get => _currentAction;
            private set => SetAndNotify(ref _currentAction, value);
        }

        /// <summary>
        ///     Execute all install actions
        /// </summary>
        public async Task Install(CancellationToken cancellationToken)
        {
            try
            {
                OnInstallStarting();
                foreach (PluginPrerequisiteAction installAction in InstallActions)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    CurrentAction = installAction;
                    await installAction.Execute(cancellationToken);
                }
            }
            finally
            {
                CurrentAction = null;
                OnInstallFinished();
            }
        }

        /// <summary>
        ///     Execute all uninstall actions
        /// </summary>
        public async Task Uninstall(CancellationToken cancellationToken)
        {
            try
            {
                OnUninstallStarting();
                foreach (PluginPrerequisiteAction uninstallAction in UninstallActions)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    CurrentAction = uninstallAction;
                    await uninstallAction.Execute(cancellationToken);
                }
            }
            finally
            {
                CurrentAction = null;
                OnUninstallFinished();
            }
        }

        /// <summary>
        ///     Called to determine whether the prerequisite is met
        /// </summary>
        /// <returns><see langword="true" /> if the prerequisite is met; otherwise <see langword="false" /></returns>
        public abstract bool IsMet();

        /// <summary>
        ///     Called before installation starts
        /// </summary>
        protected virtual void OnInstallStarting()
        {
        }

        /// <summary>
        ///     Called after installation finishes
        /// </summary>
        protected virtual void OnInstallFinished()
        {
        }

        /// <summary>
        ///     Called before uninstall starts
        /// </summary>
        protected virtual void OnUninstallStarting()
        {
        }

        /// <summary>
        ///     Called after uninstall finished
        /// </summary>
        protected virtual void OnUninstallFinished()
        {
        }
    }
}