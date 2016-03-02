using Artemis.Managers;
using Artemis.ViewModels.Abstract;

namespace Artemis.Modules.Games.Dota2
{
    public class Dota2ViewModel : GameViewModel
    {
        public Dota2ViewModel(MainManager mainManager)
        {
            MainManager = mainManager;
        }

        public static string Name => "Dota 2 (NYI)";
        public string Content => "Dota 2 Content";
    }
}