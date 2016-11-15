using System.Windows.Forms;
using Artemis.Managers;
using Artemis.Models;
using Artemis.ViewModels.Abstract;
using Ninject;

namespace Artemis.Modules.Games.Overwatch
{
    public sealed class OverwatchViewModel : GameViewModel
    {
        public OverwatchViewModel(MainManager main, IKernel kernel, [Named("OverwatchModel")] GameModel model)
            : base(main, model, kernel)
        {
            DisplayName = "Overwatch";
        }

        public void BrowseDirectory()
        {
            var dialog = new FolderBrowserDialog {SelectedPath = ((OverwatchSettings) GameSettings).GameDirectory};
            var result = dialog.ShowDialog();
            if (result != DialogResult.OK)
                return;

            ((OverwatchSettings) GameSettings).GameDirectory = dialog.SelectedPath;
            GameSettings.Save();
            ((OverwatchModel) GameModel).PlaceDll();
            NotifyOfPropertyChange(() => GameSettings);
        }
    }
}