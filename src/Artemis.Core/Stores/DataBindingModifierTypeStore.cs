using System;
using System.Collections.Generic;
using System.Linq;

namespace Artemis.Core
{
    internal class DataBindingModifierTypeStore
    {
        private static readonly List<DataBindingModifierTypeRegistration> Registrations = new List<DataBindingModifierTypeRegistration>();

        public static DataBindingModifierTypeRegistration Add(DataBindingModifierType modifierType)
        {
            DataBindingModifierTypeRegistration typeRegistration;
            lock (Registrations)
            {
                if (Registrations.Any(r => r.DataBindingModifierType == modifierType))
                    throw new ArtemisCoreException($"Data binding modifier type store already contains modifier '{modifierType.Name}'");

                typeRegistration = new DataBindingModifierTypeRegistration(modifierType, modifierType.PluginInfo.Instance) { IsInStore = true };
                Registrations.Add(typeRegistration);
            }

            OnDataBindingModifierAdded(new DataBindingModifierTypeStoreEvent(typeRegistration));
            return typeRegistration;
        }

        public static void Remove(DataBindingModifierTypeRegistration typeRegistration)
        {
            lock (Registrations)
            {
                if (!Registrations.Contains(typeRegistration))
                    throw new ArtemisCoreException($"Data binding modifier type store does not contain modifier type '{typeRegistration.DataBindingModifierType.Name}'");

                Registrations.Remove(typeRegistration);
                typeRegistration.IsInStore = false;
            }

            OnDataBindingModifierRemoved(new DataBindingModifierTypeStoreEvent(typeRegistration));
        }

        public static DataBindingModifierTypeRegistration Get(Guid pluginGuid, string type)
        {
            lock (Registrations)
            {
                return Registrations.FirstOrDefault(r => r.Plugin.PluginInfo.Guid == pluginGuid && r.DataBindingModifierType.GetType().Name == type);
            }
        }

        public static List<DataBindingModifierTypeRegistration> GetForType(Type type)
        {
            lock (Registrations)
            {
                if (type == null)
                    return new List<DataBindingModifierTypeRegistration>(Registrations);

                List<DataBindingModifierTypeRegistration> candidates = Registrations.Where(r => r.DataBindingModifierType.CompatibleTypes.Any(t => t == type)).ToList();

                // If there are multiple operators with the same description, use the closest match
                foreach (IGrouping<string, DataBindingModifierTypeRegistration> displayDataBindingModifiers in candidates.GroupBy(r => r.DataBindingModifierType.Name).Where(g => g.Count() > 1).ToList())
                {
                    DataBindingModifierTypeRegistration closest = displayDataBindingModifiers.OrderByDescending(r => r.DataBindingModifierType.CompatibleTypes.Contains(type)).FirstOrDefault();
                    foreach (DataBindingModifierTypeRegistration displayDataBindingModifier in displayDataBindingModifiers)
                    {
                        if (displayDataBindingModifier != closest)
                            candidates.Remove(displayDataBindingModifier);
                    }
                }

                return candidates;
            }
        }

        #region Events

        public static event EventHandler<DataBindingModifierTypeStoreEvent> DataBindingModifierAdded;
        public static event EventHandler<DataBindingModifierTypeStoreEvent> DataBindingModifierRemoved;

        private static void OnDataBindingModifierAdded(DataBindingModifierTypeStoreEvent e)
        {
            DataBindingModifierAdded?.Invoke(null, e);
        }

        private static void OnDataBindingModifierRemoved(DataBindingModifierTypeStoreEvent e)
        {
            DataBindingModifierRemoved?.Invoke(null, e);
        }

        #endregion
    }
}