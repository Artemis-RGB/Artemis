using System;

namespace Artemis.Core.Plugins.Exceptions
{
    public class ArtemisPluginLockException : Exception
    {
        public ArtemisPluginLockException(Exception innerException) : base(CreateExceptionMessage(innerException), innerException)
        {
        }

        private static string CreateExceptionMessage(Exception innerException)
        {
            return innerException != null
                ? "Found a lock file, skipping load, see inner exception for last known exception."
                : "Found a lock file, skipping load.";
        }
    }
}