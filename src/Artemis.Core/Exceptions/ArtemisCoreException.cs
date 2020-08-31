using System;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents errors that occur withing the Artemis Core
    /// </summary>
    public class ArtemisCoreException : Exception
    {
        internal ArtemisCoreException(string message) : base(message)
        {
        }

        internal ArtemisCoreException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}