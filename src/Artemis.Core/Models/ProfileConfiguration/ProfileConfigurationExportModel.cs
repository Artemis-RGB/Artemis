﻿using System.IO;
using Artemis.Core.JsonConverters;
using Artemis.Storage.Entities.Profile;
using Newtonsoft.Json;

namespace Artemis.Core
{
    /// <summary>
    ///     A model that can be used to serialize a profile configuration, it's profile and it's icon
    /// </summary>
    public class ProfileConfigurationExportModel
    {
        /// <summary>
        ///     Gets or sets the storage entity of the profile configuration
        /// </summary>
        public ProfileConfigurationEntity? ProfileConfigurationEntity { get; set; }

        /// <summary>
        ///     Gets or sets the storage entity of the profile
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public ProfileEntity ProfileEntity { get; set; } = null!;

        /// <summary>
        ///     Gets or sets a stream containing the profile image
        /// </summary>
        [JsonConverter(typeof(StreamConverter))]
        public Stream? ProfileImage { get; set; }
    }
}