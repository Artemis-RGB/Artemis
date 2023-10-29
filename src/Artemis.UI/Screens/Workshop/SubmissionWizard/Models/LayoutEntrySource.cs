using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Artemis.Core;
using Artemis.UI.Screens.Workshop.Layout;

namespace Artemis.UI.Screens.Workshop.SubmissionWizard.Models;

public class LayoutEntrySource : IEntrySource
{
    public LayoutEntrySource(ArtemisLayout layout)
    {
        Layout = layout;
    }

    public ArtemisLayout Layout { get; set; }
    public ObservableCollection<LayoutInfoViewModel> LayoutInfo { get; } = new();
    public KeyboardLayoutType PhysicalLayout { get; set; }

    private List<LayoutCustomLedDataLogicalLayout> GetLogicalLayouts()
    {
        return Layout.Leds
            .Where(l => l.LayoutCustomLedData.LogicalLayouts != null)
            .SelectMany(l => l.LayoutCustomLedData.LogicalLayouts!)
            .Where(l => !string.IsNullOrWhiteSpace(l.Name))
            .DistinctBy(l => l.Name)
            .ToList();
    }
}