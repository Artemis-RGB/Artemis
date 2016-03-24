using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Media;
using Artemis.Models.Interfaces;
using Artemis.Utilities;
using Newtonsoft.Json;

namespace Artemis.Models.Profiles
{
    public class LayerModel
    {
        [JsonIgnore] private readonly LayerDrawer _drawer;

        public LayerModel(string name, LayerType layerType)
        {
            Name = name;
            LayerType = layerType;
            LayerUserProperties = new LayerPropertiesModel();
            LayerCalculatedProperties = new LayerPropertiesModel();

            Children = new List<LayerModel>();
            LayerConditions = new List<LayerConditionModel>();
            LayerProperties = new List<LayerDynamicPropertiesModel>();

            _drawer = new LayerDrawer(this);
        }

        public string Name { get; set; }
        public LayerType LayerType { get; set; }
        public LayerPropertiesModel LayerUserProperties { get; set; }

        public List<LayerModel> Children { get; set; }
        public List<LayerConditionModel> LayerConditions { get; set; }
        public List<LayerDynamicPropertiesModel> LayerProperties { get; set; }

        [JsonIgnore]
        public LayerPropertiesModel LayerCalculatedProperties { get; }

        [JsonIgnore]
        public ImageSource LayerImage => _drawer.GetPreviewImage();

        public bool ConditionsMet<T>(IGameDataModel dataModel)
        {
            return LayerConditions.All(cm => cm.ConditionMet<T>(dataModel));
        }

        public void Draw<T>(IGameDataModel dataModel, Graphics g)
        {
            if (!ConditionsMet<T>(dataModel))
                return;

            Update<T>(dataModel);
            switch (LayerType)
            {
                case LayerType.Folder:
                    DrawChildren<T>(dataModel, g);
                    break;
                case LayerType.Rectangle:
                    _drawer.DrawRectangle(g);
                    break;
                case LayerType.Ellipse:
                    _drawer.DrawEllipse(g);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Update<T>(IGameDataModel dataModel)
        {
            GeneralHelpers.CopyProperties(LayerCalculatedProperties, LayerUserProperties);
            foreach (var dynamicProperty in LayerProperties)
                dynamicProperty.ApplyProperty<T>(dataModel, LayerUserProperties, LayerCalculatedProperties);
        }

        private void DrawChildren<T>(IGameDataModel dataModel, Graphics g)
        {
            foreach (var layerModel in Children)
                layerModel.Draw<T>(dataModel, g);
        }
    }

    public enum LayerType
    {
        Folder,
        Rectangle,
        Ellipse
    }
}