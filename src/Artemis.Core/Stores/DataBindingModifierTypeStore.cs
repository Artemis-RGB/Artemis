﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Artemis.Core
{
    internal class DataBindingModifierTypeStore
    {
        private static readonly List<DataBindingModifierTypeRegistration> Registrations = new();

        public static DataBindingModifierTypeRegistration Add(BaseDataBindingModifierType modifierType)
        {
            if (modifierType.Plugin == null)
                throw new ArtemisCoreException("Cannot add a data binding modifier type that is not associated with a plugin");

            DataBindingModifierTypeRegistration typeRegistration;
            lock (Registrations)
            {
                if (Registrations.Any(r => r.DataBindingModifierType == modifierType))
                    throw new ArtemisCoreException($"Data binding modifier type store already contains modifier '{modifierType.Name}'");

                typeRegistration = new DataBindingModifierTypeRegistration(modifierType, modifierType.Plugin) { IsInStore = true };
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

        public static DataBindingModifierTypeRegistration? Get(Guid pluginGuid, string type)
        {
            lock (Registrations)
            {
                return Registrations.FirstOrDefault(r => r.Plugin.Guid == pluginGuid && r.DataBindingModifierType.GetType().Name == type);
            }
        }

        public static List<DataBindingModifierTypeRegistration> GetForType(Type type, ModifierTypePart part)
        {
            lock (Registrations)
            {
                if (type == null)
                    return new List<DataBindingModifierTypeRegistration>(Registrations);

                List<DataBindingModifierTypeRegistration> candidates = Registrations.Where(r => r.DataBindingModifierType.SupportsType(type, part)).ToList();

                // If there are multiple modifiers with the same description, use the closest match
                foreach (IGrouping<string, DataBindingModifierTypeRegistration> displayDataBindingModifiers in candidates.GroupBy(r => r.DataBindingModifierType.Name).Where(g => g.Count() > 1).ToList())
                {
                    DataBindingModifierTypeRegistration closest = part == ModifierTypePart.Value
                        ? displayDataBindingModifiers.OrderByDescending(r => r.DataBindingModifierType.ValueType.ScoreCastability(type)).First()
                        : displayDataBindingModifiers.OrderByDescending(r => r.DataBindingModifierType.ParameterType!.ScoreCastability(type)).First();
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

        public static event EventHandler<DataBindingModifierTypeStoreEvent>? DataBindingModifierAdded;
        public static event EventHandler<DataBindingModifierTypeStoreEvent>? DataBindingModifierRemoved;

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