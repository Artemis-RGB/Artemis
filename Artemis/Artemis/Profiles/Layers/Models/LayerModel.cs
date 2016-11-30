using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Artemis.Models.Interfaces;
using Artemis.Profiles.Layers.Animations;
using Artemis.Profiles.Layers.Conditions;
using Artemis.Profiles.Layers.Interfaces;
using Artemis.Profiles.Layers.Types.Keyboard;
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

        [JsonIgnore]
        public ImageSource LayerImage => LayerType.DrawThumbnail(this);

        /// <summary>
        ///     Checks whether this layers conditions are met.
        ///     If they are met and this layer is an event, this also triggers that event.
        /// </summary>
        /// <param name="dataModel"></param>
        /// <returns></returns>
        public bool ConditionsMet(IDataModel dataModel)
        {
            // Conditions are not even checked if the layer isn't enabled
            return Enabled && LayerCondition.ConditionsMet(this, dataModel);
        }

        /// <summary>
        ///     Update the layer
        /// </summary>
        /// <param name="dataModel"></param>
        /// <param name="preview"></param>
        /// <param name="updateAnimations"></param>
        public void Update(IDataModel dataModel, bool preview, bool updateAnimations)
        {
            LayerType.Update(this, dataModel, preview);
            LayerAnimation?.Update(this, updateAnimations);

            LastRender = DateTime.Now;
        }

        /// <summary>
        ///     Applies the saved properties to the current properties
        /// </summary>
        /// <param name="advanced">Include advanced properties (opacity, brush)</param>
        public void ApplyProperties(bool advanced)
        {
            X = Properties.X;
            Y = Properties.Y;
            Width = Properties.Width;
            Height = Properties.Height;

            if (!advanced)
                return;

            Opacity = Properties.Opacity;
            Brush = Properties.Brush;
        }

        /// <summary>
        ///     Draw the layer using the provided context
        /// </summary>
        /// <param name="dataModel"></param>
        /// <param name="c"></param>
        /// <param name="preview"></param>
        /// <param name="updateAnimations"></param>
        public void Draw(IDataModel dataModel, DrawingContext c, bool preview, bool updateAnimations)
        {
            LayerType.Draw(this, c);
        }

        /// <summary>
        ///     Tells the current layer type to setup the layer's LayerProperties
        /// </summary>
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

        /// <summary>
        ///     Ensures all child layers have a unique order
        /// </summary>
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
            return Enabled && LayerType.ShowInEdtor;
        }

        /// <summary>
        ///     Returns every descendant of this layer
        /// </summary>
        /// <returns></returns>
        public IEnumerable<LayerModel> GetLayers()
        {
            var layers = new List<LayerModel>();
            foreach (var layerModel in Children.OrderBy(c => c.Order))
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
                LayerCondition = new DataModelCondition(),
                LayerAnimation = new NoneAnimation(),
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

        /// <summary>
        ///     Insert this layer before the given layer
        /// </summary>
        /// <param name="source"></param>
        public void InsertBefore(LayerModel source)
        {
            source.Order = Order;
            Insert(source);
        }

        /// <summary>
        ///     Insert this layer after the given layer
        /// </summary>
        /// <param name="source"></param>
        public void InsertAfter(LayerModel source)
        {
            source.Order = Order + 1;
            Insert(source);
        }

        /// <summary>
        ///     Insert the layer as a sibling
        /// </summary>
        /// <param name="source"></param>
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

        public Rect LayerRect(int scale = 4)
        {
            return new Rect(X* scale, Y* scale, Width*scale, Height*scale);
        }

        /// <summary>
        ///     Generates a flat list containing all layers that must be rendered on the keyboard,
        ///     the first mouse layer to be rendered and the first headset layer to be rendered
        /// </summary>
        /// <param name="dataModel">Instance of said game data model</param>
        /// <param name="keyboardOnly">Whether or not to ignore anything but keyboards</param>
        /// <param name="ignoreConditions"></param>
        /// <returns>A flat list containing all layers that must be rendered</returns>
        public List<LayerModel> GetRenderLayers(IDataModel dataModel, bool keyboardOnly, bool ignoreConditions = false)
        {
            var layers = new List<LayerModel>();
            foreach (var layerModel in Children.OrderByDescending(l => l.Order))
            {
                if (!layerModel.Enabled || keyboardOnly && layerModel.LayerType.DrawType != DrawType.Keyboard)
                    continue;

                if (!ignoreConditions)
                {
                    if (!layerModel.ConditionsMet(dataModel))
                        continue;
                }

                layers.Add(layerModel);
                layers.AddRange(layerModel.GetRenderLayers(dataModel, keyboardOnly, ignoreConditions));
            }

            return layers;
        }

        public void SetupCondition()
        {
            if (IsEvent && !(LayerCondition is EventCondition))
                LayerCondition = new EventCondition();
            else if (!IsEvent && !(LayerCondition is DataModelCondition))
                LayerCondition = new DataModelCondition();
        }

        #region Properties

        #region Layer type properties

        public ILayerType LayerType { get; set; }
        public ILayerCondition LayerCondition { get; set; }
        public ILayerAnimation LayerAnimation { get; set; }

        #endregion

        #region Generic properties

        public string Name { get; set; }
        public int Order { get; set; }
        public bool Enabled { get; set; }
        public bool Expanded { get; set; }
        public bool IsEvent { get; set; }
        public LayerPropertiesModel Properties { get; set; }
        public EventPropertiesModel EventProperties { get; set; }

        #endregion

        #region Relational properties

        public ChildItemCollection<LayerModel, LayerModel> Children { get; }

        [JsonIgnore]
        public LayerModel Parent { get; internal set; }

        [JsonIgnore]
        public ProfileModel Profile { get; internal set; }

        #endregion

        #region Render properties

        [JsonIgnore] private Brush _brush;

        [JsonIgnore]
        public double X { get; set; }

        [JsonIgnore]
        public double Y { get; set; }

        [JsonIgnore]
        public double Width { get; set; }

        [JsonIgnore]
        public double Height { get; set; }

        [JsonIgnore]
        public double Opacity { get; set; }

        [JsonIgnore]
        public Brush Brush
        {
            get { return _brush; }
            set
            {
                if (value == null)
                {
                    _brush = null;
                    return;
                }

                if (value.IsFrozen)
                {
                    _brush = value;
                    return;
                }

                // Clone the brush off of the UI thread and freeze it
                var cloned = value.Dispatcher.Invoke(value.CloneCurrentValue);
                cloned.Freeze();
                _brush = cloned;
            }
        }

        [JsonIgnore]
        public double AnimationProgress { get; set; }

        [JsonIgnore]
        public GifImage GifImage { get; set; }

        [JsonIgnore]
        public DateTime LastRender { get; set; }

        #endregion

        #endregion

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