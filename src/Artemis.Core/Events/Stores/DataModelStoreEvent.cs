namespace Artemis.Core;

internal class DataModelStoreEvent
{
    public DataModelStoreEvent(DataModelRegistration registration)
    {
        Registration = registration;
    }

    public DataModelRegistration Registration { get; }
}