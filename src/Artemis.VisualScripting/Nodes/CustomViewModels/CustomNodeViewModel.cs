using System.Windows;
using Artemis.Core;
using Stylet;

namespace Artemis.VisualScripting.Nodes.CustomViewModels
{
    public abstract class CustomNodeViewModel : PropertyChangedBase, IViewAware, ICustomNodeViewModel
    {
        protected CustomNodeViewModel(INode node)
        {
            Node = node;
        }

        public INode Node { get; }

        #region Implementation of IViewAware

        /// <inheritdoc />
        public void AttachView(UIElement view)
        {
            View = view;
            OnDisplay();
        }

        protected virtual void OnDisplay()
        {
        }

        /// <inheritdoc />
        public UIElement View { get; private set; }

        #endregion
    }
}