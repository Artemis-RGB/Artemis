using System;

namespace Artemis.UI.Shared
{
    public class ArtemisSharedUIException : Exception
    {
        internal ArtemisSharedUIException()
        {
        }

        internal ArtemisSharedUIException(string message) : base(message)
        {
        }

        internal ArtemisSharedUIException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}