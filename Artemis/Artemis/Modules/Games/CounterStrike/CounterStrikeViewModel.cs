using System.Windows.Forms;
using Artemis.Managers;
using Artemis.Modules.Abstract;
using Ninject;

namespace Artemis.Modules.Games.CounterStrike
{
    public sealed class CounterStrikeViewModel : ModuleViewModel
    {
        public CounterStrikeViewModel(MainManager mainManager, [Named(nameof(CounterStrikeModel))] ModuleModel moduleModel, IKernel kernel)
            : base(mainManager, moduleModel, kernel)
        {
            DisplayName = "CS:GO";
        }

        public override bool UsesProfileEditor => true;

        public void PlaceConfigFile()
        {
            ((CounterStrikeModel) ModuleModel).PlaceConfigFile();
            NotifyOfPropertyChange(() => Settings);
        }

        public void BrowseDirectory()
        {
            var dialog = new FolderBrowserDialog { SelectedPath = ((CounterStrikeSettings)Settings).GameDirectory };
            var result = dialog.ShowDialog();
            if (result != DialogResult.OK)
                return;

            ((CounterStrikeSettings)Settings).GameDirectory = dialog.SelectedPath;
            ((CounterStrikeModel)ModuleModel).PlaceConfigFile();
            Settings.Save();
            NotifyOfPropertyChange(() => Settings);
        }
    }
}
