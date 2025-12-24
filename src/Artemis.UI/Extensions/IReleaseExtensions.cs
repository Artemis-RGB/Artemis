using System;
using Artemis.Core;
using Artemis.WebClient.Workshop;

namespace Artemis.UI.Extensions;

public static class ReleaseExtensions
{
    extension(IRelease release)
    {
        /// <summary>
        /// Determines whether the release is compatible with the current version of Artemis.
        /// </summary>
        /// <returns>A value indicating whether the release is compatible with the current version of Artemis.</returns>
        public bool IsCompatible()
        {
            if (release.MinimumVersion == null || Constants.CurrentVersion == "local")
                return true;

            return Version.Parse(release.MinimumVersion) <= Version.Parse(Constants.CurrentVersion);
        }
    }
}