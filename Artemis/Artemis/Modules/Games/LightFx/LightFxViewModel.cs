using Artemis.Managers;
using Artemis.Modules.Abstract;
using Ninject;

namespace Artemis.Modules.Games.LightFx
{
    public sealed class LightFxViewModel : ModuleViewModel
    {
        public LightFxViewModel(MainManager mainManager, [Named(nameof(LightFxModel))] ModuleModel moduleModel,
            IKernel kernel) : base(mainManager, moduleModel, kernel)
        {
            DisplayName = "Light FX";
        }

        public override bool UsesProfileEditor => true;
    }
}