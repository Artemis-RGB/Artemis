using System.Collections.ObjectModel;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Services;
using Artemis.UI.Shared;

namespace Artemis.UI.Screens.ProfileEditor.VisualEditor
{
    public class VisualEditorViewModel : ActivatableViewModelBase
    {
        public VisualEditorViewModel(IProfileEditorService profileEditorService, IRgbService rgbService)
        {
            Devices = new ObservableCollection<ArtemisDevice>(rgbService.EnabledDevices);
        }

        public ObservableCollection<ArtemisDevice> Devices { get; }
    }
}