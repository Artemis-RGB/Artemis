using Artemis.Managers;
using Artemis.Models;
using Artemis.ViewModels.Abstract;
using Ninject;

namespace Artemis.Modules.Games.GtaV
{
    public sealed class GtaVViewModel : GameViewModel
    {
        public GtaVViewModel(MainManager mainManager, [Named("GtaVModel")] GameModel model, IKernel kernel)
            : base(mainManager, model, kernel)
        {
            DisplayName = "GTA V";
        }
    }
}