using System;
using RGB.NET.Layout;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a device layout decorated with extra Artemis-specific data
    /// </summary>
    public class ArtemisLayout
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="ArtemisLayout" /> class
        /// </summary>
        /// <param name="filePath">The path of the layout XML file</param>
        public ArtemisLayout(string filePath)
        {
            FilePath = filePath;
            DeviceLayout = DeviceLayout.Load(FilePath);
            IsValid = DeviceLayout != null;
        }

        /// <summary>
        ///     Gets the file path the layout was (attempted to be) loaded from
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        ///     Gets the RGB.NET device layout
        /// </summary>
        public DeviceLayout? DeviceLayout { get; }

        /// <summary>
        ///     Gets a boolean indicating whether a valid layout was loaded
        /// </summary>
        public bool IsValid { get; }

        /// <summary>
        ///     Gets the device this image is applied to
        /// </summary>
        public ArtemisDevice? Device { get; internal set; }

        /// <summary>
        ///     Gets or sets the physical layout of the device. Only applicable to keyboards
        /// </summary>
        public string? PhysicalLayout { get; set; }

        /// <summary>
        ///     Gets or sets the logical layout of the device. Only applicable to keyboards
        /// </summary>
        public string? LogicalLayout { get; set; }

        /// <summary>
        ///     Gets or sets the image of the device
        /// </summary>
        public Uri? Image { get; set; }
    }
}