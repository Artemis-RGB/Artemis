using System.Windows.Media;
using Artemis.Models.Interfaces;
using Artemis.Profiles.Layers.Models;
using Artemis.ViewModels.Profiles.Layers;

namespace Artemis.Profiles.Layers.Interfaces
{
    public interface ILayerType
    {
        /// <summary>
        ///     Layer type name
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     Gets whether this type must be drawn on the keyboard/the editor or not
        /// </summary>
        bool MustDraw { get; }

        /// <summary>
        ///     The the thumbnail for this layer type
        /// </summary>
        /// <param name="layer">The layer to draw the thumbnail for</param>
        ImageSource DrawThumbnail(LayerModel layer);

        /// <summary>
        ///     Draws the layer
        /// </summary>
        /// <param name="layer">The layer to draw</param>
        /// <param name="c">The drawing context to draw with</param>
        void Draw(LayerModel layer, DrawingContext c);

        /// <summary>
        ///     Updates the provided layer layerModel according to this type
        /// </summary>
        /// <param name="layerModel">The layerModel to apply to</param>
        /// <param name="dataModel">The datamodel to base the layer on</param>
        /// <param name="isPreview">Set to true if previewing this layer</param>
        void Update(LayerModel layerModel, IDataModel dataModel, bool isPreview = false);

        /// <summary>
        ///     Sets up the layer's properties to accommodate this layerType
        /// </summary>
        /// <param name="layerModel"></param>
        void SetupProperties(LayerModel layerModel);

        /// <summary>
        ///     Sets up a viewmodel to accomodate this layerType
        /// </summary>
        /// <param name="layerPropertiesViewModel">The current viewmodel</param>
        /// <param name="dataModel">The datamodel to use in the new viewmodel</param>
        /// <param name="proposedLayer">The layer to use in the new viewmodel</param>
        LayerPropertiesViewModel SetupViewModel(LayerPropertiesViewModel layerPropertiesViewModel, IDataModel dataModel,
            LayerModel proposedLayer);
    }
}