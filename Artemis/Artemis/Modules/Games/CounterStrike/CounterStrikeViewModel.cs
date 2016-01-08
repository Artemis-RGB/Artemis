using Artemis.Models;
using Caliburn.Micro;

namespace Artemis.Modules.Games.CounterStrike
{
    public class CounterStrikeViewModel : Screen
    {
        public CounterStrikeViewModel(MainModel mainModel)
        {
            MainModel = mainModel;

            // Settings are loaded from file by class
            //CounterStrikeSettings = new CounterStrikeSettings();

            // Create effect model and add it to MainModel
            CounterStrikeModel = new CounterStrikeModel(MainModel);
            MainModel.EffectModels.Add(CounterStrikeModel);
        }

        public CounterStrikeModel CounterStrikeModel { get; set; }

        public MainModel MainModel { get; set; }

        public static string Name => "CS:GO (NYI)";
        public string Content => "Counter-Strike: GO Content";
    }
}