using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core.LayerEffects;

namespace Artemis.Core
{
    internal class LayerEffectStore
    {
        private static readonly List<LayerEffectRegistration> Registrations = new List<LayerEffectRegistration>();

        public static LayerEffectRegistration Add(LayerEffectDescriptor descriptor)
        {
            LayerEffectRegistration registration;
            lock (Registrations)
            {
                if (Registrations.Any(r => r.LayerEffectDescriptor == descriptor))
                    throw new ArtemisCoreException($"Store already contains layer brush '{descriptor.DisplayName}'");

                registration = new LayerEffectRegistration(descriptor, descriptor.LayerEffectProvider.PluginInfo.Instance) { IsInStore = true };
                Registrations.Add(registration);
            }

            OnLayerEffectAdded(new LayerEffectStoreEvent(registration));
            return registration;
        }

        public static void Remove(LayerEffectRegistration registration)
        {
            lock (Registrations)
            {
                if (!Registrations.Contains(registration))
                    throw new ArtemisCoreException($"Store does not contain layer brush '{registration.LayerEffectDescriptor.DisplayName}'");

                Registrations.Remove(registration);
                registration.IsInStore = false;
            }

            OnLayerEffectRemoved(new LayerEffectStoreEvent(registration));
        }

        public static List<LayerEffectRegistration> GetAll()
        {
            lock (Registrations)
            {
                return new List<LayerEffectRegistration>(Registrations);
            }
        }

        public static LayerEffectRegistration Get(Guid pluginGuid, string typeName)
        {
            lock (Registrations)
            {
                return Registrations.FirstOrDefault(d => d.Plugin.PluginInfo.Guid == pluginGuid && d.LayerEffectDescriptor.LayerEffectType.Name == typeName);
            }
        }

        #region Events

        public static event EventHandler<LayerEffectStoreEvent> LayerEffectAdded;
        public static event EventHandler<LayerEffectStoreEvent> LayerEffectRemoved;

        private static void OnLayerEffectAdded(LayerEffectStoreEvent e)
        {
            LayerEffectAdded?.Invoke(null, e);
        }

        private static void OnLayerEffectRemoved(LayerEffectStoreEvent e)
        {
            LayerEffectRemoved?.Invoke(null, e);
        }

        #endregion
    }
}