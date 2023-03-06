using System;

namespace Artemis.Core;

/// <summary>
///     An exception thrown when a plugin lock file error occurs
/// </summary>
public class ArtemisPluginLockException : Exception
{
    internal ArtemisPluginLockException(Exception? innerException) : base(CreateExceptionMessage(innerException), innerException)
    {
    }

    private static string CreateExceptionMessage(Exception? innerException)
    {
        return innerException != null
            ? "Found a lock file, skipping automatic load, see inner exception for last known exception. Please manually re-enable the plugin."
            : "Found a lock file, skipping automatic load. Please manually re-enable the plugin.";
    }
}