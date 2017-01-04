using Artemis.Managers;
using Artemis.Modules.Abstract;
using Ninject;

namespace Artemis.Modules.Games.GtaV
{
    public sealed class GtaVViewModel : ModuleViewModel
    {
        public GtaVViewModel(MainManager mainManager, [Named(nameof(GtaVModel))] ModuleModel moduleModel, IKernel kernel)
            : base(mainManager, moduleModel, kernel)
        {
            DisplayName = "GTA V";
        }

        public override bool UsesProfileEditor => true;
    }
}