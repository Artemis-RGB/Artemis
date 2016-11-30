using System;
using System.ComponentModel;
using Artemis.Models.Interfaces;
using Artemis.Utilities;

namespace Artemis.Profiles.Layers.Models
{
    public class DynamicPropertiesModel
    {
        /// <summary>
        ///     Property this dynamic property applies on
        /// </summary>
        public string LayerProperty { get; set; }

        /// <summary>
        ///     Property to base the percentage upon
        /// </summary>
        public string GameProperty { get; set; }

        /// <summary>
        ///     Percentage source, the number that defines 100%
        /// </summary>
        public float PercentageSource { get; set; }

        /// <summary>
        ///     Percentage source property, the property that defines 100%
        /// </summary>
        public string PercentageProperty { get; set; }

        /// <summary>
        ///     Type of property
        /// </summary>
        public LayerPropertyType LayerPropertyType { get; set; }

        /// <summary>
        ///     Extra options on top of the selected properties
        /// </summary>
        public LayerPropertyOptions LayerPropertyOptions { get; set; }

        internal void ApplyProperty(IDataModel dataModel, LayerModel layerModel)
        {
            if (LayerPropertyType == LayerPropertyType.PercentageOf)
                ApplyPercentageOf(dataModel, layerModel, PercentageSource);
            if (LayerPropertyType == LayerPropertyType.PercentageOfProperty)
                ApplyPercentageOfProperty(dataModel, layerModel);
        }

        private void ApplyPercentageOf(IDataModel dataModel, LayerModel layerModel, float src)
        {
            if (GameProperty == null)
                return;

            var gameProperty = dataModel.GetPropValue<float>(GameProperty);
            var percentage = gameProperty/src;

            if (LayerProperty == "Width")
                ApplyWidth(layerModel, percentage);
            else if (LayerProperty == "Height")
                ApplyHeight(layerModel, percentage);
            else if (LayerProperty == "Opacity")
                ApplyOpacity(layerModel, percentage);
        }

        private void ApplyWidth(LayerModel layerModel, float percentage)
        {
            var newWidth = Math.Round(percentage*(float) layerModel.Width, 2);
            var difference = layerModel.Width - newWidth;
            if (newWidth < 0)
                newWidth = 0;

            layerModel.Width = newWidth;

            // Apply the right to left option
            if (LayerPropertyOptions == LayerPropertyOptions.RightToLeft)
                layerModel.X = layerModel.X + difference;
        }

        private void ApplyHeight(LayerModel layerModel, float percentage)
        {
            var newHeight = Math.Round(percentage*(float) layerModel.Height, 2);
            var difference = layerModel.Height - newHeight;
            if (newHeight < 0)
                newHeight = 0;

            layerModel.Height = newHeight;

            if (LayerPropertyOptions == LayerPropertyOptions.Downwards)
                layerModel.Y = layerModel.Y + difference;
        }

        private void ApplyOpacity(LayerModel layerModel, float percentage)
        {
            layerModel.Opacity = percentage*(float) layerModel.Opacity;
            if (layerModel.Opacity < 0.0)
                layerModel.Opacity = 0.0;
            if (layerModel.Opacity > 1.0)
                layerModel.Opacity = 1.0;

            // Apply the inverse/decrease option
            if (LayerPropertyOptions == LayerPropertyOptions.Decrease)
                layerModel.Opacity = 1.0 - layerModel.Opacity;

            var brush = layerModel.Brush.Clone();
            brush.Opacity = layerModel.Opacity;
            layerModel.Brush = brush;
        }

        private void ApplyPercentageOfProperty(IDataModel dataModel, LayerModel layerModel)
        {
            var value = dataModel.GetPropValue<float>(PercentageProperty);
            ApplyPercentageOf(dataModel, layerModel, value);
        }
    }

    public enum LayerPropertyType
    {
        [Description("% of")] PercentageOf,
        [Description("% of property")] PercentageOfProperty
    }

    public enum LayerPropertyOptions
    {
        [Description("Left to right")] LeftToRight,
        [Description("Right to left")] RightToLeft,
        [Description("Downwards")] Downwards,
        [Description("Upwards")] Upwards,
        [Description("Increase")] Increase,
        [Description("Decrease")] Decrease
    }
}