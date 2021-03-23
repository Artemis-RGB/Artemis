using System;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents SkiaSharp graphics-context related errors
    /// </summary>
    public class ArtemisGraphicsContextException : Exception
    {
        /// <inheritdoc />
        public ArtemisGraphicsContextException()
        {
        }

        /// <inheritdoc />
        public ArtemisGraphicsContextException(string message) : base(message)
        {
        }

        /// <inheritdoc />
        public ArtemisGraphicsContextException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}