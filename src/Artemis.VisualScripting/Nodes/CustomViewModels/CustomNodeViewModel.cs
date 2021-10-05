using System;
using System.ComponentModel;
using Artemis.Core;
using Stylet;

namespace Artemis.VisualScripting.Nodes.CustomViewModels
{
    public abstract class CustomNodeViewModel : PropertyChangedBase, ICustomNodeViewModel
    {
        protected CustomNodeViewModel(INode node)
        {
            Node = node;
        }

        public INode Node { get; }

        #region Implementation of ICustomNodeViewModel

        /// <inheritdoc />
        public virtual void OnActivate()
        {
            Node.PropertyChanged += NodeOnPropertyChanged;
        }

        /// <inheritdoc />
        public virtual void OnDeactivate()
        {
            Node.PropertyChanged -= NodeOnPropertyChanged;
        }

        #endregion

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

        private void NodeOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Storage")
                OnNodeModified();
        }

        #endregion
    }
}