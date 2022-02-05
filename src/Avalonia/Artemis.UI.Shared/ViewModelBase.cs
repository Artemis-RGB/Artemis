using System;
using System.Reactive.Disposables;
using Artemis.UI.Shared.Events;
using FluentAvalonia.UI.Controls;
using ReactiveUI;
using ReactiveUI.Validation.Helpers;

namespace Artemis.UI.Shared
{
    /// <summary>
    ///     Represents the base class for Artemis view models
    /// </summary>
    public abstract class ContentDialogViewModelBase : ReactiveValidationObject, IActivatableViewModel, IDisposable
    {
        /// <summary>
        /// Gets the content dialog that hosts the view model
        /// </summary>
        public ContentDialog? ContentDialog { get; internal set; }

        #region Implementation of IActivatableViewModel

        /// <inheritdoc />
        public ViewModelActivator Activator { get; } = new();

        #endregion

        #region IDisposable

        /// <summary>
        ///     Releases the unmanaged resources used by the object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">
        ///     <see langword="true" /> to release both managed and unmanaged resources;
        ///     <see langword="false" /> to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }

    /// <summary>
    ///     Represents the base class for Artemis view models
    /// </summary>
    public abstract class ViewModelValidationBase : ReactiveValidationObject
    {
        private string? _displayName;

        /// <summary>
        ///     Gets or sets the display name of the view model
        /// </summary>
        public string? DisplayName
        {
            get => _displayName;
            set => this.RaiseAndSetIfChanged(ref _displayName, value);
        }
    }

    /// <summary>
    ///     Represents the base class for Artemis view models
    /// </summary>
    public abstract class ViewModelBase : ReactiveObject
    {
        private string? _displayName;

        /// <summary>
        ///     Gets or sets the display name of the view model
        /// </summary>
        public string? DisplayName
        {
            get => _displayName;
            set => this.RaiseAndSetIfChanged(ref _displayName, value);
        }
    }

    /// <summary>
    ///     Represents the base class for Artemis view models that are interested in the activated event
    /// </summary>
    public abstract class ActivatableViewModelBase : ViewModelBase, IActivatableViewModel
    {
        /// <inheritdoc />
        public ViewModelActivator Activator { get; } = new();
    }

    /// <summary>
    ///     Represents the base class for Artemis view models used to drive dialogs
    /// </summary>
    public abstract class DialogViewModelBase<TResult> : ActivatableViewModelBase
    {
        /// <summary>
        ///     Closes the dialog with the given <paramref name="result" />
        /// </summary>
        /// <param name="result">The result of the dialog</param>
        public void Close(TResult result)
        {
            CloseRequested?.Invoke(this, new DialogClosedEventArgs<TResult>(result));
        }

        /// <summary>
        ///     Closes the dialog without a result
        /// </summary>
        public void Cancel()
        {
            CancelRequested?.Invoke(this, EventArgs.Empty);
        }

        internal event EventHandler<DialogClosedEventArgs<TResult>>? CloseRequested;
        internal event EventHandler? CancelRequested;
    }
}