using System.Globalization;
using Newtonsoft.Json;

namespace Artemis.Core.Services.Core
{
    /// <summary>
    ///     Represents build information related to the currently running Artemis build
    /// </summary>
    public class BuildInfo
    {
        /// <summary>
        ///     Gets the unique ID of this build
        /// </summary>
        [JsonProperty]
        public int BuildId { get; internal set; }

        /// <summary>
        ///     Gets the build number. This contains the date and the build count for that day.
        ///     <para>Per example <c>20210108.4</c></para>
        /// </summary>
        [JsonProperty]
        public double BuildNumber { get; internal set; }

        /// <summary>
        ///     Gets the build number formatted as a string. This contains the date and the build count for that day.
        ///     <para>Per example <c>20210108.4</c></para>
        /// </summary>
        public string BuildNumberDisplay => BuildNumber.ToString(CultureInfo.InvariantCulture);

        /// <summary>
        ///     Gets the branch of the triggering repo the build was created for.
        /// </summary>
        [JsonProperty]
        public string SourceBranch { get; internal set; } = null!;

        /// <summary>
        ///     Gets the commit ID used to create this build
        /// </summary>
        [JsonProperty]
        public string SourceVersion { get; internal set; } = null!;

        /// <summary>
        /// Gets a boolean indicating whether the current build is a local build
        /// </summary>
        public bool IsLocalBuild { get; internal set; }
    }
}