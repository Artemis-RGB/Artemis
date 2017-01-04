using Artemis.Managers;
using Artemis.Modules.Abstract;
using Ninject;

namespace Artemis.Modules.Overlays.OverlayProfile
{
    public sealed class OverlayProfileViewModel : ModuleViewModel
    {
        public OverlayProfileViewModel(MainManager mainManager, [Named(nameof(OverlayProfileModel))] ModuleModel model,
            IKernel kernel) : base(mainManager, model, kernel)
        {
            DisplayName = "Overlay profile";
        }

        public override bool UsesProfileEditor => true;
    }
}