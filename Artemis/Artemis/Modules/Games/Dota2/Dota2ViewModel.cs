using System.Windows.Forms;
using Artemis.Managers;
using Artemis.Modules.Abstract;
using Ninject;

namespace Artemis.Modules.Games.Dota2
{
    public sealed class Dota2ViewModel : ModuleViewModel
    {
        public Dota2ViewModel(MainManager mainManager, [Named(nameof(Dota2Model))] ModuleModel moduleModel, IKernel kernel) 
            : base(mainManager, moduleModel, kernel)
        {
            DisplayName = "Dota 2";
        }

        public override bool UsesProfileEditor => true;

        public void PlaceConfigFile()
        {
            ((Dota2Model) ModuleModel).PlaceConfigFile();
            NotifyOfPropertyChange(() => Settings);
        }

        public void BrowseDirectory()
        {
            var dialog = new FolderBrowserDialog { SelectedPath = ((Dota2Settings)Settings).GameDirectory };
            var result = dialog.ShowDialog();
            if (result != DialogResult.OK)
                return;

            ((Dota2Settings)Settings).GameDirectory = dialog.SelectedPath;
            ((Dota2Model)ModuleModel).PlaceConfigFile();
            Settings.Save();
            NotifyOfPropertyChange(() => Settings);
        }
    }
}
