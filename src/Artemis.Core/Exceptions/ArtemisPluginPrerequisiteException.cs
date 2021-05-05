using System;

namespace Artemis.Core
{
    /// <summary>
    ///     An exception thrown when a plugin prerequisite-related error occurs
    /// </summary>
    public class ArtemisPluginPrerequisiteException : Exception
    {
        internal ArtemisPluginPrerequisiteException(IPrerequisitesSubject subject)
        {
            Subject = subject;
        }

        internal ArtemisPluginPrerequisiteException(IPrerequisitesSubject subject, string message) : base(message)
        {
            Subject = subject;
        }

        internal ArtemisPluginPrerequisiteException(IPrerequisitesSubject subject, string message, Exception inner) : base(message, inner)
        {
            Subject = subject;
        }

        /// <summary>
        ///     Gets the subject the error is related to
        /// </summary>
        public IPrerequisitesSubject Subject { get; }
    }
}