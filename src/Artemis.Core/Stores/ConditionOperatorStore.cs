using System;
using System.Collections.Generic;
using System.Linq;

namespace Artemis.Core
{
    internal class ConditionOperatorStore
    {
        private static readonly List<ConditionOperatorRegistration> Registrations = new List<ConditionOperatorRegistration>();

        public static ConditionOperatorRegistration Add(ConditionOperator conditionOperator)
        {
            ConditionOperatorRegistration registration;
            lock (Registrations)
            {
                if (Registrations.Any(r => r.ConditionOperator == conditionOperator))
                    throw new ArtemisCoreException($"Condition operator store store already contains operator '{conditionOperator.Description}'");

                registration = new ConditionOperatorRegistration(conditionOperator, conditionOperator.PluginInfo.Instance) {IsInStore = true};
                Registrations.Add(registration);
            }

            OnConditionOperatorAdded(new ConditionOperatorStoreEvent(registration));
            return registration;
        }

        public static void Remove(ConditionOperatorRegistration registration)
        {
            lock (Registrations)
            {
                if (!Registrations.Contains(registration))
                    throw new ArtemisCoreException($"Condition operator store does not contain operator '{registration.ConditionOperator.Description}'");

                Registrations.Remove(registration);
                registration.IsInStore = false;
            }

            OnConditionOperatorRemoved(new ConditionOperatorStoreEvent(registration));
        }

        public static ConditionOperatorRegistration Get(Guid pluginGuid, string type)
        {
            lock (Registrations)
            {
                return Registrations.FirstOrDefault(r => r.Plugin.PluginInfo.Guid == pluginGuid && r.ConditionOperator.GetType().Name == type);
            }
        }

        public static List<ConditionOperatorRegistration> GetForType(Type type)
        {
            lock (Registrations)
            {
                if (type == null)
                    return new List<ConditionOperatorRegistration>(Registrations);

                List<ConditionOperatorRegistration> candidates = Registrations.Where(r => r.ConditionOperator.CompatibleTypes.Any(t => t.IsCastableFrom(type))).ToList();

                // If there are multiple operators with the same description, use the closest match
                foreach (IGrouping<string, ConditionOperatorRegistration> candidate in candidates.GroupBy(r => r.ConditionOperator.Description).Where(g => g.Count() > 1).ToList())
                {
                    ConditionOperatorRegistration closest = candidate.OrderByDescending(r => r.ConditionOperator.CompatibleTypes.Contains(type)).FirstOrDefault();
                    foreach (ConditionOperatorRegistration conditionOperator in candidate)
                    {
                        if (conditionOperator != closest)
                            candidates.Remove(conditionOperator);
                    }
                }

                return candidates;
            }
        }

        #region Events

        public static event EventHandler<ConditionOperatorStoreEvent> ConditionOperatorAdded;
        public static event EventHandler<ConditionOperatorStoreEvent> ConditionOperatorRemoved;

        private static void OnConditionOperatorAdded(ConditionOperatorStoreEvent e)
        {
            ConditionOperatorAdded?.Invoke(null, e);
        }

        private static void OnConditionOperatorRemoved(ConditionOperatorStoreEvent e)
        {
            ConditionOperatorRemoved?.Invoke(null, e);
        }

        #endregion
    }
}