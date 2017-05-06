using Artemis.DAL;
using Artemis.Managers;
using Artemis.Modules.Abstract;
using Artemis.Settings;
using Artemis.Utilities;
using Ninject;

namespace Artemis.Modules.Games.Terraria
{
    public sealed class TerrariaViewModel : ModuleViewModel
    {
        private string _versionText;


        public TerrariaViewModel(MainManager mainManager, [Named(nameof(TerrariaModel))] ModuleModel moduleModel, IKernel kernel) : base(mainManager, moduleModel, kernel)
        {
            DisplayName = "Terraria";
            SetVersionText();
        }

        public override bool UsesProfileEditor => true;

        public string VersionText
        {
            get { return _versionText; }
            set
            {
                if (value == _versionText) return;
                _versionText = value;
                NotifyOfPropertyChange(() => VersionText);
            }
        }

        private void SetVersionText()
        {
            if (!SettingsProvider.Load<GeneralSettings>().EnablePointersUpdate)
            {
                VersionText = "You disabled pointer updates, this could result in the Terraria module not working after a game update.";
                return;
            }

            Updater.GetPointers();
            var version = SettingsProvider.Load<OffsetSettings>().Terraria?.GameVersion;
            VersionText = $"Requires patch {version}. When a new patch is released Artemis downloads new pointers for the latest version (unless disabled in settings).";
        }
    }
}