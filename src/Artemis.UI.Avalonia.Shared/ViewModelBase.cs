using System;
using System.Reactive;
using System.Reactive.Disposables;
using ReactiveUI;

namespace Artemis.UI.Avalonia.Shared
{
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
    public abstract class ActivatableViewModelBase : ViewModelBase, IActivatableViewModel, IDisposable
    {
        /// <inheritdoc />
        protected ActivatableViewModelBase()
        {
            this.WhenActivated(disposables => Disposable.Create(Dispose).DisposeWith(disposables));
        }

        /// <summary>
        ///     Releases the unmanaged resources used by the object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">
        ///     <see langword="true" /> to release both managed and unmanaged resources;
        ///     <see langword="false" /> to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }

        /// <inheritdoc />
        public ViewModelActivator Activator { get; } = new();

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    ///     Represents the base class for Artemis view models used to drive dialogs
    /// </summary>
    public abstract class DialogViewModelBase<TResult> : ActivatableViewModelBase
    {

        /// <inheritdoc />
        protected DialogViewModelBase()
        {
            Close = ReactiveCommand.Create<TResult, TResult>(t => t);
            Cancel = ReactiveCommand.Create(() => { });
        }


        /// <summary>
        ///     Closes the dialog with a given result
        /// </summary>
        public ReactiveCommand<TResult, TResult> Close { get; }

        /// <summary>
        /// Closes the dialog without a result
        /// </summary>
        public ReactiveCommand<Unit, Unit> Cancel { get; }
    }
}