using System;

namespace Artemis.UI.Extensions;

public static class VersionExtensions
{
    /// <param name="version">The version to convert</param>
    extension(Version version)
    {
        /// <summary>
        /// Convert a Version to a long representation for easy comparison in PostgreSQL
        /// <remarks>Assumes format: major.year.dayOfYear.revision (e.g., 1.2024.0225.2)</remarks>
        /// </summary>
        /// <returns>A long value that preserves version comparison order</returns>
        public long ArtemisVersionToLong()
        {
            // Format: major.year.dayOfYear.revision
            // Convert to: majorYYYYDDDRRRR (16 digits)
            // Major: 1 digit (0-9)
            // Year: 4 digits (e.g., 2024)
            // Day: 3 digits (001-366, padded)
            // Revision: 4 digits (0000-9999, padded)

            long major = Math.Max(0, Math.Min(9, version.Major));
            long year = Math.Max(1000, Math.Min(9999, version.Minor));
            long day = Math.Max(1, Math.Min(366, version.Build));
            long revision = Math.Max(0, Math.Min(9999, version.Revision >= 0 ? version.Revision : 0));

            return major * 100000000000L +
                   year * 10000000L +
                   day * 10000L +
                   revision;
        }
        
        public static Version FromLong(long versionLong)
        {
            int major = (int)(versionLong / 100000000000L);
            int year = (int)((versionLong / 10000000L) % 10000);
            int day = (int)((versionLong / 10000L) % 1000);
            int revision = (int)(versionLong % 10000L);

            return new Version(major, year, day, revision);
        }
    }
}