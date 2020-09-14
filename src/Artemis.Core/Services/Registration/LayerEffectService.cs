using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core.LayerEffects;

namespace Artemis.Core.Services
{
    internal class LayerEffectService : ILayerEffectService
    {
        public LayerEffectRegistration RegisterLayerEffect(LayerEffectDescriptor descriptor)
        {
            if (descriptor == null)
                throw new ArgumentNullException(nameof(descriptor));

            return LayerEffectStore.Add(descriptor);
        }

        public void RemoveLayerEffect(LayerEffectRegistration registration)
        {
            if (registration == null)
                throw new ArgumentNullException(nameof(registration));
            LayerEffectStore.Remove(registration);
        }

        public List<LayerEffectDescriptor> GetLayerEffects()
        {
            return LayerEffectStore.GetAll().Select(r => r.LayerEffectDescriptor).ToList();
        }
    }
}