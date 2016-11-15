using System.Windows.Forms;
using Artemis.DAL;
using Artemis.Managers;
using Artemis.Models;
using Artemis.Properties;
using Artemis.ViewModels.Abstract;
using Ninject;

namespace Artemis.Modules.Games.UnrealTournament
{
    public sealed class UnrealTournamentViewModel : GameViewModel
    {
        public UnrealTournamentViewModel(MainManager main, IKernel kernel,
            [Named("UnrealTournamentModel")] GameModel model) : base(main, model, kernel)
        {
            DisplayName = "Unreal Tournament";
            InstallGif();
        }

        public UnrealTournamentModel UnrealTournamentModel { get; set; }


        public void BrowseDirectory()
        {
            var dialog = new FolderBrowserDialog
            {
                SelectedPath = ((UnrealTournamentSettings) GameSettings).GameDirectory
            };
            var result = dialog.ShowDialog();
            if (result != DialogResult.OK)
                return;

            ((UnrealTournamentSettings) GameSettings).GameDirectory = dialog.SelectedPath;
            GameSettings.Save();
            ((UnrealTournamentModel) GameModel).PlaceFiles();
            NotifyOfPropertyChange(() => GameSettings);
        }

        // Installing GIF on editor open to make sure the proper profiles are loaded
        private void InstallGif()
        {
            var gif = Resources.redeemer;
            if (gif == null)
                return;

            ProfileProvider.InsertGif("UnrealTournament", "Default", "Redeemer GIF", gif, "redeemer");
        }
    }
}