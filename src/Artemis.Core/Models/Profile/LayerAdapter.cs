using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Storage.Entities.Profile;
using Artemis.Storage.Entities.Profile.AdaptionHints;
using RGB.NET.Core;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents an adapter that adapts a layer to a certain set of devices using <see cref="IAdaptionHint" />s
    /// </summary>
    public class LayerAdapter : IStorageModel
    {
        internal LayerAdapter(Layer layer)
        {
            Layer = layer;
            AdaptionHints = new List<IAdaptionHint>();
        }

        /// <summary>
        ///     Gets the layer this adapter can adapt
        /// </summary>
        public Layer Layer { get; }

        /// <summary>
        ///     Gets or sets a list containing the adaption hints used by this adapter
        /// </summary>
        public List<IAdaptionHint> AdaptionHints { get; set; }

        /// <summary>
        ///     Modifies the layer, adapting it to the provided <paramref name="devices" />
        /// </summary>
        /// <param name="devices">The devices to adapt the layer to</param>
        public void Adapt(List<ArtemisDevice> devices)
        {
            // Use adaption hints if provided
            if (AdaptionHints.Any())
            {
                foreach (IAdaptionHint adaptionHint in AdaptionHints)
                    adaptionHint.Apply(Layer, devices);
            }
            // If there are no hints, try to find matching LEDs anyway
            else
            {
                List<ArtemisLed> availableLeds = devices.SelectMany(d => d.Leds).ToList();
                List<ArtemisLed> usedLeds = new();

                foreach (LedEntity ledEntity in Layer.LayerEntity.Leds)
                {
                    // TODO: If this is a keyboard LED and the layouts don't match, convert it before looking for it on the devices

                    LedId ledId = Enum.Parse<LedId>(ledEntity.LedName);
                    ArtemisLed? led = availableLeds.FirstOrDefault(l => l.RgbLed.Id == ledId);

                    if (led != null)
                    {
                        availableLeds.Remove(led);
                        usedLeds.Add(led);
                    }
                }

                Layer.AddLeds(usedLeds);
            }
        }

        /// <summary>
        /// Automatically determine hints for this layer
        /// </summary>
        public List<IAdaptionHint> DetermineHints(IEnumerable<ArtemisDevice> devices)
        {
            List<IAdaptionHint> newHints = new();
            // Any fully covered device will add a device adaption hint for that type
            foreach (IGrouping<ArtemisDevice, ArtemisLed> deviceLeds in Layer.Leds.GroupBy(l => l.Device))
            {
                ArtemisDevice device = deviceLeds.Key;
                // If there is already an adaption hint for this type, don't add another
                if (AdaptionHints.Any(h => h is DeviceAdaptionHint d && d.DeviceType == device.RgbDevice.DeviceInfo.DeviceType))
                    continue;
                if (DoesLayerCoverDevice(device))
                {
                    DeviceAdaptionHint hint = new() {DeviceType = device.RgbDevice.DeviceInfo.DeviceType};
                    AdaptionHints.Add(hint);
                    newHints.Add(hint);
                }
            }

            // Any fully covered category will add a category adaption hint for its category
            foreach (DeviceCategory deviceCategory in Enum.GetValues<DeviceCategory>())
            {
                if (AdaptionHints.Any(h => h is CategoryAdaptionHint c && c.Category == deviceCategory))
                    continue;

                List<ArtemisDevice> categoryDevices = devices.Where(d => d.Categories.Contains(deviceCategory)).ToList();
                if (categoryDevices.Any() && categoryDevices.All(DoesLayerCoverDevice))
                {
                    CategoryAdaptionHint hint = new() {Category = deviceCategory};
                    AdaptionHints.Add(hint);
                    newHints.Add(hint);
                }
            }

            return newHints;
        }

        private bool DoesLayerCoverDevice(ArtemisDevice device)
        {
            return device.Leds.All(l => Layer.Leds.Contains(l));
        }

        #region Implementation of IStorageModel

        /// <inheritdoc />
        public void Load()
        {
            AdaptionHints.Clear();
            // Kind of meh.
            // This leaves the adapter responsible for finding the right hint for the right entity, but it's gotta be done somewhere..
            foreach (IAdaptionHintEntity hintEntity in Layer.LayerEntity.AdaptionHints)
                switch (hintEntity)
                {
                    case DeviceAdaptionHintEntity entity:
                        AdaptionHints.Add(new DeviceAdaptionHint(entity));
                        break;
                    case CategoryAdaptionHintEntity entity:
                        AdaptionHints.Add(new CategoryAdaptionHint(entity));
                        break;
                    case KeyboardSectionAdaptionHintEntity entity:
                        AdaptionHints.Add(new KeyboardSectionAdaptionHint(entity));
                        break;
                }
        }

        /// <inheritdoc />
        public void Save()
        {
            Layer.LayerEntity.AdaptionHints.Clear();
            foreach (IAdaptionHint adaptionHint in AdaptionHints)
                Layer.LayerEntity.AdaptionHints.Add(adaptionHint.GetEntry());
        }

        #endregion
    }
}