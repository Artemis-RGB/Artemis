using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Media;
using System.Xml.Serialization;
using Artemis.Models.Interfaces;
using Artemis.Models.Profiles.Events;
using Artemis.Models.Profiles.Layers;
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
        public int Order { get; set; }
        public LayerType LayerType { get; set; }
        public bool Enabled { get; set; }
        public bool Expanded { get; set; }
        public bool Event { get; set; }
        public LayerPropertiesModel Properties { get; set; }
        public EventPropertiesModel EventProperties { get; set; }
        public ChildItemCollection<LayerModel, LayerModel> Children { get; }

        [XmlIgnore]
        public ImageSource LayerImage => Drawer.DrawThumbnail(this);

        [XmlIgnore]
        public LayerModel Parent { get; internal set; }

        [XmlIgnore]
        public ProfileModel Profile { get; internal set; }

        [XmlIgnore]
        public GifImage GifImage { get; set; }

        /// <summary>
        /// Checks whether this layers conditions are met. 
        /// If they are met and this layer is an event, this also triggers that event.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataModel"></param>
        /// <returns></returns>
        public bool ConditionsMet<T>(IDataModel dataModel)
        {
            // Conditions are not even checked if the layer isn't enabled
            if (!Enabled)
                return false;

            if (Event)
            {
                if (EventProperties.MustStop(this))
                {
                    EventProperties.MustTrigger = true;
                    return false;
                }
            }

            var conditionsMet = Properties.Conditions.All(cm => cm.ConditionMet<T>(dataModel));
            if (conditionsMet && Event && EventProperties.MustTrigger)
                EventProperties.TriggerEvent(this);

            return conditionsMet;
        }

        public void Draw(IDataModel dataModel, DrawingContext c, bool preview, bool updateAnimations)
        {
            if (LayerType != LayerType.Keyboard && LayerType != LayerType.KeyboardGif)
                return;

            // Preview simply shows the properties as they are. When not previewing they are applied
            var appliedProperties = !preview
                ? Properties.GetAppliedProperties(dataModel)
                : Properties.GetAppliedProperties(dataModel, true);

            // Update animations
            AnimationUpdater.UpdateAnimation((KeyboardPropertiesModel) Properties, updateAnimations);

            if (LayerType == LayerType.Keyboard)
                Drawer.Draw(c, (KeyboardPropertiesModel) Properties, appliedProperties);
            else if (LayerType == LayerType.KeyboardGif)
                GifImage = Drawer.DrawGif(c, (KeyboardPropertiesModel) Properties, appliedProperties, GifImage);
        }

        public Brush GenerateBrush<T>(LayerType type, IDataModel dataModel, bool preview, bool updateAnimations)
        {
            if (!Enabled)
                return null;
            if (LayerType != LayerType.Folder && LayerType != type)
                return null;

            // Preview simply shows the properties as they are. When not previewing they are applied
            AppliedProperties appliedProperties;
            if (!preview)
            {
                if (!ConditionsMet<T>(dataModel))
                    return null; // Return null when not previewing and the conditions arent met
                appliedProperties = Properties.GetAppliedProperties(dataModel);
            }
            else
                appliedProperties = Properties.GetAppliedProperties(dataModel, true);

            // TODO: Mouse/headset animations

            if (LayerType != LayerType.Folder)
                return appliedProperties.Brush;

            Brush res = null;
            foreach (var layerModel in Children.OrderByDescending(l => l.Order))
            {
                var brush = layerModel.GenerateBrush<T>(type, dataModel, preview, updateAnimations);
                if (brush != null)
                    res = brush;
            }
            return res;
        }

        public void SetupProperties()
        {
            // Set up the correct properties model
            var brush = new SolidColorBrush(ColorHelpers.GetRandomRainbowMediaColor());
            if (LayerType == LayerType.Mouse && !(Properties is MousePropertiesModel))
            {
                Properties = new MousePropertiesModel {Brush = brush};
                // TODO: Check for event and default properties for it
                return;
            }
            if (LayerType == LayerType.Headset && !(Properties is HeadsetPropertiesModel))
            {
                Properties = new HeadsetPropertiesModel {Brush = brush};
                // TODO: Check for event and default properties for it
                return;
            }

            // Here we know that it's not a mouse/headset layer so set up the keyboard model if need be
            if (!(Properties is KeyboardPropertiesModel))
            {
                Properties = new KeyboardPropertiesModel
                {
                    Brush = brush,
                    Animation = LayerAnimation.None,
                    Height = 1,
                    Width = 1,
                    X = 0,
                    Y = 0,
                    Opacity = 1
                };
            }

            // If the type is an event, set it up 
            if (Event && EventProperties == null)
            {
                EventProperties = new KeyboardEventPropertiesModel
                {
                    ExpirationType = ExpirationType.Time,
                    Length = new TimeSpan(0, 0, 1),
                    TriggerDelay = new TimeSpan(0)
                };
            }
        }

        public void FixOrder()
        {
            Children.Sort(l => l.Order);
            for (var i = 0; i < Children.Count; i++)
                Children[i].Order = i;
        }

        /// <summary>
        ///     Returns whether the layer meets the requirements to be drawn in the profile editor
        /// </summary>
        /// <returns></returns>
        public bool MustDraw()
        {
            // If any of the parents are disabled, this layer must not be drawn
            var parent = Parent;
            while (parent != null)
            {
                if (!parent.Enabled)
                    return false;
                parent = parent.Parent;
            }
            return Enabled && (LayerType == LayerType.Keyboard || LayerType == LayerType.KeyboardGif);
        }

        /// <summary>
        ///     Returns every descendant of this layer
        /// </summary>
        /// <returns></returns>
        public IEnumerable<LayerModel> GetLayers()
        {
            var layers = new List<LayerModel>();
            foreach (var layerModel in Children)
            {
                layers.Add(layerModel);
                layers.AddRange(layerModel.GetLayers());
            }

            return layers;
        }

        /// <summary>
        ///     Creates a new Keyboard layer with default settings
        /// </summary>
        /// <returns></returns>
        public static LayerModel CreateLayer()
        {
            return new LayerModel
            {
                Name = "New layer",
                Enabled = true,
                Order = -1,
                LayerType = LayerType.Keyboard,
                Properties = new KeyboardPropertiesModel
                {
                    Brush = new SolidColorBrush(ColorHelpers.GetRandomRainbowMediaColor()),
                    Animation = LayerAnimation.None,
                    Height = 1,
                    Width = 1,
                    X = 0,
                    Y = 0,
                    Opacity = 1
                }
            };
        }

        public void InsertBefore(LayerModel source)
        {
            source.Order = Order;
            Insert(source);
        }

        public void InsertAfter(LayerModel source)
        {
            source.Order = Order + 1;
            Insert(source);
        }

        private void Insert(LayerModel source)
        {
            if (Parent != null)
            {
                foreach (var child in Parent.Children.OrderBy(c => c.Order))
                {
                    if (child.Order >= source.Order)
                        child.Order++;
                }
                Parent.Children.Add(source);
            }
            else if (Profile != null)
            {
                foreach (var layer in Profile.Layers.OrderBy(l => l.Order))
                {
                    if (layer.Order >= source.Order)
                        layer.Order++;
                }
                Profile.Layers.Add(source);
            }
        }

        /// <summary>
        ///     Generates a flat list containing all layers that must be rendered on the keyboard,
        ///     the first mouse layer to be rendered and the first headset layer to be rendered
        /// </summary>
        /// <typeparam name="T">The game data model to base the conditions on</typeparam>
        /// <param name="dataModel">Instance of said game data model</param>
        /// <param name="includeMice">Whether or not to include mice in the list</param>
        /// <param name="includeHeadsets">Whether or not to include headsets in the list</param>
        /// <param name="ignoreConditions"></param>
        /// <returns>A flat list containing all layers that must be rendered</returns>
        public List<LayerModel> GetRenderLayers<T>(IDataModel dataModel, bool includeMice, bool includeHeadsets,
            bool ignoreConditions = false)
        {
            var layers = new List<LayerModel>();
            foreach (var layerModel in Children.OrderByDescending(c => c.Order))
            {
                if (!layerModel.Enabled ||
                    !includeMice && layerModel.LayerType == LayerType.Mouse ||
                    !includeHeadsets && layerModel.LayerType == LayerType.Headset)
                    continue;

                if (!ignoreConditions)
                {
                    if (!layerModel.ConditionsMet<T>(dataModel))
                        continue;
                }

                layers.Add(layerModel);
                layers.AddRange(layerModel.GetRenderLayers<T>(dataModel, includeMice, includeHeadsets, ignoreConditions));
            }

            return layers;
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