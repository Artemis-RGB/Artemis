using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core.LayerBrushes;
using Ninject;

namespace Artemis.Core.Services
{
    internal class LayerBrushService : ILayerBrushService
    {
        private readonly IKernel _kernel;

        public LayerBrushService(IKernel kernel)
        {
            _kernel = kernel;
        }
        public LayerBrushRegistration RegisterLayerBrush(LayerBrushDescriptor descriptor)
        {
            if (descriptor == null)
                throw new ArgumentNullException(nameof(descriptor));

            descriptor.Kernel = _kernel;
            return LayerBrushStore.Add(descriptor);
        }

        public void RemoveLayerBrush(LayerBrushRegistration registration)
        {
            if (registration == null)
                throw new ArgumentNullException(nameof(registration));
            LayerBrushStore.Remove(registration);
        }

        public List<LayerBrushDescriptor> GetLayerBrushes()
        {
            return LayerBrushStore.GetAll().Select(r => r.LayerBrushDescriptor).ToList();
        }
    }
}
