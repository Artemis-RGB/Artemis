using Artemis.Core.LayerBrushes;
using Stylet;

namespace Artemis.UI.Shared.LayerBrushes
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
        ///     Creates a new instance of the <see cref="BrushConfigurationViewModel" /> class with a validator
        /// </summary>
        /// <param name="layerBrush"></param>
        /// <param name="validator"></param>
        protected BrushConfigurationViewModel(BaseLayerBrush layerBrush, IModelValidator validator) : base(validator)
        {
            LayerBrush = layerBrush;
        }

        /// <summary>
        ///     Gets the layer brush this view model is associated with
        /// </summary>
        public BaseLayerBrush LayerBrush { get; }
    }
}