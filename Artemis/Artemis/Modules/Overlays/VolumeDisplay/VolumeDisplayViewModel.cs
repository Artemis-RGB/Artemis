using Artemis.Managers;
using Artemis.ViewModels.Abstract;

namespace Artemis.Modules.Overlays.VolumeDisplay
{
    public class VolumeDisplayViewModel : OverlayViewModel
    {
        public VolumeDisplayViewModel(MainManager mainManager)
        {
            MainManager = mainManager;

            // Settings are loaded from file by class
            OverlaySettings = new VolumeDisplaySettings();

            // Create effect model and add it to MainManager
            OverlayModel = new VolumeDisplayModel(mainManager, (VolumeDisplaySettings) OverlaySettings);
            MainManager.EffectManager.EffectModels.Add(OverlayModel);
        }
    }
}