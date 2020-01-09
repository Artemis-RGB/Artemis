using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.PropertyTree
{
    public abstract class PropertyTreeItemViewModel : PropertyChangedBase
    {
        /// <summary>
        ///     Updates the tree item's input if it is visible and has keyframes enabled
        /// </summary>
        /// <param name="forceUpdate">Force update regardless of visibility and keyframes</param>
        public abstract void Update(bool forceUpdate);
    }
}