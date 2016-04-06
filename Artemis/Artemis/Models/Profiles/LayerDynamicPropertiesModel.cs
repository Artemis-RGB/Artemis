using System.Collections.Generic;
using System.Drawing;
using System.Linq.Dynamic;
using System.Reflection;
using Artemis.Models.Interfaces;

namespace Artemis.Models.Profiles
{
    public class LayerDynamicPropertiesModel
    {
        public string LayerProperty { get; set; }
        public string GameProperty { get; set; }
        public string RequiredOperator { get; set; }
        public string RequiredValue { get; set; }
        public LayerPopertyType LayerPopertyType { get; set; }

        /// <summary>
        ///     Only used when LayerPropertyType is PercentageOf or PercentageOfProperty
        /// </summary>
        public string PercentageSource { get; set; }

        internal void ApplyProperty<T>(IGameDataModel dataModel, LayerPropertiesModel userProps,
            LayerPropertiesModel props)
        {
            var dataList = new List<T> {(T) dataModel};

            // Attempt to set the property
            var layerProp = props.GetType().GetProperty(LayerProperty);
            var layerUserProp = userProps.GetType().GetProperty(LayerProperty);

            if (LayerPopertyType == LayerPopertyType.PercentageOf)
                SetPercentageOf(props, userProps, dataModel, int.Parse(PercentageSource));
            if (LayerPopertyType == LayerPopertyType.PercentageOfProperty)
                SetPercentageOfProperty(props, userProps, dataModel);
        }

        private void SetPercentageOf(LayerPropertiesModel props, LayerPropertiesModel userProps,
            IGameDataModel dataModel, int percentageSource)
        {
            // Property that will be set
            var layerProp = props.GetType().GetProperty(LayerProperty);
            // Property to use as a 100%
            var userProp = userProps.GetType().GetProperty(LayerProperty);
            // Value to use as a source
            var source = dataModel.GetType().GetProperty(GameProperty)?.GetValue(dataModel, null);
            if (layerProp == null || userProp == null || source == null)
                return;

            var percentage = double.Parse(source.ToString())/percentageSource;
            layerProp.SetValue(props, (int) (percentage*(int) userProp.GetValue(userProps, null)));
        }

        private void SetPercentageOfProperty(LayerPropertiesModel props, LayerPropertiesModel userProps,
            IGameDataModel dataModel)
        {
            var value = dataModel.GetType().GetProperty(PercentageSource)?.GetValue(dataModel, null);
            if (value != null)
                SetPercentageOf(props, userProps, dataModel, (int) value);
        }
    }

    public enum LayerPopertyType
    {
        PercentageOf,
        PercentageOfProperty,
        Color
    }
}