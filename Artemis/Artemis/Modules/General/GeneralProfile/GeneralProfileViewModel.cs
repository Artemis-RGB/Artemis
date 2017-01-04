using Artemis.Managers;
using Artemis.Modules.Abstract;
using Ninject;

namespace Artemis.Modules.General.GeneralProfile
{
    public sealed class GeneralProfileViewModel : ModuleViewModel
    {
        public GeneralProfileViewModel(MainManager mainManager, [Named(nameof(GeneralProfileModel))] ModuleModel model,
            IKernel kernel) : base(mainManager, model, kernel)
        {
            DisplayName = "General profile";
        }

        public override bool UsesProfileEditor => true;
    }
}