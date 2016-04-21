using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Media;
using System.Xml.Serialization;
using Artemis.Models.Interfaces;
using Artemis.Utilities;
using Artemis.Utilities.ParentChild;

namespace Artemis.Models.Profiles
{
    public class LayerModel : IChildItem<LayerModel>, IChildItem<ProfileModel>
    {
        [XmlIgnore] private readonly LayerDrawer _drawer;
        [XmlIgnore] private bool _mustDraw;

        public LayerModel()
        {
            UserProps = new LayerPropertiesModel();
            CalcProps = new LayerPropertiesModel();

            Children = new ChildItemCollection<LayerModel, LayerModel>(this);
            LayerConditions = new List<LayerConditionModel>();
            LayerProperties = new List<LayerDynamicPropertiesModel>();

            _mustDraw = true;
            _drawer = new LayerDrawer(this, 4);
        }

        public string Name { get; set; }
        public LayerType LayerType { get; set; }
        public bool Enabled { get; set; }
        public int Order { get; set; }
        public LayerPropertiesModel UserProps { get; set; }

        public ChildItemCollection<LayerModel, LayerModel> Children { get; }
        public List<LayerConditionModel> LayerConditions { get; set; }
        public List<LayerDynamicPropertiesModel> LayerProperties { get; set; }

        [XmlIgnore]
        public LayerPropertiesModel CalcProps { get; set; }

        [XmlIgnore]
        public ImageSource LayerImage => _drawer.GetThumbnail();

        [XmlIgnore]
        public LayerModel ParentLayer { get; internal set; }

        [XmlIgnore]
        public ProfileModel ParentProfile { get; internal set; }

        public bool ConditionsMet<T>(IGameDataModel dataModel)
        {
            return Enabled && LayerConditions.All(cm => cm.ConditionMet<T>(dataModel));
        }

        public void DrawPreview(DrawingContext c)
        {
            GeneralHelpers.CopyProperties(CalcProps, UserProps);
            if (LayerType == LayerType.Keyboard || LayerType == LayerType.Keyboard)
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
                foreach (var layerModel in Children.OrderByDescending(l => l.Order))
                    layerModel.Draw<T>(dataModel, c);
            else if (LayerType == LayerType.Keyboard || LayerType == LayerType.Keyboard)
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

        public void Reorder(LayerModel selectedLayer, bool moveUp)
        {
            // Fix the sorting just in case
            FixOrder();

            int newOrder;
            if (moveUp)
                newOrder = selectedLayer.Order - 1;
            else
                newOrder = selectedLayer.Order + 1;

            var target = Children.FirstOrDefault(l => l.Order == newOrder);
            if (target == null)
                return;

            target.Order = selectedLayer.Order;
            selectedLayer.Order = newOrder;
        }

        private void FixOrder()
        {
            Children.Sort(l => l.Order);
            for (var i = 0; i < Children.Count; i++)
                Children[i].Order = i;
        }

        #region IChildItem<Parent> Members

        LayerModel IChildItem<LayerModel>.Parent
        {
            get { return ParentLayer; }
            set { ParentLayer = value; }
        }

        ProfileModel IChildItem<ProfileModel>.Parent
        {
            get { return ParentProfile; }
            set { ParentProfile = value; }
        }

        #endregion
    }

    public enum LayerType
    {
        [Description("Folder")] Folder,
        [Description("Keyboard")] Keyboard,
        [Description("Keyboard - GIF")] KeyboardGif,
        [Description("Mouse")] Mouse,
        [Description("Headset")] Headset
    }
}