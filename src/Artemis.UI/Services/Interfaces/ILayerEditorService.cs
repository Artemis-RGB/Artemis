using System.Windows;
using System.Windows.Media;
using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Profile.LayerShapes;
using SkiaSharp;

namespace Artemis.UI.Services.Interfaces
{
    public interface ILayerEditorService : IArtemisUIService
    {
        /// <summary>
        ///     Returns the layer's bounds, corrected for the current render scale.
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        Rect GetLayerBounds(Layer layer);

        /// <summary>
        ///     Returns the layer's anchor, corrected for the current render scale.
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        Point GetLayerAnchorPosition(Layer layer);

        /// <summary>
        ///     Returns the layer shape's bounds, corrected for the current render scale.
        /// </summary>
        /// <param name="layerShape"></param>
        /// <returns></returns>
        Rect GetLayerShapeBounds(LayerShape layerShape);

        /// <summary>
        ///     Creates a WPF transform group that contains all the transformations required to render the provided layer.
        ///     Note: Run on UI thread.
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        TransformGroup GetLayerTransformGroup(Layer layer);

        /// <summary>
        ///     Returns an absolute and scaled rectangular path for the given layer that is corrected for the current render scale.
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="includeTranslation"></param>
        /// <param name="includeScale"></param>
        /// <param name="includeRotation"></param>
        /// <returns></returns>
        SKPath GetLayerPath(Layer layer, bool includeTranslation, bool includeScale, bool includeRotation);

        /// <summary>
        ///     Sets the base properties of the given shape to match the provided unscaled rectangle. The rectangle is corrected
        ///     for the current render scale, anchor property and size property.
        /// </summary>
        /// <param name="layerShape"></param>
        /// <param name="rect"></param>
        void SetShapeBaseFromRectangle(LayerShape layerShape, Rect rect);

        /// <summary>
        ///     Returns a new point scaled to the layer.
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="point"></param>
        /// <param name="absolute"></param>
        /// <returns></returns>
        SKPoint GetScaledPoint(Layer layer, SKPoint point, bool absolute);
    }
}