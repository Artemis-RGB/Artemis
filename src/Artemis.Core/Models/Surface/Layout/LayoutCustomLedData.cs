using System.Collections.Generic;
using System.Xml.Serialization;
#pragma warning disable 1591

namespace Artemis.Core
{
    /// <summary>
    ///     Represents extra Artemis-specific information stored in RGB.NET layouts
    /// </summary>
    [XmlRoot("CustomData")]
    public class LayoutCustomLedData
    {
        [XmlArray("LogicalLayouts")]
        public List<LayoutCustomLedDataLogicalLayout>? LogicalLayouts { get; set; }
    }

    /// <summary>
    ///     Represents extra Artemis-specific information stored in RGB.NET layouts
    /// </summary>
    [XmlType("LogicalLayout")]
    public class LayoutCustomLedDataLogicalLayout
    {
        [XmlAttribute("Name")]
        public string? Name { get; set; }

        [XmlAttribute("Image")]
        public string? Image { get; set; }
    }
}