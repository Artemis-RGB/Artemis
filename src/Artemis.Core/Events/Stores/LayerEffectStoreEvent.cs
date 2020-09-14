namespace Artemis.Core
{
    internal class LayerEffectStoreEvent
    {
        public LayerEffectStoreEvent(LayerEffectRegistration registration)
        {
            Registration = registration;
        }

        public LayerEffectRegistration Registration { get; }
    }
}