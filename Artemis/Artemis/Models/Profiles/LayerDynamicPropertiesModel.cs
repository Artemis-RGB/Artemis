using System.ComponentModel;
using Artemis.Models.Interfaces;
using Artemis.Utilities;
using static System.Decimal;

namespace Artemis.Models.Profiles
{
    public class LayerDynamicPropertiesModel
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
        public string PercentageSource { get; set; }

        /// <summary>
        ///     Type of property
        /// </summary>
        public LayerPropertyType LayerPropertyType { get; set; }

        internal void ApplyProperty<T>(IGameDataModel data, LayerPropertiesModel userProps, LayerPropertiesModel props)
        {
            if (LayerPropertyType == LayerPropertyType.PercentageOf)
                Apply(props, userProps, data, int.Parse(PercentageSource));
            if (LayerPropertyType == LayerPropertyType.PercentageOfProperty)
                ApplyProp(props, userProps, data);
        }

        private void Apply(LayerPropertiesModel props, LayerPropertiesModel userProps, IGameDataModel data,
            int percentageSource)
        {
            // Property to apply on
            var layerProp = props.GetType().GetProperty(LayerProperty);
            // User's settings
            var userProp = userProps.GetType().GetProperty(LayerProperty);
            // Property to base the percentage upon
            var gameProperty = data.GetPropValue<int>(GameProperty);
            if (layerProp == null || userProp == null)
                return;

            var percentage = ToDouble(gameProperty)/percentageSource;
            layerProp.SetValue(props, (int) (percentage*(int) userProp.GetValue(userProps, null)));
        }

        private void ApplyProp(LayerPropertiesModel props, LayerPropertiesModel userProps, IGameDataModel data)
        {
            var value = data.GetPropValue<int>(PercentageSource);
            Apply(props, userProps, data, value);
        }
    }

    public enum LayerPropertyType
    {
        [Description("None")] None,
        [Description("% of")] PercentageOf,
        [Description("% of property")] PercentageOfProperty
    }
}