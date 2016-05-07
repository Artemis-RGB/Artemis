using Artemis.Managers;
using Artemis.ViewModels.Abstract;

namespace Artemis.Modules.Overlays.VolumeDisplay
{
    public sealed class VolumeDisplayViewModel : OverlayViewModel
    {
        public VolumeDisplayViewModel(MainManager mainManager, KeyboardManager keyboardManager,
            EffectManager effectManager)
            : base(mainManager, effectManager)
        {
            DisplayName = "Volume Display";

            // Settings are loaded from file by class
            OverlaySettings = new VolumeDisplaySettings();

            // Create effect model and add it to MainManager
            OverlayModel = new VolumeDisplayModel(mainManager, keyboardManager, (VolumeDisplaySettings) OverlaySettings);
            EffectManager.EffectModels.Add(OverlayModel);
        }
    }
}