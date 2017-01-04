using Artemis.Managers;
using Artemis.Modules.Abstract;
using Ninject;

namespace Artemis.Modules.Games.Dota2
{
    public sealed class Dota2ViewModel : ModuleViewModel
    {
        public Dota2ViewModel(MainManager mainManager, [Named(nameof(Dota2Model))] ModuleModel moduleModel,
            IKernel kernel) : base(mainManager, moduleModel, kernel)
        {
            DisplayName = "Dota 2";
        }

        public override bool UsesProfileEditor => true;

        public void BrowseDirectory()
        {
            ((Dota2Model) ModuleModel).PlaceConfigFile();
            NotifyOfPropertyChange(() => Settings);
        }
    }
}