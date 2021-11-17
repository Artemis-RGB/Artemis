using System;

namespace Artemis.UI.Exceptions
{
    public class ArtemisUIException : Exception
    {
        public ArtemisUIException()
        {
        }

        public ArtemisUIException(string message) : base(message)
        {
        }

        public ArtemisUIException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}