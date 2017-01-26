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

        public void Ets2BrowseDirectory()
        {
            var settings = (EurotruckSimulator2Settings) Settings;
            var model = (EurotruckSimulator2Model) ModuleModel;

            var dialog = new FolderBrowserDialog {SelectedPath = settings.Ets2GameDirectory};
            var result = dialog.ShowDialog();
            if (result != DialogResult.OK)
                return;

            settings.Ets2GameDirectory = dialog.SelectedPath;
            NotifyOfPropertyChange(() => Settings);
            Settings.Save();
            model.PlaceTruckSimulatorPlugin(settings.Ets2GameDirectory, "eurotrucks2.exe");
        }

        public void AtsBrowseDirectory()
        {
            var settings = (EurotruckSimulator2Settings) Settings;
            var model = (EurotruckSimulator2Model)ModuleModel;

            var dialog = new FolderBrowserDialog {SelectedPath = settings.AtsGameDirectory};
            var result = dialog.ShowDialog();
            if (result != DialogResult.OK)
                return;

            settings.AtsGameDirectory = dialog.SelectedPath;
            NotifyOfPropertyChange(() => Settings);
            Settings.Save();
            model.PlaceTruckSimulatorPlugin(settings.AtsGameDirectory, "amtrucks.exe");
        }

        public void Ets2PlacePlugin()
        {
            var ets2Path = ((EurotruckSimulator2Settings)Settings).Ets2GameDirectory;
            ((EurotruckSimulator2Model)ModuleModel).PlaceTruckSimulatorPlugin(ets2Path, "eurotrucks2.exe");
        }

        public void AtsPlacePlugin()
        {
            var atsPath = ((EurotruckSimulator2Settings)Settings).AtsGameDirectory;
            ((EurotruckSimulator2Model)ModuleModel).PlaceTruckSimulatorPlugin(atsPath, "amtrucks.exe");
        }
    }
}