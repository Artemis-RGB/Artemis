using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Artemis.Modules.Abstract;
using Artemis.Profiles.Layers.Abstract;
using Artemis.Profiles.Layers.Animations;
using Artemis.Profiles.Layers.Conditions;
using Artemis.Profiles.Layers.Interfaces;
using Artemis.Profiles.Layers.Types.Keyboard;
using Artemis.Utilities;
using Artemis.Utilities.ParentChild;
using Betwixt;
using Newtonsoft.Json;

namespace Artemis.Profiles.Layers.Models
{
    public class LayerModel : IChildItem<LayerModel>, IChildItem<ProfileModel>
    {
        public LayerModel()
        {
            Children = new ChildItemCollection<LayerModel, LayerModel>(this);
            TweenModel = new TweenModel(this);
            RenderAllowed = true;

            var model = Properties as KeyboardPropertiesModel;
            if (model != null)
                GifImage = new GifImage(model.GifFile, Properties.AnimationSpeed);

            LayerConditionsMet += OnLayerConditionsMet;
            LayerConditionsUnmet += OnLayerConditionsUnmet;
        }

        public event EventHandler<EventArgs> LayerConditionsMet;
        public event EventHandler<EventArgs> LayerConditionsUnmet;

        [JsonIgnore]
        public ImageSource LayerImage => LayerType.DrawThumbnail(this);

        [JsonIgnore]
        public TweenModel TweenModel { get; set; }

        /// <summary>
        ///     Checks whether this layers conditions are met.
        ///     If they are met and this layer is an event, this also triggers that event.
        /// </summary>
        /// <param name="dataModel"></param>
        /// <returns></returns>
        public bool AreConditionsMet(ModuleDataModel dataModel)
        {
            // Conditions are not even checked if the layer isn't enabled
            if (!Enabled)
                return false;

            FadeTweener?.Update(40);
            var conditionsMet = LayerCondition.ConditionsMet(this, dataModel);
            if (conditionsMet && !_conditionsMetLastFrame)
                OnLayerConditionsMet();
            if (!conditionsMet && _conditionsMetLastFrame)
                OnLayerConditionsUnmet();

            _conditionsMetLastFrame = conditionsMet;
            return FadeTweener != null && FadeTweener.Running || conditionsMet;
        }

        /// <summary>
        ///     Update the layer
        /// </summary>
        /// <param name="dataModel"></param>
        /// <param name="preview"></param>
        /// <param name="updateAnimations"></param>
        public void Update(ModuleDataModel dataModel, bool preview, bool updateAnimations)
        {
            if (LayerType == null)
                return;

            LayerType.Update(this, dataModel, preview);
            LayerAnimation?.Update(this, updateAnimations);

            if (!preview)
                TweenModel.Update();

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
        public void Draw(ModuleDataModel dataModel, DrawingContext c, bool preview, bool updateAnimations)
        {
            if (Brush == null || !preview && !RenderAllowed)
                return;

            ApplyHierarchyOpacity(c);
            LayerType.Draw(this, c);
            PopHierarchyOpacity(c);
        }

        private void ApplyHierarchyOpacity(DrawingContext c)
        {
            if (FadeTweener != null && FadeTweener.Running)
                c.PushOpacity(FadeTweener.Value);

            var current = this;
            while (current.Parent != null)
            {
                current = current.Parent;
                if (current.FadeTweener != null && current.FadeTweener.Running)
                    c.PushOpacity(current.FadeTweener.Value);
            }
        }
        
        private void PopHierarchyOpacity(DrawingContext c)
        {
            if (FadeTweener != null && FadeTweener.Running)
                c.Pop();

            var current = this;
            while (current.Parent != null)
            {
                current = current.Parent;
                if (current.FadeTweener != null && current.FadeTweener.Running)
                    c.Pop();
            }
        }

        /// <summary>
        ///     Tells the current layer type to setup the layer's LayerProperties
        /// </summary>
        public void SetupProperties()
        {
            LayerType.SetupProperties(this);

            // If the type is an event, set it up 
            if (IsEvent && EventProperties == null)
                EventProperties = new KeyboardEventPropertiesModel
                {
                    ExpirationType = ExpirationType.Time,
                    Length = new TimeSpan(0, 0, 1),
                    TriggerDelay = new TimeSpan(0)
                };
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
                    Opacity = 1,
                    HeightEaseTime = 0,
                    HeightEase = "Linear",
                    WidthEaseTime = 0,
                    WidthEase = "Linear",
                    OpacityEaseTime = 0,
                    OpacityEase = "Linear"
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
                    if (child.Order >= source.Order)
                        child.Order++;
                Parent.Children.Add(source);
            }
            else if (Profile != null)
            {
                foreach (var layer in Profile.Layers.OrderBy(l => l.Order))
                    if (layer.Order >= source.Order)
                        layer.Order++;
                Profile.Layers.Add(source);
            }
        }

