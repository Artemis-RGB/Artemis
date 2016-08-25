using Artemis.Managers;
using Artemis.ViewModels.Abstract;

namespace Artemis.Modules.Overlays.VolumeDisplay
{
    public sealed class VolumeDisplayViewModel : OverlayViewModel
    {
        public VolumeDisplayViewModel(MainManager mainManager, VolumeDisplayModel model) : base(mainManager, model)
        {
            DisplayName = "Volume Display";
        }
    }
}