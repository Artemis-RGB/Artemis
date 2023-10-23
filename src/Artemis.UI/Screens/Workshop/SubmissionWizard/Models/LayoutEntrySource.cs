using System;
using System.Collections.Generic;
using Artemis.Core;
using Artemis.WebClient.Workshop;
using KeyboardLayoutType = Artemis.WebClient.Workshop.KeyboardLayoutType;

namespace Artemis.UI.Screens.Workshop.SubmissionWizard.Models;

public class LayoutEntrySource : IEntrySource
{
    public LayoutEntrySource(ArtemisLayout layout)
    {
        Layout = layout;
    }

    public ArtemisLayout Layout { get; set; }
    public List<LayoutInfo> LayoutInfo { get; } = new();
}

public class LayoutInfo
{
    public Guid DeviceProvider { get; set; }
    public RGBDeviceType DeviceType { get; set; }
    public string Model { get; set; }
    public string Vendor { get; set; }
    public string? LogicalLayout { get; set; }
    public KeyboardLayoutType? PhysicalLayout { get; set; }
}