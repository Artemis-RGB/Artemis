using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Media;
using System.Xml.Serialization;
using Artemis.Models.Interfaces;
using Artemis.Utilities;

namespace Artemis.Models.Profiles
{
    public class LayerModel
    {
        [XmlIgnore] private readonly LayerDrawer _drawer;
        private bool _mustDraw;

        public LayerModel()
        {
            UserProps = new LayerPropertiesModel();
            CalcProps = new LayerPropertiesModel();

            Children = new List<LayerModel>();
            LayerConditions = new List<LayerConditionModel>();
            LayerProperties = new List<LayerDynamicPropertiesModel>();

            _mustDraw = true;
            _drawer = new LayerDrawer(this, 4);
        }

        public string Name { get; set; }
        public LayerType LayerType { get; set; }
        public bool Enabled { get; set; }
        public LayerPropertiesModel UserProps { get; set; }
        
        public List<LayerModel> Children { get; set; }
        public List<LayerConditionModel> LayerConditions { get; set; }
        public List<LayerDynamicPropertiesModel> LayerProperties { get; set; }

        [XmlIgnore]
        public LayerPropertiesModel CalcProps { get; set; }

        [XmlIgnore]
        public ImageSource LayerImage => _drawer.GetThumbnail();

        public bool ConditionsMet<T>(IGameDataModel dataModel)
        {
            return Enabled && LayerConditions.All(cm => cm.ConditionMet<T>(dataModel));
        }

        public void DrawPreview(DrawingContext c)
        {
            GeneralHelpers.CopyProperties(CalcProps, UserProps);
            if (LayerType == LayerType.KeyboardRectangle || LayerType == LayerType.KeyboardEllipse)
                _drawer.Draw(c, _mustDraw);
            else if (LayerType == LayerType.KeyboardGif)
                _drawer.DrawGif(c);
            _mustDraw = false;
        }

        public void Draw<T>(IGameDataModel dataModel, DrawingContext c)
        {
            if (!ConditionsMet<T>(dataModel))
                return;

            if (LayerType == LayerType.Folder)
                foreach (var layerModel in Children)
                    layerModel.Draw<T>(dataModel, c);
            else if (LayerType == LayerType.KeyboardRectangle || LayerType == LayerType.KeyboardEllipse)
                _drawer.Draw(c);
            else if (LayerType == LayerType.KeyboardGif)
                _drawer.DrawGif(c);
            else if (LayerType == LayerType.Mouse)
                _drawer.UpdateMouse();
            else if (LayerType == LayerType.Headset)
                _drawer.UpdateHeadset();
        }

        public void Update<T>(IGameDataModel dataModel)
        {
            if (LayerType == LayerType.Folder)
            {
                foreach (var layerModel in Children)
                    layerModel.Update<T>(dataModel);
                return;
            }

            GeneralHelpers.CopyProperties(CalcProps, UserProps);
            foreach (var dynamicProperty in LayerProperties)
                dynamicProperty.ApplyProperty<T>(dataModel, UserProps, CalcProps);
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