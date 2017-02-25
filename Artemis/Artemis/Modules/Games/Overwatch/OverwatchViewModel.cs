using System.Windows.Forms;
using Artemis.Managers;
using Artemis.Modules.Abstract;
using Ninject;

namespace Artemis.Modules.Games.Overwatch
{
    public sealed class OverwatchViewModel : ModuleViewModel
    {
        public OverwatchViewModel(MainManager mainManager, [Named(nameof(OverwatchModel))] ModuleModel moduleModel,
            IKernel kernel) : base(mainManager, moduleModel, kernel)
        {
            DisplayName = "Overwatch";
        }

        public override bool UsesProfileEditor => true;

        public void PlaceDll()
        {
            ((OverwatchModel)ModuleModel).PlaceDll();
        }

        public void BrowseDirectory()
        {
            var dialog = new FolderBrowserDialog {SelectedPath = ((OverwatchSettings) Settings).GameDirectory};
            var result = dialog.ShowDialog();
            if (result != DialogResult.OK)
                return;

            ((OverwatchSettings) Settings).GameDirectory = dialog.SelectedPath;
            ((OverwatchModel) ModuleModel).PlaceDll();
            Settings.Save();
            NotifyOfPropertyChange(() => Settings);
        }
    }
}