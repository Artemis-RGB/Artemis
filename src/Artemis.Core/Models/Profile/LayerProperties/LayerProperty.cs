using System.Collections.ObjectModel;
using System.Linq;

namespace Artemis.Core.Models.Profile.LayerProperties
{
    public class LayerProperty<T> : BaseLayerProperty
    {
        public LayerProperty(Layer layer, BaseLayerProperty parent, string id, string name, string description) : base(layer, parent, id, name, description, typeof(T))
        {
        }

        /// <summary>
        ///     The value of the property without any keyframes applied
        /// </summary>
        public T Value
        {
            get => BaseValue != null ? (T) BaseValue : default;
            set => BaseValue = value;
        }

        /// <summary>
        ///     The value of the property with keyframes applied
        /// </summary>
        public T CurrentValue => (T) KeyframeEngine.GetCurrentValue();

        /// <summary>
        ///     A list of keyframes defining different values of the property in time, this list contains the strongly typed
        ///     <see cref="Keyframe{T}" />
        /// </summary>
        public ReadOnlyCollection<Keyframe<T>> Keyframes => BaseKeyframes.Cast<Keyframe<T>>().ToList().AsReadOnly();

        /// <summary>
        ///     Adds a keyframe to the property.
        /// </summary>
        /// <param name="keyframe">The keyframe to remove</param>
        public void AddKeyframe(Keyframe<T> keyframe)
        {
            BaseKeyframes.Add(keyframe);
        }

        /// <summary>
        ///     Removes a keyframe from the property.
        /// </summary>
        /// <param name="keyframe">The keyframe to remove</param>
        public void RemoveKeyframe(Keyframe<T> keyframe)
        {
            BaseKeyframes.Remove(keyframe);
        }

        /// <summary>
        ///     Gets the current value using the keyframes
        /// </summary>
        /// <returns></returns>
        public T GetCurrentValue()
        {
            if (KeyframeEngine == null || !Keyframes.Any())
                return Value;

            return (T) KeyframeEngine.GetCurrentValue();
        }
    }
}