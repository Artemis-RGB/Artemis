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
        public int BuildId { get; internal set; }

        /// <summary>
        ///     Gets the build number. This contains the date and the build count for that day.
        ///     <example>Per example 20210108.4</example>
        /// </summary>
        public double BuildNumber { get; internal set; }

        /// <summary>
        ///     Gets the branch of the triggering repo the build was created for.
        /// </summary>
        public string SourceBranch { get; internal set; } = null!;

        /// <summary>
        ///     Gets the commit ID used to create this build
        /// </summary>
        public string SourceVersion { get; internal set; } = null!;
    }
}