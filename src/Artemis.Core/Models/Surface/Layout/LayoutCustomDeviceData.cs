using System.Xml.Serialization;

#pragma warning disable 1591

namespace Artemis.Core;

/// <summary>
///     Represents extra Artemis-specific information stored in RGB.NET layouts
/// </summary>
[XmlRoot("CustomData")]
public class LayoutCustomDeviceData
{
    [XmlElement("DeviceImage")]
    public string? DeviceImage { get; set; }
}