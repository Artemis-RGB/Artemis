using System;

namespace Artemis.Core.Services;

/// <summary>
///     A service that initializes the Core and manages the render loop
/// </summary>
public interface ICoreService : IArtemisService
{
    /// <summary>
    ///     Gets whether the or not the core has been initialized
    /// </summary>
    bool IsInitialized { get; }

    /// <summary>
    ///     Gets a boolean indicating whether Artemis is running in an elevated environment (admin permissions)
    /// </summary>
    bool IsElevated { get; set; }

    /// <summary>
    ///     Initializes the core, only call once
    /// </summary>
    void Initialize();

    /// <summary>
    ///     Occurs the core has finished initializing
    /// </summary>
    event EventHandler Initialized;
}