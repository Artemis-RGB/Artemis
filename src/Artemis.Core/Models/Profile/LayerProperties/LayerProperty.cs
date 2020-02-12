using System.Collections.ObjectModel;
using System.Linq;
using Artemis.Core.Plugins.LayerBrush;
using Artemis.Core.Plugins.Models;

namespace Artemis.Core.Models.Profile.LayerProperties
{
    /// <summary>
    ///     Represents a property on the layer. This property is visible in the profile editor and can be key-framed (unless
    ///     opted out).
    ///     <para>To create and register a new LayerProperty use <see cref="LayerBrush.RegisterLayerProperty" /></para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LayerProperty<T> : BaseLayerProperty
    {
        internal LayerProperty(Layer layer, BaseLayerProperty parent, string id, string name, string description)
            : base(layer, null, parent, id, name, description, typeof(T))
        {
        }

        internal LayerProperty(Layer layer, string id, string name, string description)
            : base(layer, null, null, id, name, description, typeof(T))
        {
        }

        internal LayerProperty(Layer layer, PluginInfo pluginInfo, BaseLayerProperty parent, string id, string name, string description)
            : base(layer, pluginInfo, parent, id, name, description, typeof(T))
        {
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