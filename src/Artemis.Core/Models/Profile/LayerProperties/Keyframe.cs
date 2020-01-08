namespace Artemis.Core.Models.Profile.LayerProperties
{
    /// <inheritdoc />
    public class Keyframe<T> : BaseKeyframe
    {
        public Keyframe(Layer layer, LayerProperty<T> propertyBase) : base(layer, propertyBase)
        {
        }

        public LayerProperty<T> Property => (LayerProperty<T>) BaseProperty;

        public T Value
        {
            get => (T) BaseValue;
            set => BaseValue = value;
        }
    }
}