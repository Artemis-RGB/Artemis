using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core.Events;
using Artemis.Core.Exceptions;
using Artemis.Core.Plugins.Models;
using SkiaSharp;

namespace Artemis.Core.Models.Profile.LayerProperties
{
    /// <summary>
    /// Contains all the properties of the layer and provides easy access to the default properties.
    /// </summary>
    public class LayerPropertyCollection : IEnumerable<BaseLayerProperty>
    {
        private readonly Dictionary<(Guid, string), BaseLayerProperty> _properties;

        internal LayerPropertyCollection(Layer layer)
        {
            _properties = new Dictionary<(Guid, string), BaseLayerProperty>();

            Layer = layer;
            CreateDefaultProperties();
        }

        /// <summary>
        ///     Gets the layer these properties are applied on
        /// </summary>
        public Layer Layer { get; }

        /// <inheritdoc />
        public IEnumerator<BaseLayerProperty> GetEnumerator()
        {
            return _properties.Values.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        ///     Removes the provided layer property from the layer.
        /// </summary>
        /// <typeparam name="T">The type of value of the layer property</typeparam>
        /// <param name="layerProperty">The property to remove from the layer</param>
        public void RemoveLayerProperty<T>(LayerProperty<T> layerProperty)
        {
            RemoveLayerProperty((BaseLayerProperty) layerProperty);
        }

        /// <summary>
        ///     Removes the provided layer property from the layer.
        /// </summary>
        /// <param name="layerProperty">The property to remove from the layer</param>
        public void RemoveLayerProperty(BaseLayerProperty layerProperty)
        {
            if (!_properties.ContainsKey((layerProperty.PluginInfo.Guid, layerProperty.Id)))
                throw new ArtemisCoreException($"Could not find a property with ID {layerProperty.Id}.");

            var property = _properties[(layerProperty.PluginInfo.Guid, layerProperty.Id)];
            property.Parent?.Children.Remove(property);
            _properties.Remove((layerProperty.PluginInfo.Guid, layerProperty.Id));

            OnLayerPropertyRemoved(new LayerPropertyEventArgs(property));
        }

        /// <summary>
        ///     If found, returns the <see cref="LayerProperty{T}" /> matching the provided ID
        /// </summary>
        /// <typeparam name="T">The type of the layer property</typeparam>
        /// <param name="pluginInfo">The plugin this property belongs to</param>
        /// <param name="id"></param>
        /// <returns></returns>
        public LayerProperty<T> GetLayerPropertyById<T>(PluginInfo pluginInfo, string id)
        {
            if (!_properties.ContainsKey((pluginInfo.Guid, id)))
                return null;

            var property = _properties[(pluginInfo.Guid, id)];
            if (property.Type != typeof(T))
                throw new ArtemisCoreException($"Property type mismatch. Expected property {property} to have type {typeof(T)} but it has {property.Type} instead.");
            return (LayerProperty<T>) _properties[(pluginInfo.Guid, id)];
        }

        /// <summary>
        ///     Adds the provided layer property and its children to the layer.
        ///     If found, the last stored base value and keyframes will be applied to the provided property.
        /// </summary>
        /// <typeparam name="T">The type of value of the layer property</typeparam>
        /// <param name="layerProperty">The property to apply to the layer</param>
        /// <returns>True if an existing value was found and applied, otherwise false.</returns>
        public bool RegisterLayerProperty<T>(LayerProperty<T> layerProperty)
        {
            return RegisterLayerProperty((BaseLayerProperty) layerProperty);
        }

        /// <summary>
        ///     Adds the provided layer property to the layer.
        ///     If found, the last stored base value and keyframes will be applied to the provided property.
        /// </summary>
        /// <param name="layerProperty">The property to apply to the layer</param>
        /// <returns>True if an existing value was found and applied, otherwise false.</returns>
        public bool RegisterLayerProperty(BaseLayerProperty layerProperty)
        {
            if (_properties.ContainsKey((layerProperty.PluginInfo.Guid, layerProperty.Id)))
                throw new ArtemisCoreException($"Duplicate property ID detected. Layer already contains a property with ID {layerProperty.Id}.");

            var entity = Layer.LayerEntity.PropertyEntities.FirstOrDefault(p => p.Id == layerProperty.Id && p.ValueType == layerProperty.Type.Name);
            // TODO: Catch serialization exceptions and log them
            if (entity != null)
                layerProperty.ApplyToProperty(entity);

            _properties.Add((layerProperty.PluginInfo.Guid, layerProperty.Id), layerProperty);
            OnLayerPropertyRegistered(new LayerPropertyEventArgs(layerProperty));
            return entity != null;
        }

        #region Default properties

        /// <summary>
        ///     Gets the shape type property of the layer
        /// </summary>
        public LayerProperty<LayerShapeType> ShapeType { get; private set; }

        /// <summary>
        ///     Gets the fill type property of the layer
        /// </summary>
        public LayerProperty<LayerFillType> FillType { get; private set; }

        /// <summary>
        ///     Gets the blend mode property of the layer
        /// </summary>
        public LayerProperty<SKBlendMode> BlendMode { get; private set; }

        /// <summary>
        ///     Gets the brush reference property of the layer
        /// </summary>
        public LayerProperty<LayerBrushReference> BrushReference { get; private set; }

        /// <summary>
        ///     Gets the anchor point property of the layer
        /// </summary>
        public LayerProperty<SKPoint> AnchorPoint { get; private set; }

        /// <summary>
        ///     Gets the position of the layer
        /// </summary>
        public LayerProperty<SKPoint> Position { get; private set; }

        /// <summary>
        ///     Gets the size property of the layer
        /// </summary>
        public LayerProperty<SKSize> Scale { get; private set; }

        /// <summary>
        ///     Gets the rotation property of the layer range 0 - 360
        /// </summary>
        public LayerProperty<float> Rotation { get; private set; }

        /// <summary>
        ///     Gets the opacity property of the layer range 0 - 100
        /// </summary>
        public LayerProperty<float> Opacity { get; private set; }

        private void CreateDefaultProperties()
        {
            // Shape
            var shape = new LayerProperty<object>(Layer, "Core.Shape", "Shape", "A collection of basic shape properties");
            ShapeType = new LayerProperty<LayerShapeType>(Layer, shape, "Core.ShapeType", "Shape type", "The type of shape to draw in this layer") {CanUseKeyframes = false};
            FillType = new LayerProperty<LayerFillType>(Layer, shape, "Core.FillType", "Fill type", "How to make the shape adjust to scale changes") {CanUseKeyframes = false};
            BlendMode = new LayerProperty<SKBlendMode>(Layer, shape, "Core.BlendMode", "Blend mode", "How to blend this layer into the resulting image") {CanUseKeyframes = false};
            ShapeType.Value = LayerShapeType.Rectangle;
            FillType.Value = LayerFillType.Stretch;
            BlendMode.Value = SKBlendMode.SrcOver;

            RegisterLayerProperty(shape);
            foreach (var shapeProperty in shape.Children)
                RegisterLayerProperty(shapeProperty);

            // Brush
            var brush = new LayerProperty<object>(Layer, "Core.Brush", "Brush", "A collection of properties that configure the selected brush");
            BrushReference = new LayerProperty<LayerBrushReference>(Layer, brush, "Core.BrushReference", "Brush type", "The type of brush to use for this layer") {CanUseKeyframes = false};

            RegisterLayerProperty(brush);
            foreach (var brushProperty in brush.Children)
                RegisterLayerProperty(brushProperty);

            // Transform
            var transform = new LayerProperty<object>(Layer, "Core.Transform", "Transform", "A collection of transformation properties") {ExpandByDefault = true};
            AnchorPoint = new LayerProperty<SKPoint>(Layer, transform, "Core.AnchorPoint", "Anchor Point", "The point at which the shape is attached to its position") {InputStepSize = 0.001f};
            Position = new LayerProperty<SKPoint>(Layer, transform, "Core.Position", "Position", "The position of the shape") {InputStepSize = 0.001f};
            Scale = new LayerProperty<SKSize>(Layer, transform, "Core.Scale", "Scale", "The scale of the shape") {InputAffix = "%", MinInputValue = 0f};
            Rotation = new LayerProperty<float>(Layer, transform, "Core.Rotation", "Rotation", "The rotation of the shape in degrees") {InputAffix = "°"};
            Opacity = new LayerProperty<float>(Layer, transform, "Core.Opacity", "Opacity", "The opacity of the shape") {InputAffix = "%", MinInputValue = 0f, MaxInputValue = 100f};
            Scale.Value = new SKSize(100, 100);
            Opacity.Value = 100;

            RegisterLayerProperty(transform);
            foreach (var transformProperty in transform.Children)
                RegisterLayerProperty(transformProperty);
        }

        #endregion

        #region Events

        public event EventHandler<LayerPropertyEventArgs> LayerPropertyRegistered;
        public event EventHandler<LayerPropertyEventArgs> LayerPropertyRemoved;

        private void OnLayerPropertyRegistered(LayerPropertyEventArgs e)
        {
            LayerPropertyRegistered?.Invoke(this, e);
        }

        private void OnLayerPropertyRemoved(LayerPropertyEventArgs e)
        {
            LayerPropertyRemoved?.Invoke(this, e);
        }

        #endregion
    }
}