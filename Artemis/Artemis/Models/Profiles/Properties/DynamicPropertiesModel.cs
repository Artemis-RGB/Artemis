using System.ComponentModel;
using Artemis.Models.Interfaces;
using Artemis.Utilities;
using static System.Decimal;

namespace Artemis.Models.Profiles.Properties
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
        public double PercentageSource { get; set; }

        /// <summary>
        ///     Percentage source property, the property that defines 100%
        /// </summary>
        public string PercentageProperty { get; set; }

        /// <summary>
        ///     Type of property
        /// </summary>
        public LayerPropertyType LayerPropertyType { get; set; }

        


        internal void ApplyProperty(IGameDataModel dataModel, KeyboardPropertiesModel properties)
        {
            if (LayerPropertyType == LayerPropertyType.PercentageOf)
                ApplyPercentageOf(dataModel, properties, PercentageSource);
            if (LayerPropertyType == LayerPropertyType.PercentageOfProperty)
                ApplyPercentageOfProperty(dataModel, properties);
        }

        private void ApplyPercentageOf(IGameDataModel dataModel, KeyboardPropertiesModel properties,
            double percentageSource)
        {
            // Property to apply on
            var layerProp = properties.GetType().GetProperty(LayerProperty);
            // Property to base the percentage upon
            var gameProperty = dataModel.GetPropValue<int>(GameProperty);

            if (layerProp == null)
                return;

            var percentage = ToDouble(gameProperty)/percentageSource;
            var appliedValue = percentage*(double) layerProp.GetValue(layerProp, null);

            // Opacity requires some special treatment as it causes an exception if it's < 0.0 or > 1.0
            if (LayerProperty == "Opacity")
            {
                if (appliedValue < 0.0)
                    appliedValue = 0.0;
                if (appliedValue > 1.0)
                    appliedValue = 1.0;
            }

            layerProp.SetValue(layerProp, appliedValue);
        }

        private void ApplyPercentageOfProperty(IGameDataModel dataModel, KeyboardPropertiesModel properties)
        {
            var value = dataModel.GetPropValue<double>(PercentageProperty);
            ApplyPercentageOf(dataModel, properties, value);
        }
    }

    public enum LayerPropertyType
    {
        [Description("None")] None,
        [Description("% of")] PercentageOf,
        [Description("% of property")] PercentageOfProperty
    }
}