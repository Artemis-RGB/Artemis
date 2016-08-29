using System.Collections.Generic;
using System.Windows.Media;
using Artemis.Models.Interfaces;
using Artemis.Profiles.Layers.Abstract;
using Artemis.Profiles.Layers.Models;
using Artemis.ViewModels.Profiles;
using Newtonsoft.Json;

namespace Artemis.Profiles.Layers.Interfaces
{
    public interface ILayerType
    {
        /// <summary>
        ///     Layer type name
        /// </summary>
        [JsonIgnore]
        string Name { get; }

        /// <summary>
        ///     Gets whether this type must be drawn in the editor or not. Setting this to true
        ///     enables moving and resizing the layer
        /// </summary>
        [JsonIgnore]
        bool ShowInEdtor { get; }

        /// <summary>
        ///     Gets for what kind of device this layer should be drawn.
        /// </summary>
        [JsonIgnore]
        DrawType DrawType { get; }

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
        /// <param name="layerEditorViewModel">The layer editor VM this type resides in</param>
        /// <param name="layerPropertiesViewModel">The current viewmodel</param>
        LayerPropertiesViewModel SetupViewModel(LayerEditorViewModel layerEditorViewModel, LayerPropertiesViewModel layerPropertiesViewModel);
    }

    public enum DrawType
    {
        None,
        Keyboard,
        Mouse,
        Headset,
        Generic
    }
}