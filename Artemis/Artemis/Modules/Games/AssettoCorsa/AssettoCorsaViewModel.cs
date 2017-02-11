using Artemis.Managers;
using Artemis.Modules.Abstract;
using Ninject;

namespace Artemis.Modules.Games.AssettoCorsa
{
    public sealed class AssettoCorsaViewModel : ModuleViewModel
    {
        public AssettoCorsaViewModel(MainManager mainManager, [Named(nameof(AssettoCorsaModel))] ModuleModel moduleModel,
            IKernel kernel) : base(mainManager, moduleModel, kernel)
        {
            DisplayName = "Assetto Corsa";
        }

        public override bool UsesProfileEditor => true;
    }
}