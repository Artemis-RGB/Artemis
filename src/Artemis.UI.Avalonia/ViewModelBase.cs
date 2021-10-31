using System;
using System.Reactive.Disposables;
using ReactiveUI;

namespace Artemis.UI.Avalonia
{
    public abstract class ViewModelBase : ReactiveObject
    {
        private string? _displayName;

        public string? DisplayName
        {
            get => _displayName;
            set => this.RaiseAndSetIfChanged(ref _displayName, value);
        }
    }

    public abstract class ActivatableViewModelBase : ViewModelBase, IActivatableViewModel, IDisposable
    {
        /// <inheritdoc />
        protected ActivatableViewModelBase()
        {
            this.WhenActivated(disposables =>
            {
                Disposable.Create(Dispose).DisposeWith(disposables);
            });
        }

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        public ViewModelActivator Activator { get; } = new();
    }
}