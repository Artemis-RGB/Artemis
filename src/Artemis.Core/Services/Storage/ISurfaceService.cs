using System;
using System.Collections.ObjectModel;
using Artemis.Core.Events;
using Artemis.Core.Models.Surface;
using Artemis.Core.Services.Interfaces;

namespace Artemis.Core.Services.Storage
{
    public interface ISurfaceService : IArtemisService
    {
        /// <summary>
        ///     Gets the currently active surface configuration, to change config use <see cref="SetActiveSurfaceConfiguration" />
        /// </summary>
        SurfaceConfiguration ActiveSurfaceConfiguration { get; }

        /// <summary>
        ///     Gets a read-only list of all surface configurations
        /// </summary>
        ReadOnlyCollection<SurfaceConfiguration> SurfaceConfigurations { get; }

        /// <summary>
        ///     Creates a new surface configuration with the supplied name
        /// </summary>
        /// <param name="name">The name for the new surface configuration</param>
        /// <returns></returns>
        SurfaceConfiguration CreateSurfaceConfiguration(string name);

        /// <summary>
        ///     Sets the provided configuration as active and applies it to the surface
        /// </summary>
        /// <param name="surfaceConfiguration">The configuration to activate and apply</param>
        void SetActiveSurfaceConfiguration(SurfaceConfiguration surfaceConfiguration);

        /// <summary>
        ///     Saves the provided surface configuration to permanent storage and if config is active, applies it to the surface
        /// </summary>
        /// <param name="surfaceConfiguration">The configuration to save (and apply if active)</param>
        /// <param name="includeDevices">Whether to also save devices. If false, devices changes won't be applied either</param>
        void UpdateSurfaceConfiguration(SurfaceConfiguration surfaceConfiguration, bool includeDevices);

        /// <summary>
        ///     Deletes the supplied surface configuration, surface configuration may not be the active surface configuration
        /// </summary>
        /// <param name="surfaceConfiguration">The surface configuration to delete, may not be the active surface configuration</param>
        void DeleteSurfaceConfiguration(SurfaceConfiguration surfaceConfiguration);

        /// <summary>
        ///     Occurs when the active device configuration has been changed
        /// </summary>
        event EventHandler<SurfaceConfigurationEventArgs> ActiveSurfaceConfigurationChanged;
    }
}