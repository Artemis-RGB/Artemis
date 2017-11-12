using System.Windows.Forms;
using Artemis.DAL;
using Artemis.Managers;
using Artemis.Modules.Abstract;
using Artemis.Properties;
using Ninject;

namespace Artemis.Modules.Games.UnrealTournament
{
    public sealed class UnrealTournamentViewModel : ModuleViewModel
    {
        public UnrealTournamentViewModel(MainManager mainManager,
            [Named(nameof(UnrealTournamentModel))] ModuleModel moduleModel, IKernel kernel)
            : base(mainManager, moduleModel, kernel)
        {
            DisplayName = "Unreal Tournament";
            InstallGif();
        }

        public override bool UsesProfileEditor => true;

        public void BrowseDirectory()
        {
            var dialog = new FolderBrowserDialog {SelectedPath = ((UnrealTournamentSettings) Settings).GameDirectory};
            var result = dialog.ShowDialog();
            if (result != DialogResult.OK)
                return;

            ((UnrealTournamentSettings) Settings).GameDirectory = dialog.SelectedPath;
            ((UnrealTournamentModel) ModuleModel).PlaceFiles();
            Settings.Save();
            NotifyOfPropertyChange(() => Settings);
        }

        public void PlaceFiles()
        {
            ((UnrealTournamentModel)ModuleModel).PlaceFiles();
            Settings.Save();
            NotifyOfPropertyChange(() => Settings);
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