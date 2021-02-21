// ReSharper disable InconsistentNaming

namespace Artemis.Core
{
    // Copied from RGB.NET to avoid needing to reference RGB.NET
    /// <summary>
    ///     Represents a physical layout type for a keyboard
    /// </summary>
    public enum KeyboardLayoutType
    {
        /// <summary>
        ///     An unknown layout type
        /// </summary>
        Unknown = 0,

        /// <summary>
        ///     The ANSI layout type, often used in the US (uses a short enter)
        /// </summary>
        ANSI = 1,

        /// <summary>
        ///     The ISO layout type, often used in the EU (uses a tall enter)
        /// </summary>
        ISO = 2,

        /// <summary>
        ///     The JIS layout type, often used in Japan (based on ISO)
        /// </summary>
        JIS = 3,

        /// <summary>
        ///     The ABNT layout type, often used in Brazil/Portugal (based on ISO)
        /// </summary>
        ABNT = 4,

        /// <summary>
        ///     The KS layout type, often used in South Korea
        /// </summary>
        KS = 5
    }
}