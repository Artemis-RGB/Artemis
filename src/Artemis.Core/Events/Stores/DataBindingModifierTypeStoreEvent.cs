namespace Artemis.Core
{
    internal class DataBindingModifierTypeStoreEvent
    {
        public DataBindingModifierTypeStoreEvent(DataBindingModifierTypeRegistration typeRegistration)
        {
            TypeRegistration = typeRegistration;
        }

        public DataBindingModifierTypeRegistration TypeRegistration { get; }
    }
}