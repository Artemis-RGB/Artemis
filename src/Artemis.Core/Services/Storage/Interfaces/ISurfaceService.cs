using System;
using System.Collections.ObjectModel;
using Artemis.Core.Events;
using Artemis.Core.Models.Surface;
using Artemis.Core.Services.Interfaces;

namespace Artemis.Core.Services.Storage.Interfaces
{
    public interface ISurfaceService : IArtemisService
    {
        /// <summary>
        ///     Gets the currently active surface entity, to change config use <see cref="SetActiveSurfaceConfiguration" />
        /// </summary>
        ArtemisSurface ActiveSurface { get; }

        /// <summary>
        ///     Gets a read-only list of all surface configurations
        /// </summary>
        ReadOnlyCollection<ArtemisSurface> SurfaceConfigurations { get; }

        /// <summary>
        ///     Creates a new surface entity with the supplied name
        /// </summary>
        /// <param name="name">The name for the new surface entity</param>
        /// <returns></returns>
        ArtemisSurface CreateSurfaceConfiguration(string name);

        /// <summary>
        ///     Sets the provided entity as active and applies it to the surface
        /// </summary>
        /// <param name="surface">The entity to activate and apply</param>
        void SetActiveSurfaceConfiguration(ArtemisSurface surface);

        /// <summary>
        ///     Saves the provided surface entity to permanent storage and if config is active, applies it to the surface
        /// </summary>
        /// <param name="surface">The entity to save (and apply if active)</param>
        /// <param name="includeDevices">Whether to also save devices. If false, devices changes won't be applied either</param>
        void UpdateSurfaceConfiguration(ArtemisSurface surface, bool includeDevices);

        /// <summary>
        ///     Deletes the supplied surface entity, surface entity may not be the active surface entity
        /// </summary>
        /// <param name="surface">The surface entity to delete, may not be the active surface entity</param>
        void DeleteSurfaceConfiguration(ArtemisSurface surface);

        /// <summary>
        ///     Occurs when the active device entity has been changed
        /// </summary>
        event EventHandler<SurfaceConfigurationEventArgs> ActiveSurfaceConfigurationChanged;

        /// <summary>
        ///     Occurs when a surface configuration has been updated
        /// </summary>
        event EventHandler<SurfaceConfigurationEventArgs> SurfaceConfigurationUpdated;
    }
}