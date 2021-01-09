using System;

namespace Artemis.UI.Shared.Services
{
    /// <summary>
    ///     A service that allows you to view, monitor and manage the open/close state of the main window
    /// </summary>
    public interface IWindowService : IArtemisSharedUIService
    {
        /// <summary>
        ///     Gets a boolean indicating whether the main window is currently open
        /// </summary>
        bool IsMainWindowOpen { get; }

        /// <summary>
        ///     Sets up the main window manager that controls the state of the main window
        /// </summary>
        /// <param name="mainWindowManager">The main window manager to use to control the state of the main window</param>
        void ConfigureMainWindowManager(IMainWindowManager mainWindowManager);

        /// <summary>
        ///     Opens the main window if it is not already open
        /// </summary>
        void OpenMainWindow();

        /// <summary>
        ///     Closes the main window if it is not already closed
        /// </summary>
        void CloseMainWindow();

        /// <summary>
        ///     Occurs when the main window has been opened
        /// </summary>
        public event EventHandler? MainWindowOpened;

        /// <summary>
        ///     Occurs when the main window has been closed
        /// </summary>
        public event EventHandler? MainWindowClosed;
    }
}