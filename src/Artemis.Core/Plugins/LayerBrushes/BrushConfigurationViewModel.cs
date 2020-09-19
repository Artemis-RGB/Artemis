using Stylet;

namespace Artemis.Core.LayerBrushes
{
    /// <summary>
    ///     Represents a view model for a brush configuration window
    /// </summary>
    public abstract class BrushConfigurationViewModel : Screen
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="BrushConfigurationViewModel" /> class
        /// </summary>
        /// <param name="layerBrush"></param>
        protected BrushConfigurationViewModel(BaseLayerBrush layerBrush)
        {
            LayerBrush = layerBrush;
        }

        /// <summary>
        ///     Gets the layer brush this view model is associated with
        /// </summary>
        public BaseLayerBrush LayerBrush { get; }
    }
}