using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core.LayerEffects;
using Ninject;

namespace Artemis.Core.Services
{
    internal class LayerEffectService : ILayerEffectService
    {
        private readonly IKernel _kernel;

        public LayerEffectService(IKernel kernel)
        {
            _kernel = kernel;
        }

        public LayerEffectRegistration RegisterLayerEffect(LayerEffectDescriptor descriptor)
        {
            if (descriptor == null)
                throw new ArgumentNullException(nameof(descriptor));

            descriptor.Kernel = _kernel;
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