using System;
using System.Collections.Generic;
using System.Linq;

namespace Artemis.Core
{
    internal class NodeTypeStore
    {
        private static readonly List<NodeTypeRegistration> Registrations = new();

        public static NodeTypeRegistration Add(NodeData nodeData)
        {
            if (nodeData.Plugin == null)
                throw new ArtemisCoreException("Cannot add a data binding modifier type that is not associated with a plugin");

            NodeTypeRegistration typeRegistration;
            lock (Registrations)
            {
                if (Registrations.Any(r => r.NodeData == nodeData))
                    throw new ArtemisCoreException($"Data binding modifier type store already contains modifier '{nodeData.Name}'");

                typeRegistration = new NodeTypeRegistration(nodeData, nodeData.Plugin) { IsInStore = true };
                Registrations.Add(typeRegistration);
            }

            OnDataBindingModifierAdded(new NodeTypeStoreEvent(typeRegistration));
            return typeRegistration;
        }

        public static void Remove(NodeTypeRegistration typeRegistration)
        {
            lock (Registrations)
            {
                if (!Registrations.Contains(typeRegistration))
                    throw new ArtemisCoreException($"Data binding modifier type store does not contain modifier type '{typeRegistration.NodeData.Name}'");

                Registrations.Remove(typeRegistration);
                typeRegistration.IsInStore = false;
            }

            OnDataBindingModifierRemoved(new NodeTypeStoreEvent(typeRegistration));
        }

        public static IEnumerable<NodeData> GetAll()
        {
            lock (Registrations)
            {
                return Registrations.Select(r => r.NodeData).ToList();
            }
        }

        public static NodeTypeRegistration? Get(Guid pluginGuid, string type)
        {
            lock (Registrations)
            {
                return Registrations.FirstOrDefault(r => r.Plugin.Guid == pluginGuid && r.NodeData.Type.Name == type);
            }
        }

        #region Events

        public static event EventHandler<NodeTypeStoreEvent>? DataBindingModifierAdded;
        public static event EventHandler<NodeTypeStoreEvent>? DataBindingModifierRemoved;

        private static void OnDataBindingModifierAdded(NodeTypeStoreEvent e)
        {
            DataBindingModifierAdded?.Invoke(null, e);
        }

        private static void OnDataBindingModifierRemoved(NodeTypeStoreEvent e)
        {
            DataBindingModifierRemoved?.Invoke(null, e);
        }

        #endregion
    }
}
