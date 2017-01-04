using System.IO;
using System.Windows.Forms;
using Artemis.Managers;
using Artemis.Modules.Abstract;
using Ninject;

namespace Artemis.Modules.Games.EurotruckSimulator2
{
    public sealed class EurotruckSimulator2ViewModel : ModuleViewModel
    {
        public EurotruckSimulator2ViewModel(MainManager mainManager,
            [Named(nameof(EurotruckSimulator2Model))] ModuleModel moduleModel,
            IKernel kernel) : base(mainManager, moduleModel, kernel)
        {
            DisplayName = "ETS 2";
        }

        public override bool UsesProfileEditor => true;
        public new EurotruckSimulator2Settings Settings { get; set; }

        public void BrowseDirectory()
        {
            var dialog = new FolderBrowserDialog {SelectedPath = Settings.GameDirectory};
            var result = dialog.ShowDialog();
            if (result != DialogResult.OK)
                return;

            Settings.GameDirectory = Path.GetDirectoryName(dialog.SelectedPath);
            NotifyOfPropertyChange(() => Settings);
            Settings.Save();

            ((EurotruckSimulator2Model) ModuleModel).PlacePlugin();
        }
    }
}