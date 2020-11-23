using System;

namespace Artemis.Core.Services
{
    /// <summary>
    ///     Specifies the set of modifier keys.
    /// </summary>
    [Flags]
    public enum KeyboardModifierKey
    {
        /// <summary>No modifiers are pressed.</summary>
        None = 0,

        /// <summary>The ALT key.</summary>
        Alt = 1,

        /// <summary>The CTRL key.</summary>
        Control = 2,

        /// <summary>The SHIFT key.</summary>
        Shift = 4,

        /// <summary>The Windows logo key.</summary>
        Windows = 8
    }
}