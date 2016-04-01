using System.Collections.Generic;
using System.ComponentModel;
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

            _drawer = new LayerDrawer(this, 4);
        }

        public string Name { get; set; }
        public LayerType LayerType { get; set; }
        public LayerPropertiesModel LayerUserProperties { get; set; }

        public List<LayerModel> Children { get; set; }
        public List<LayerConditionModel> LayerConditions { get; set; }
        public List<LayerDynamicPropertiesModel> LayerProperties { get; set; }

        [JsonIgnore]
        public LayerPropertiesModel LayerCalculatedProperties { get; set; }

        [JsonIgnore]
        public ImageSource LayerImage => _drawer.GetThumbnail();

        public bool ConditionsMet<T>(IGameDataModel dataModel)
        {
            return LayerConditions.All(cm => cm.ConditionMet<T>(dataModel));
        }

        public void DrawPreview(Graphics g)
        {
            if (LayerType == LayerType.KeyboardRectangle || LayerType == LayerType.KeyboardEllipse)
                _drawer.Draw(g);
            else if (LayerType == LayerType.KeyboardGif)
                _drawer.DrawGif(g);
        }

        public void Draw<T>(IGameDataModel dataModel, Graphics g)
        {
            if (!ConditionsMet<T>(dataModel))
                return;

            Update<T>(dataModel);

            if (LayerType == LayerType.Folder)
                DrawChildren<T>(dataModel, g);
            else if (LayerType == LayerType.KeyboardRectangle || LayerType == LayerType.KeyboardEllipse)
                _drawer.Draw(g);
            else if (LayerType == LayerType.KeyboardGif)
                _drawer.DrawGif(g);
            else if (LayerType == LayerType.Mouse)
                _drawer.UpdateMouse();
            else if (LayerType == LayerType.Headset)
                _drawer.UpdateHeadset();
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
        [Description("Folder")] Folder,
        [Description("Keyboard - Rectangle")] KeyboardRectangle,
        [Description("Keyboard - Ellipse")] KeyboardEllipse,
        [Description("Keyboard - GIF")] KeyboardGif,
        [Description("Mouse")] Mouse,
        [Description("Headset")] Headset
    }
}