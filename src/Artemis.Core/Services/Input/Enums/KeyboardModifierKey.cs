using System;
using System.ComponentModel;

namespace Artemis.Core.Services
{
    /// <summary>
    ///     Specifies the set of modifier keys.
    /// </summary>
    [Flags]
    public enum KeyboardModifierKey
    {
        /// <summary>No modifiers are pressed.</summary>
        [Description("None")]
        None = 0,

        /// <summary>The ALT key.</summary>
        [Description("Alt")]
        Alt = 1,

        /// <summary>The CTRL key.</summary>
        [Description("Ctrl")]
        Control = 2,

        /// <summary>The SHIFT key.</summary>
        [Description("Shift")]
        Shift = 4,

        /// <summary>The Windows logo key.</summary>
        [Description("Win")]
        Windows = 8
    }
}