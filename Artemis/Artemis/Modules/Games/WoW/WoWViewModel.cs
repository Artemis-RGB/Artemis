using Artemis.Managers;
using Artemis.Modules.Abstract;
using Ninject;

namespace Artemis.Modules.Games.WoW
{
    public sealed class WoWViewModel : ModuleViewModel
    {
        public WoWViewModel(MainManager mainManager, [Named(nameof(WoWModel))] ModuleModel moduleModel, IKernel kernel)
            : base(mainManager, moduleModel, kernel)
        {
            DisplayName = "WoW";
        }

        public override bool UsesProfileEditor => true;
    }
}
