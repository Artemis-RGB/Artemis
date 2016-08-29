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

        internal void ApplyProperty(IDataModel dataModel, LayerPropertiesModel properties)
        {
            if (LayerPropertyType == LayerPropertyType.PercentageOf)
                ApplyPercentageOf(dataModel, properties, PercentageSource);
            if (LayerPropertyType == LayerPropertyType.PercentageOfProperty)
                ApplyPercentageOfProperty(dataModel, properties);
        }

        private void ApplyPercentageOf(IDataModel dataModel, LayerPropertiesModel properties, float src)
        {
            if (GameProperty == null)
                return;

            var gameProperty = dataModel.GetPropValue<float>(GameProperty);
            var percentage = gameProperty/src;

            if (LayerProperty == "Width")
                ApplyWidth(properties, percentage);
            else if (LayerProperty == "Height")
                ApplyHeight(properties, percentage);
            else if (LayerProperty == "Opacity")
                ApplyOpacity(properties, percentage);
        }

        private void ApplyWidth(LayerPropertiesModel properties, float percentage)
        {
            var newWidth = Math.Round(percentage*(float) properties.Width, 2);
            var difference = properties.Width - newWidth;
            if (newWidth < 0)
                newWidth = 0;

            properties.Width = newWidth;

            // Apply the right to left option
            if (LayerPropertyOptions == LayerPropertyOptions.RightToLeft)
                properties.X = properties.X + difference;
        }

        private void ApplyHeight(LayerPropertiesModel properties, float percentage)
        {
            var newHeight = Math.Round(percentage*(float) properties.Height, 2);
            var difference = properties.Height - newHeight;
            if (newHeight < 0)
                newHeight = 0;

            properties.Height = newHeight;

            if (LayerPropertyOptions == LayerPropertyOptions.Downwards)
                properties.Y = properties.Y + difference;
        }

        private void ApplyOpacity(LayerPropertiesModel properties, float percentage)
        {
            properties.Opacity = percentage*(float) properties.Opacity;
            if (properties.Opacity < 0.0)
                properties.Opacity = 0.0;
            if (properties.Opacity > 1.0)
                properties.Opacity = 1.0;

            // Apply the inverse/decrease option
            if (LayerPropertyOptions == LayerPropertyOptions.Decrease)
                properties.Opacity = 1.0 - properties.Opacity;

            var brush = properties.Brush.Clone();
            brush.Opacity = properties.Opacity;
            properties.Brush = brush;
        }

        private void ApplyPercentageOfProperty(IDataModel dataModel, LayerPropertiesModel properties)
        {
            var value = dataModel.GetPropValue<float>(PercentageProperty);
            ApplyPercentageOf(dataModel, properties, value);
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