using System;
using System.Linq;
using Artemis.Core.Services;
using Artemis.Storage.Entities.Profile;
using RGB.NET.Core;

namespace Artemis.Core
{
    internal class KeyboardLayoutAdapter
    {
        public void Adapt(Layer layer, IRgbService rgbService)
        {
            foreach (ArtemisDevice keyboard in rgbService.EnabledDevices.Where(d => d.RgbDevice.DeviceInfo.DeviceType == RGBDeviceType.Keyboard))
            {
                // Iterate each LED on the entity, only taking those with a physical layout (meaning they belong to a keyboard)
                foreach (LedEntity layerEntityLed in layer.LayerEntity.Leds.Where(l => l.PhysicalLayout != null))
                {
                    if (!Enum.TryParse(layerEntityLed.LedName, out LedId ledId))
                        continue;

                    KeyboardLayoutType sourceLayout = (KeyboardLayoutType) layerEntityLed.PhysicalLayout!;
                    // If there is a base-LED difference, change the LED ID
                }
            }
        }
    }
}