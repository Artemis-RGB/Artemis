using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using Artemis.Core;
using ReactiveUI;

namespace Artemis.UI.Shared.VisualScripting
{
    public abstract class CustomNodeViewModel : ActivatableViewModelBase, ICustomNodeViewModel
    {
        protected CustomNodeViewModel(INode node)
        {
            Node = node;

            this.WhenActivated(d =>
            {
                Node.PropertyChanged += NodeOnPropertyChanged;
                Disposable.Create(() => Node.PropertyChanged -= NodeOnPropertyChanged).DisposeWith(d);
            });
        }

        public INode Node { get; }

        #region Events

        /// <inheritdoc />
        public event EventHandler NodeModified;

        /// <summary>
        /// Invokes the <see cref="NodeModified"/> event
        /// </summary>
        protected virtual void OnNodeModified()
        {
            NodeModified?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Event handlers

        private void NodeOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Storage")
                OnNodeModified();
        }

        #endregion
    }
}