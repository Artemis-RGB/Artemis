using System;

namespace Artemis.Core.Exceptions
{
    public class ArtemisCoreException : Exception
    {
        public ArtemisCoreException()
        {
        }

        public ArtemisCoreException(string message) : base(message)
        {
        }

        public ArtemisCoreException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}