using System;

namespace Artemis.UI.Avalonia.Shared.Exceptions
{
    /// <summary>
    ///     Represents errors that occur within the Artemis Shared UI library
    /// </summary>
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