using System;
using System.IO;
using System.Text.Json.Serialization;
using Artemis.Core.JsonConverters;
using Artemis.Storage.Entities.Profile;

namespace Artemis.Core;

/// <summary>
///     A model that can be used to serialize a profile configuration, it's profile and it's icon
/// </summary>
public class ProfileConfigurationExportModel : IDisposable
{
    /// <summary>
    ///     Gets or sets the storage entity of the profile configuration
    /// </summary>
    public ProfileConfigurationEntity? ProfileConfigurationEntity { get; set; }

    /// <summary>
    ///     Gets or sets the storage entity of the profile
    /// </summary>
    [JsonRequired]
    public ProfileEntity ProfileEntity { get; set; } = null!;

    /// <summary>
    ///     Gets or sets a stream containing the profile image
    /// </summary>
    [JsonConverter(typeof(StreamConverter))]
    public Stream? ProfileImage { get; set; }

    /// <inheritdoc />
    public void Dispose()
    {
        ProfileImage?.Dispose();
    }
}