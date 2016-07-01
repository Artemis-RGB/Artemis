using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Artemis.Models.Interfaces;
using Artemis.Profiles.Layers.Interfaces;
using Artemis.Profiles.Layers.Types.Headset;
using Artemis.Profiles.Layers.Types.Keyboard;
using Artemis.Profiles.Layers.Types.Mouse;
using Artemis.Utilities;
using Artemis.Utilities.ParentChild;
using Newtonsoft.Json;

namespace Artemis.Profiles.Layers.Models
{
    public class LayerModel : IChildItem<LayerModel>, IChildItem<ProfileModel>
    {
        public LayerModel()
        {
            Children = new ChildItemCollection<LayerModel, LayerModel>(this);

            var model = Properties as KeyboardPropertiesModel;
            if (model != null)
                GifImage = new GifImage(model.GifFile);
        }

        public ILayerType LayerType { get; set; }
        public ILayerCondition LayerCondition { get; set; }
        public ILayerAnimation LayerAnimation { get; set; }

        public string Name { get; set; }
        public int Order { get; set; }

        public bool Enabled { get; set; }
        public bool Expanded { get; set; }
        public bool IsEvent { get; set; }
        public LayerPropertiesModel Properties { get; set; }
        public EventPropertiesModel EventProperties { get; set; }
        public ChildItemCollection<LayerModel, LayerModel> Children { get; }

        [JsonIgnore]
        public LayerPropertiesModel AppliedProperties { get; set; }

        [JsonIgnore]
        public ImageSource LayerImage => LayerType.DrawThumbnail(this);

        [JsonIgnore]
        public LayerModel Parent { get; internal set; }

        [JsonIgnore]
        public ProfileModel Profile { get; internal set; }

        [JsonIgnore]
        public GifImage GifImage { get; set; }

        /// <summary>
        ///     Checks whether this layers conditions are met.
        ///     If they are met and this layer is an event, this also triggers that event.
        /// </summary>
        /// <param name="dataModel"></param>
        /// <returns></returns>
        public bool ConditionsMet(IDataModel dataModel)
        {
            // Conditions are not even checked if the layer isn't enabled
            return Enabled & LayerCondition.ConditionsMet(this, dataModel);
        }

        public void Update(IDataModel dataModel, bool preview, bool updateAnimations)
        {
            if (!LayerType.MustDraw)
                return;

            LayerType.Update(this, dataModel, preview);
            LayerAnimation.Update(this, updateAnimations);
        }

        public void Draw(IDataModel dataModel, DrawingContext c, bool preview, bool updateAnimations)
        {
            if (!LayerType.MustDraw)
                return;

            LayerType.Draw(this, c);
        }

        public void SetupProperties()
        {
            LayerType.SetupProperties(this);

            // If the type is an event, set it up 
            if (IsEvent && EventProperties == null)
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
            return Enabled && LayerType.MustDraw;
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
                LayerType = new KeyboardType(),
                Properties = new KeyboardPropertiesModel
                {
                    Brush = new SolidColorBrush(ColorHelpers.GetRandomRainbowMediaColor()),
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
        public List<LayerModel> GetRenderLayers(IDataModel dataModel, bool includeMice, bool includeHeadsets,
            bool ignoreConditions = false)
        {
            var layers = new List<LayerModel>();
            foreach (var layerModel in Children.OrderByDescending(c => c.Order))
            {
                if (!layerModel.Enabled || !includeMice && layerModel.LayerType is MouseType ||
                    !includeHeadsets && layerModel.LayerType is HeadsetType)
                    continue;

                if (!ignoreConditions & !layerModel.ConditionsMet(dataModel))
                    continue;

                layers.Add(layerModel);
                layers.AddRange(layerModel.GetRenderLayers(dataModel, includeMice, includeHeadsets, ignoreConditions));
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
}