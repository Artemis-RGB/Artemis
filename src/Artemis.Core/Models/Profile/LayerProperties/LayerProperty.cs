using System;
using System.Collections.ObjectModel;
using System.Linq;
using Artemis.Core.Plugins.Models;

namespace Artemis.Core.Models.Profile.LayerProperties
{
    /// <summary>
    ///     Represents a property on the layer. This property is visible in the profile editor and can be key-framed (unless
    ///     opted out)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LayerProperty<T> : BaseLayerProperty
    {
        internal LayerProperty(Layer layer, BaseLayerProperty parent, string id, string name, string description) : base(layer, null, parent, id, name, description, typeof(T))
        {
        }

        internal LayerProperty(Layer layer, string id, string name, string description) : base(layer, null, null, id, name, description, typeof(T))
        {
        }

        /// <summary>
        ///     Represents a property on the layer. This property is visible in the profile editor and can be key-framed (unless
        ///     opted out)
        ///     <para>
        ///         Note: The value and keyframes of the property are stored using the ID, after adding the property to the layer
        ///         these are restored.
        ///     </para>
        /// </summary>
        /// <param name="layer">The layer the property is applied to</param>
        /// <param name="pluginInfo">The plugin to create this property for</param>
        /// <param name="parent">The parent of this property, use this to create a tree-hierarchy in the editor</param>
        /// <param name="id">A and ID identifying your property, must be unique within your plugin</param>
        /// <param name="name">A name for your property, this is visible in the editor</param>
        /// <param name="description">A description for your property, this is visible in the editor</param>
        public LayerProperty(Layer layer, PluginInfo pluginInfo, BaseLayerProperty parent, string id, string name, string description) : base(layer, pluginInfo, parent, id, name, description,
            typeof(T))
        {
            if (layer == null)
                throw new ArgumentNullException(nameof(layer));
            if (pluginInfo == null)
                throw new ArgumentNullException(nameof(pluginInfo));
            if (id == null)
                throw new ArgumentNullException(nameof(id));
        }

        /// <summary>
        ///     Represents a property on the layer. This property is visible in the profile editor and can be key-framed (unless
        ///     opted out)
        ///     <para>
        ///         Note: The value and keyframes of the property are stored using the ID, after adding the property to the layer
        ///         these are restored.
        ///     </para>
        /// </summary>
        /// <param name="layer">The layer the property is applied to</param>
        /// <param name="pluginInfo">The plugin to create this property for</param>
        /// <param name="id">A and ID identifying your property, must be unique within your plugin</param>
        /// <param name="name">A name for your property, this is visible in the editor</param>
        /// <param name="description">A description for your property, this is visible in the editor</param>
        public LayerProperty(Layer layer, PluginInfo pluginInfo, string id, string name, string description) : base(layer, pluginInfo, null, id, name, description, typeof(T))
        {
            if (layer == null)
                throw new ArgumentNullException(nameof(layer));
            if (pluginInfo == null)
                throw new ArgumentNullException(nameof(pluginInfo));
            if (id == null)
                throw new ArgumentNullException(nameof(id));
        }

        /// <summary>
        ///     Gets or sets the value of the property without any keyframes applied
        /// </summary>
        public T Value
        {
            get => BaseValue != null ? (T) BaseValue : default;
            set => BaseValue = value;
        }

        /// <summary>
        ///     Gets the value of the property with keyframes applied
        /// </summary>
        public T CurrentValue
        {
            get
            {
                var currentValue = GetCurrentValue();
                return currentValue == null ? default : (T) currentValue;
            }
        }

        /// <summary>
        ///     Gets a list of keyframes defining different values of the property in time, this list contains the strongly typed
        ///     <see cref="Keyframe{T}" />
        /// </summary>
        public ReadOnlyCollection<Keyframe<T>> Keyframes => BaseKeyframes.Cast<Keyframe<T>>().ToList().AsReadOnly();

        /// <summary>
        ///     Adds a keyframe to the property.
        /// </summary>
        /// <param name="keyframe">The keyframe to remove</param>
        public void AddKeyframe(Keyframe<T> keyframe)
        {
            base.AddKeyframe(keyframe);
        }

        /// <summary>
        ///     Removes a keyframe from the property.
        /// </summary>
        /// <param name="keyframe">The keyframe to remove</param>
        public void RemoveKeyframe(Keyframe<T> keyframe)
        {
            base.RemoveKeyframe(keyframe);
        }
    }
}