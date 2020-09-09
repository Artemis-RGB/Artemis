namespace Artemis.Core
{
    internal class ConditionOperatorStoreEvent
    {
        public ConditionOperatorStoreEvent(ConditionOperatorRegistration registration)
        {
            Registration = registration;
        }

        public ConditionOperatorRegistration Registration { get; }
    }
}