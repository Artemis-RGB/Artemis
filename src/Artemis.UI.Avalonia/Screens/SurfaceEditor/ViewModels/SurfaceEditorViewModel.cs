using System.Collections.ObjectModel;
using Artemis.Core;
using Artemis.Core.Services;
using ReactiveUI;

namespace Artemis.UI.Avalonia.Screens.SurfaceEditor.ViewModels
{
    public class SurfaceEditorViewModel : MainScreenViewModel
    {
        public SurfaceEditorViewModel(IScreen hostScreen, IRgbService rgbService) : base(hostScreen, "surface-editor")
        {
            DisplayName = "Surface Editor";
            Devices = new ObservableCollection<ArtemisDevice>(rgbService.Devices);
        }

        public ObservableCollection<ArtemisDevice> Devices { get; }
    }
}