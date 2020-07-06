using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Artemis.UI.Shared.Services.Dialog;
using MaterialDesignThemes.Wpf;
using Ninject.Parameters;

namespace Artemis.UI.Shared.Services.Interfaces
{
    public interface IDialogService : IArtemisSharedUIService
    {
        /// <summary>
        ///     Shows a confirm dialog on the dialog host provided in <see cref="identifier" />.
        /// </summary>
        /// <param name="header">The title of the dialog</param>
        /// <param name="text">The body text of the dialog</param>
        /// <param name="confirmText">The text of the confirm button, defaults to "Confirm"</param>
        /// <param name="cancelText">The text of the cancel button, defaults to "Cancel"</param>
        /// <returns>A task that resolves to true if confirmed and false if cancelled</returns>
        Task<bool> ShowConfirmDialog(string header, string text, string confirmText = "Confirm", string cancelText = "Cancel");

        /// <summary>
        ///     Shows a confirm dialog on the dialog host provided in <see cref="identifier" />.
        /// </summary>
        /// <param name="identifier">
        ///     The identifier of the <see cref="DialogHost" /> to use eg.
        ///     <code>&lt;materialDesign:DialogHost Identifier="MyDialogHost"&gt;</code>
        /// </param>
        /// <param name="header">The title of the dialog</param>
        /// <param name="text">The body text of the dialog</param>
        /// <param name="confirmText">The text of the confirm button, defaults to "Confirm"</param>
        /// <param name="cancelText">The text of the cancel button, defaults to "Cancel"</param>
        /// <returns>A task that resolves to true if confirmed and false if cancelled</returns>
        Task<bool> ShowConfirmDialogAt(string identifier, string header, string text, string confirmText = "Confirm", string cancelText = "Cancel");

        /// <summary>
        ///     Shows a dialog by initializing a view model implementing <see cref="DialogViewModelBase" />
        /// </summary>
        /// <typeparam name="T">The type of the view model</typeparam>
        /// <returns>A task resolving to the result of the dialog's <see cref="DialogSession" /></returns>
        Task<object> ShowDialog<T>() where T : DialogViewModelBase;

        /// <summary>
        ///     Shows a dialog by initializing a view model implementing <see cref="DialogViewModelBase" /> with arguments passed
        ///     to the view models constructor
        /// </summary>
        /// <typeparam name="T">The type of the view model</typeparam>
        /// <param name="parameters">A dictionary of constructor arguments to pass to the view model</param>
        /// <returns>A task resolving to the result of the dialog's <see cref="DialogSession" /></returns>
        Task<object> ShowDialog<T>(Dictionary<string, object> parameters) where T : DialogViewModelBase;

        /// <summary>
        ///     Shows a dialog by initializing a view model implementing <see cref="DialogViewModelBase" /> using an array of
        ///     Ninject <see cref="IParameter" />, requires you to reference Ninject
        /// </summary>
        /// <typeparam name="T">The type of the view model</typeparam>
        /// <param name="parameters">An array of Ninject <see cref="IParameter" /> to pass to the view model during activation</param>
        /// <returns>A task resolving to the result of the dialog's <see cref="DialogSession" /></returns>
        Task<object> ShowDialog<T>(IParameter[] parameters) where T : DialogViewModelBase;

        /// <summary>
        ///     Shows a dialog by initializing a view model implementing <see cref="DialogViewModelBase" />
        /// </summary>
        /// <typeparam name="T">The type of the view model</typeparam>
        /// <param name="identifier">
        ///     The identifier of the <see cref="DialogHost" /> to use eg.
        ///     <code>&lt;materialDesign:DialogHost Identifier="MyDialogHost"&gt;</code>
        /// </param>
        /// <returns>A task resolving to the result of the dialog's <see cref="DialogSession" /></returns>
        Task<object> ShowDialogAt<T>(string identifier) where T : DialogViewModelBase;

        /// <summary>
        ///     Shows a dialog by initializing a view model implementing <see cref="DialogViewModelBase" /> with arguments passed
        ///     to the view models constructor
        /// </summary>
        /// <typeparam name="T">The type of the view model</typeparam>
        /// <param name="identifier">
        ///     The identifier of the <see cref="DialogHost" /> to use eg.
        ///     <code>&lt;materialDesign:DialogHost Identifier="MyDialogHost"&gt;</code>
        /// </param>
        /// <param name="parameters">A dictionary of constructor arguments to pass to the view model</param>
        /// <returns>A task resolving to the result of the dialog's <see cref="DialogSession" /></returns>
        Task<object> ShowDialogAt<T>(string identifier, Dictionary<string, object> parameters) where T : DialogViewModelBase;

        /// <summary>
        ///     Shows a dialog by initializing a view model implementing <see cref="DialogViewModelBase" /> using an array of
        ///     Ninject <see cref="IParameter" />, requires you to reference Ninject
        /// </summary>
        /// <typeparam name="T">The type of the view model</typeparam>
        /// <param name="identifier">
        ///     The identifier of the <see cref="DialogHost" /> to use eg.
        ///     <code>&lt;materialDesign:DialogHost Identifier="MyDialogHost"&gt;</code>
        /// </param>
        /// <param name="parameters">An array of Ninject <see cref="IParameter" /> to pass to the view model during activation</param>
        /// <returns>A task resolving to the result of the dialog's <see cref="DialogSession" /></returns>
        Task<object> ShowDialogAt<T>(string identifier, IParameter[] parameters) where T : DialogViewModelBase;

        /// <summary>
        ///     Shows a dialog displaying the provided message and exception. Does not handle, log or throw the exception.
        /// </summary>
        /// <param name="message">The message to display in the dialog title</param>
        /// <param name="exception">The exception to display. The exception message and stacktrace will be shown.</param>
        /// <returns>A task resolving when the dialog is closed</returns>
        Task ShowExceptionDialog(string message, Exception exception);

        bool IsExceptionDialogOpen { get; }
    }
}