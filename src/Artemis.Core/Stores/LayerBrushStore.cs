using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core.LayerBrushes;

namespace Artemis.Core;

internal class LayerBrushStore
{
    private static readonly List<LayerBrushRegistration> Registrations = [];

    public static LayerBrushRegistration Add(LayerBrushDescriptor descriptor)
    {
        LayerBrushRegistration registration;
        lock (Registrations)
        {
            if (Registrations.Any(r => r.LayerBrushDescriptor == descriptor))
                throw new ArtemisCoreException($"Store already contains layer brush '{descriptor.DisplayName}'");

            registration = new LayerBrushRegistration(descriptor, descriptor.Provider) {IsInStore = true};
            Registrations.Add(registration);
        }

        OnLayerBrushAdded(new LayerBrushStoreEvent(registration));
        return registration;
    }

    public static void Remove(LayerBrushRegistration registration)
    {
        lock (Registrations)
        {
            if (!Registrations.Contains(registration))
                throw new ArtemisCoreException($"Store does not contain layer brush '{registration.LayerBrushDescriptor.DisplayName}'");

            Registrations.Remove(registration);
            registration.IsInStore = false;
        }

        OnLayerBrushRemoved(new LayerBrushStoreEvent(registration));
    }

    public static List<LayerBrushRegistration> GetAll()
    {
        lock (Registrations)
        {
            return [..Registrations];
        }
    }

    public static LayerBrushRegistration? Get(string id, string typeName)
    {
        lock (Registrations)
        {
            return Registrations.FirstOrDefault(d => d.PluginFeature.Id == id &&
                                                     d.LayerBrushDescriptor.LayerBrushType.Name == typeName);
        }
    }

    public static event EventHandler<LayerBrushStoreEvent>? LayerBrushAdded;
    public static event EventHandler<LayerBrushStoreEvent>? LayerBrushRemoved;

    private static void OnLayerBrushAdded(LayerBrushStoreEvent e)
    {
        LayerBrushAdded?.Invoke(null, e);
    }

    private static void OnLayerBrushRemoved(LayerBrushStoreEvent e)
    {
        LayerBrushRemoved?.Invoke(null, e);
    }
}