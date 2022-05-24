using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private readonly List<IAdaptionHint> _adaptionHints;

        internal LayerAdapter(Layer layer)
        {
            _adaptionHints = new List<IAdaptionHint>();
            Layer = layer;
            AdaptionHints = new ReadOnlyCollection<IAdaptionHint>(_adaptionHints);
        }

        /// <summary>
        ///     Gets the layer this adapter can adapt
        /// </summary>
        public Layer Layer { get; }

        /// <summary>
        ///     Gets or sets a list containing the adaption hints used by this adapter
        /// </summary>
        public ReadOnlyCollection<IAdaptionHint> AdaptionHints { get; set; }

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
            if (devices.All(DoesLayerCoverDevice))
            {
                DeviceAdaptionHint hint = new() {DeviceType = RGBDeviceType.All};
                Add(hint);
                newHints.Add(hint);
            }
            else
            {
                // Any fully covered device will add a device adaption hint for that type
                foreach (IGrouping<ArtemisDevice, ArtemisLed> deviceLeds in Layer.Leds.GroupBy(l => l.Device))
                {
                    ArtemisDevice device = deviceLeds.Key;
                    // If there is already an adaption hint for this type, don't add another
                    if (AdaptionHints.Any(h => h is DeviceAdaptionHint d && d.DeviceType == device.DeviceType))
                        continue;
                    if (DoesLayerCoverDevice(device))
                    {
                        DeviceAdaptionHint hint = new() {DeviceType = device.DeviceType};
                        Add(hint);
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
                        Add(hint);
                        newHints.Add(hint);
                    }
                }
            }

            return newHints;
        }

        private bool DoesLayerCoverDevice(ArtemisDevice device)
        {
            return device.Leds.All(l => Layer.Leds.Contains(l));
        }

        /// <summary>
        /// Adds an adaption hint to the adapter.
        /// </summary>
        /// <param name="adaptionHint">The adaption hint to add.</param>
        public void Add(IAdaptionHint adaptionHint)
        {
            if (_adaptionHints.Contains(adaptionHint))
                return;
            
            _adaptionHints.Add(adaptionHint);
            AdapterHintAdded?.Invoke(this, new LayerAdapterHintEventArgs(adaptionHint));
        }

        /// <summary>
        /// Removes the first occurrence of a specific adaption hint from the adapter.
        /// </summary>
        /// <param name="adaptionHint">The adaption hint to remove.</param>
        public void Remove(IAdaptionHint adaptionHint)
        {
            if (_adaptionHints.Remove(adaptionHint))
                AdapterHintRemoved?.Invoke(this, new LayerAdapterHintEventArgs(adaptionHint));
        }

        /// <summary>
        /// Removes all adaption hints from the adapter.
        /// </summary>
        public void Clear()
        {
            while (_adaptionHints.Any())
                Remove(_adaptionHints.First());
        }

        #region Implementation of IStorageModel

        /// <inheritdoc />
        public void Load()
        {
            _adaptionHints.Clear();
            // Kind of meh.
            // This leaves the adapter responsible for finding the right hint for the right entity, but it's gotta be done somewhere..
            foreach (IAdaptionHintEntity hintEntity in Layer.LayerEntity.AdaptionHints)
                switch (hintEntity)
                {
                    case DeviceAdaptionHintEntity entity:
                        Add(new DeviceAdaptionHint(entity));
                        break;
                    case CategoryAdaptionHintEntity entity:
                        Add(new CategoryAdaptionHint(entity));
                        break;
                    case KeyboardSectionAdaptionHintEntity entity:
                        Add(new KeyboardSectionAdaptionHint(entity));
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

        /// <summary>
        /// Occurs whenever a new adapter hint is added to the adapter.
        /// </summary>
        public event EventHandler<LayerAdapterHintEventArgs>? AdapterHintAdded;

        /// <summary>
        /// Occurs whenever an adapter hint is removed from the adapter.
        /// </summary>
        public event EventHandler<LayerAdapterHintEventArgs>? AdapterHintRemoved;
    }
}