        public Rect LayerRect(int scale = 4)
        {
            var width = Width;
            var height = Height;
            if (width < 0)
                width = 0;
            if (height < 0)
                height = 0;

            return new Rect(X * scale, Y * scale, width * scale, height * scale);
        }

        // TODO: Make this and ProfileModel's GetRenderLayers the same through inheritance
        /// <summary>
        ///     Generates a flat list containing all layers that must be rendered on the keyboard,
        ///     the first mouse layer to be rendered and the first headset layer to be rendered
        /// </summary>
        /// <param name="dataModel">Instance of said game data model</param>
        /// <param name="keyboardOnly">Whether or not to ignore anything but keyboards</param>
        /// <param name="ignoreConditions"></param>
        /// <returns>A flat list containing all layers that must be rendered</returns>
        public List<LayerModel> GetRenderLayers(ModuleDataModel dataModel, bool keyboardOnly, bool ignoreConditions = false)
        {
            var layers = new List<LayerModel>();
            foreach (var layerModel in Children.OrderByDescending(l => l.Order))
            {
                if (!layerModel.Enabled || keyboardOnly && layerModel.LayerType.DrawType != DrawType.Keyboard)
                    continue;

                if (!ignoreConditions)
                {
                    if (!layerModel.AreConditionsMet(dataModel) || !layerModel.RenderAllowed)
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

        public void SetupKeybinds()
        {
            RenderAllowed = true;

            // Clean up old keybinds
            RemoveKeybinds();

            for (var index = 0; index < Properties.LayerKeybindModels.Count; index++)
            {
                var keybindModel = Properties.LayerKeybindModels[index];
                keybindModel.Register(this, index);
            }
        }

        public void RemoveKeybinds()
        {
            foreach (var keybindModel in Properties.LayerKeybindModels)
                keybindModel.Unregister();
        }

        private void OnLayerConditionsMet(object sender, EventArgs eventArgs)
        {
            if (FadeInTime <= 0)
                return;
            if (FadeTweener != null && FadeTweener.Running)
                FadeTweener = new Tweener<float>(FadeTweener.Value, 1, FadeInTime, Ease.Quint.Out, TweenModel.LerpFuncFloat);
            else
                FadeTweener = new Tweener<float>(0, 1, FadeInTime, Ease.Quint.Out, TweenModel.LerpFuncFloat);
            FadeTweener.Start();
        }

        private void OnLayerConditionsUnmet(object sender, EventArgs eventArgs)
        {
            if (FadeOutTime <= 0)
                return;

            if (FadeTweener != null && FadeTweener.Running)
                FadeTweener = new Tweener<float>(FadeTweener.Value, 0, FadeOutTime, Ease.Quint.In, TweenModel.LerpFuncFloat);
            else
                FadeTweener = new Tweener<float>(1, 0, FadeOutTime, Ease.Quint.In, TweenModel.LerpFuncFloat);
            FadeTweener.Start();
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
        public bool RenderAllowed { get; set; }
        public bool Expanded { get; set; }
        public bool IsEvent { get; set; }
        public double FadeInTime { get; set; }
        public double FadeOutTime { get; set; }
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
        [JsonIgnore] private bool _conditionsMetLastFrame;

        [JsonIgnore]
        public Tweener<float> FadeTweener { get; set; }

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

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{nameof(Name)}: {Name}, {nameof(Order)}: {Order}, {nameof(X)}: {X}, {nameof(Y)}: {Y}, {nameof(Width)}: {Width}, {nameof(Height)}: {Height}";
        }

        protected virtual void OnLayerConditionsMet()
        {
            LayerConditionsMet?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnLayerConditionsUnmet()
        {
            LayerConditionsUnmet?.Invoke(this, EventArgs.Empty);
        }
    }
}
