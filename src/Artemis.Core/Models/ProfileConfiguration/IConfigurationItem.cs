using System;
using System.ComponentModel;

namespace Artemis.Core;

/// <summary>
/// Defines a contract for configuration items that can be added to a configuration section.
/// </summary>
public interface IConfigurationItem : INotifyPropertyChanged
{
    /// <summary>
    ///     Gets the unique ID of this configuration item, this ID remains stable between versions of a profile.
    /// </summary>
    Guid Id { get; }
}
