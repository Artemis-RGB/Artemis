using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.PropertyTree
{
    public abstract class PropertyTreeItemViewModel : PropertyChangedBase
    {
        protected PropertyTreeItemViewModel(LayerPropertyViewModel layerPropertyViewModel)
        {
            LayerPropertyViewModel = layerPropertyViewModel;
        }

        public LayerPropertyViewModel LayerPropertyViewModel { get; }

        /// <summary>
        ///     Updates the tree item's input if it is visible and has keyframes enabled
        /// </summary>
        /// <param name="forceUpdate">Force update regardless of visibility and keyframes</param>
        public abstract void Update(bool forceUpdate);

        /// <summary>
        ///     Removes the layer property recursively
        /// </summary>
        /// <param name="layerPropertyViewModel"></param>
        public abstract void RemoveLayerProperty(LayerPropertyViewModel layerPropertyViewModel);

        /// <summary>
        ///     Adds the layer property recursively
        /// </summary>
        /// <param name="layerPropertyViewModel"></param>
        public abstract void AddLayerProperty(LayerPropertyViewModel layerPropertyViewModel);
    }
}