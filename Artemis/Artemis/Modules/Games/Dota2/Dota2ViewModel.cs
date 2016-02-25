using Artemis.Managers;
using Caliburn.Micro;

namespace Artemis.Modules.Games.Dota2
{
    public class Dota2ViewModel : Screen
    {
        public Dota2ViewModel(MainManager mainManager)
        {
            MainManager = mainManager;
        }

        public MainManager MainManager { get; set; }

        public static string Name => "Dota 2 (NYI)";
        public string Content => "Dota 2 Content";
    }
}