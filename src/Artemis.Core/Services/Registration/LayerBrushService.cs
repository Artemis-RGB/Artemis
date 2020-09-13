using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core.LayerBrushes;

namespace Artemis.Core.Services
{
    internal class LayerBrushService : ILayerBrushService
    {
        private readonly ISettingsService _settingsService;

        public LayerBrushService(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        public LayerBrushRegistration RegisterLayerBrush(LayerBrushDescriptor descriptor)
        {
            if (descriptor == null)
                throw new ArgumentNullException(nameof(descriptor));

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

        public LayerBrushDescriptor GetDefaultLayerBrush()
        {
            var defaultReference = _settingsService.GetSetting("ProfileEditor.DefaultLayerBrushDescriptor", new LayerBrushReference
            {
                BrushPluginGuid = Guid.Parse("92a9d6ba-6f7a-4937-94d5-c1d715b4141a"),
                BrushType = "ColorBrush"
            });

            return LayerBrushStore.Get(defaultReference.Value.BrushPluginGuid, defaultReference.Value.BrushType)?.LayerBrushDescriptor;
        }
    }
}