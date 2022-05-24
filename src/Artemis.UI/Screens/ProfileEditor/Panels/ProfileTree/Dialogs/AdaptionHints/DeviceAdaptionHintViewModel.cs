using Artemis.Core;

namespace Artemis.UI.Screens.ProfileEditor.ProfileTree.Dialogs.AdaptionHints;

public class DeviceAdaptionHintViewModel : AdaptionHintViewModelBase
{
    public DeviceAdaptionHintViewModel(DeviceAdaptionHint adaptionHint) : base(adaptionHint)
    {
        DeviceAdaptionHint = adaptionHint;
    }

    public DeviceAdaptionHint DeviceAdaptionHint { get; }
}