using System.IO;
using System.Windows.Forms;
using Artemis.Managers;
using Artemis.Models;
using Artemis.ViewModels.Abstract;
using Ninject;

namespace Artemis.Modules.Games.EurotruckSimulator2
{
    public sealed class EurotruckSimulator2ViewModel : GameViewModel
    {
        public EurotruckSimulator2ViewModel(MainManager main, IKernel kernel,
            [Named("EurotruckSimulator2Model")] GameModel model) : base(main, model, kernel)
        {
            DisplayName = "ETS 2";
        }

        public void BrowseDirectory()
        {
            var dialog = new FolderBrowserDialog
            {
                SelectedPath = ((EurotruckSimulator2Settings) GameSettings).GameDirectory
            };
            var result = dialog.ShowDialog();
            if (result != DialogResult.OK)
                return;

            ((EurotruckSimulator2Settings) GameSettings).GameDirectory = Path.GetDirectoryName(dialog.SelectedPath);
            NotifyOfPropertyChange(() => GameSettings);

            GameSettings.Save();
            ((EurotruckSimulator2Model) GameModel).PlacePlugin();
        }
    }
}