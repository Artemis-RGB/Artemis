using Artemis.Models;
using Caliburn.Micro;

namespace Artemis.Modules.Games.Dota2
{
    public class Dota2ViewModel : Screen
    {
        public Dota2ViewModel(MainModel mainModel)
        {
            MainModel = mainModel;
        }

        public MainModel MainModel { get; set; }

        public static string Name => "Dota 2 (NYI)";
        public string Content => "Dota 2 Content";
    }
}