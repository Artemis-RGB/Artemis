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
            DisplayName = "Truck Simulator";
        }

        public override bool UsesProfileEditor => true;

        public void PlacePlugins()
        {
            ((EurotruckSimulator2Model) ModuleModel).PlacePlugins();
        }

        public void Ets2BrowseDirectory()
        {
            var settings = (EurotruckSimulator2Settings) Settings;
            var dialog = new FolderBrowserDialog {SelectedPath = settings.Ets2GameDirectory};
            var result = dialog.ShowDialog();
            if (result != DialogResult.OK)
                return;

            settings.Ets2GameDirectory = Path.GetDirectoryName(dialog.SelectedPath);
            NotifyOfPropertyChange(() => Settings);
            Settings.Save();

            ((EurotruckSimulator2Model) ModuleModel).PlacePlugins();
        }

        public void AtsBrowseDirectory()
        {
            var settings = (EurotruckSimulator2Settings) Settings;
            var dialog = new FolderBrowserDialog {SelectedPath = settings.AtsGameDirectory};
            var result = dialog.ShowDialog();
            if (result != DialogResult.OK)
                return;

            settings.AtsGameDirectory = Path.GetDirectoryName(dialog.SelectedPath);
            NotifyOfPropertyChange(() => Settings);
            Settings.Save();

            ((EurotruckSimulator2Model) ModuleModel).PlacePlugins();
        }
    }
}