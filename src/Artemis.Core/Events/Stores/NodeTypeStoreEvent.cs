namespace Artemis.Core
{
    internal class NodeTypeStoreEvent
    {
        public NodeTypeStoreEvent(NodeTypeRegistration typeRegistration)
        {
            TypeRegistration = typeRegistration;
        }

        public NodeTypeRegistration TypeRegistration { get; }
    }
}