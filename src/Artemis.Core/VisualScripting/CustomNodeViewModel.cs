namespace Artemis.Core
{
    /// <summary>
    ///     Represents a custom view model for a <see cref="INode" />
    /// </summary>
    public interface ICustomNodeViewModel
    {
        /// <summary>
        ///     Gets the node the view models belongs to
        /// </summary>
        public INode Node { get; }

        /// <summary>
        ///     Called whenever the custom view model is activated
        /// </summary>
        void OnActivate();

        /// <summary>
        ///     Called whenever the custom view model is closed
        /// </summary>
        void OnDeactivate();
    }
}