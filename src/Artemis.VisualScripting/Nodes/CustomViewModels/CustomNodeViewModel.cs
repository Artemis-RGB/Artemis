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
        }

        /// <inheritdoc />
        public virtual void OnDeactivate()
        {
        }

        #endregion
    }
}