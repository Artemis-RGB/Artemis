using System.Windows.Forms;
using Artemis.Managers;
using Artemis.Modules.Abstract;
using Ninject;

namespace Artemis.Modules.Games.WoW
{
    public sealed class WoWViewModel : ModuleViewModel
    {
        public WoWViewModel(MainManager mainManager, [Named(nameof(WoWModel))] ModuleModel moduleModel, IKernel kernel)
            : base(mainManager, moduleModel, kernel)
        {
            DisplayName = "WoW";
        }

        public override bool UsesProfileEditor => true;

        public void PlaceAddon()
        {
            ((WoWModel) ModuleModel).PlaceAddon();
        }

        public void BrowseDirectory()
        {
            var dialog = new FolderBrowserDialog {SelectedPath = ((WoWSettings) Settings).GameDirectory};
            var result = dialog.ShowDialog();
            if (result != DialogResult.OK)
                return;

            ((WoWSettings) Settings).GameDirectory = dialog.SelectedPath;
            ((WoWModel) ModuleModel).PlaceAddon();
            Settings.Save();
            NotifyOfPropertyChange(() => Settings);
        }
    }
}