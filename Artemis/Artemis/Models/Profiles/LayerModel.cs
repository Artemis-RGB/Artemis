using System.ComponentModel;
using System.Linq;
using System.Windows.Media;
using System.Xml.Serialization;
using Artemis.Models.Interfaces;
using Artemis.Models.Profiles.Properties;
using Artemis.Utilities;
using Artemis.Utilities.Layers;
using Artemis.Utilities.ParentChild;

namespace Artemis.Models.Profiles
{
    public class LayerModel : IChildItem<LayerModel>, IChildItem<ProfileModel>
    {
        public LayerModel()
        {
            Children = new ChildItemCollection<LayerModel, LayerModel>(this);
        }

        public string Name { get; set; }
        public LayerType LayerType { get; set; }
        public bool Enabled { get; set; }
        public int Order { get; set; }
        public LayerPropertiesModel Properties { get; set; }
        public ChildItemCollection<LayerModel, LayerModel> Children { get; }

        [XmlIgnore]
        public ImageSource LayerImage => Drawer.DrawThumbnail(this);

        [XmlIgnore]
        public LayerModel Parent { get; internal set; }

        [XmlIgnore]
        public ProfileModel Profile { get; internal set; }

        [XmlIgnore]
        public GifImage GifImage { get; set; }

        public bool ConditionsMet<T>(IGameDataModel dataModel)
        {
            return Enabled && Properties.Conditions.All(cm => cm.ConditionMet<T>(dataModel));
        }

        public void Draw<T>(IGameDataModel dataModel, DrawingContext c, bool preview, bool updateAnimations)
        {
            // Don't draw when the layer is disabled
            if (!Enabled)
                return;

            // Preview simply shows the properties as they are. When not previewing they are applied
            LayerPropertiesModel appliedProperties;
            if (!preview)
            {
                if (!ConditionsMet<T>(dataModel))
                    return; // Don't draw the layer when not previewing and the conditions arent met
                appliedProperties = Properties.GetAppliedProperties(dataModel);
            }
            else
                appliedProperties = GeneralHelpers.Clone(Properties);

            // Update animations on layer types that support them
            if ((LayerType == LayerType.Keyboard || LayerType == LayerType.KeyboardGif) && updateAnimations)
            {
                AnimationUpdater.UpdateAnimation((KeyboardPropertiesModel) Properties,
                    (KeyboardPropertiesModel) appliedProperties);
            }

            // Folders are drawn recursively
            if (LayerType == LayerType.Folder)
            {
                foreach (var layerModel in Children.OrderByDescending(l => l.Order))
                    layerModel.Draw<T>(dataModel, c, preview, updateAnimations);
            }
            // All other types are handles by the Drawer helper
            else if (LayerType == LayerType.Keyboard)
                Drawer.Draw(c, (KeyboardPropertiesModel) Properties, (KeyboardPropertiesModel) appliedProperties);
            else if (LayerType == LayerType.KeyboardGif)
                GifImage = Drawer.DrawGif(c, (KeyboardPropertiesModel) appliedProperties, GifImage);
            else if (LayerType == LayerType.Mouse)
                Drawer.UpdateMouse(appliedProperties);
            else if (LayerType == LayerType.Headset)
                Drawer.UpdateHeadset(appliedProperties);
        }

        public void SetupProperties()
        {
            if ((LayerType == LayerType.Keyboard || LayerType == LayerType.KeyboardGif) &&
                !(Properties is KeyboardPropertiesModel))
            {
                Properties = new KeyboardPropertiesModel
                {
                    Brush = new SolidColorBrush(ColorHelpers.GetRandomRainbowMediaColor()),
                    Animation = LayerAnimation.None,
                    Height = 1,
                    Width = 1,
                    X = 0,
                    Y = 0,
                    Opacity = 1
                };
            }
            else if (LayerType == LayerType.Mouse && !(Properties is MousePropertiesModel))
                Properties = new MousePropertiesModel();
            else if (!(Properties is GenericPropertiesModel))
                Properties = new GenericPropertiesModel();
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

        /// <summary>
        /// Returns whether the layer meets the requirements to be drawn
        /// </summary>
        /// <returns></returns>
        public bool MustDraw()
        {
            return Enabled && (LayerType == LayerType.Keyboard || LayerType == LayerType.KeyboardGif);
        }

        #region IChildItem<Parent> Members

        LayerModel IChildItem<LayerModel>.Parent
        {
            get { return Parent; }
            set { Parent = value; }
        }

        ProfileModel IChildItem<ProfileModel>.Parent
        {
            get { return Profile; }
            set { Profile = value; }
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