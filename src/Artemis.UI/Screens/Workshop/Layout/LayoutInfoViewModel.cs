using System;
using Artemis.Core;
using Artemis.UI.Screens.Workshop.SubmissionWizard.Models;
using Artemis.UI.Shared;
using PropertyChanged.SourceGenerator;
using RGB.NET.Core;
using KeyboardLayoutType = Artemis.Core.KeyboardLayoutType;

namespace Artemis.UI.Screens.Workshop.Layout;

public partial class LayoutInfoViewModel : ViewModelBase
{
    [Notify] private Guid _deviceProvider;
    [Notify] private string? _vendor;
    [Notify] private string? _model;
    [Notify] private KeyboardLayoutType? _physicalLayout;
    [Notify] private string? _logicalLayout;

    /// <inheritdoc />
    public LayoutInfoViewModel(ArtemisLayout layout)
    {
        DisplayKeyboardLayout = layout.RgbLayout.Type == RGBDeviceType.Keyboard;
    }

    public LayoutInfoViewModel(ArtemisLayout layout, LayoutInfo layoutInfo)
    {
        DisplayKeyboardLayout = layout.RgbLayout.Type == RGBDeviceType.Keyboard;
        
    }

    public bool DisplayKeyboardLayout { get; }
}