using Artemis.Managers;
using Artemis.Modules.Abstract;
using Ninject;

namespace Artemis.Modules.Games.CounterStrike
{
    public sealed class CounterStrikeViewModel : ModuleViewModel
    {
        public CounterStrikeViewModel(MainManager mainManager,
            [Named(nameof(CounterStrikeModel))] ModuleModel moduleModel, IKernel kernel)
            : base(mainManager, moduleModel, kernel)
        {
            DisplayName = "CS:GO";
        }

        public override bool UsesProfileEditor => true;

        public void BrowseDirectory()
        {
            ((CounterStrikeModel) ModuleModel).PlaceConfigFile();
            NotifyOfPropertyChange(() => Settings);
        }
    }
}