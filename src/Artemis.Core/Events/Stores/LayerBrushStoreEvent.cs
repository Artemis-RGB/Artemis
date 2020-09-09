namespace Artemis.Core
{
    internal class LayerBrushStoreEvent
    {
        public LayerBrushStoreEvent(LayerBrushRegistration registration)
        {
            Registration = registration;
        }

        public LayerBrushRegistration Registration { get; }
    }
}