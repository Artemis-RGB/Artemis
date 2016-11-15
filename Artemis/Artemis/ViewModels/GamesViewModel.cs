using System.Linq;
using Artemis.DAL;
using Artemis.Managers;
using Artemis.Modules.Effects.ProfilePreview;
using Artemis.Settings;
using Artemis.ViewModels.Abstract;

namespace Artemis.ViewModels
{
    public sealed class GamesViewModel : BaseViewModel
    {
        private readonly IOrderedEnumerable<GameViewModel> _vms;

        public GamesViewModel(GameViewModel[] gameViewModels, ProfileManager profileManager,
            ProfilePreviewModel profilePreviewModel)
        {
            DisplayName = "Games";

            // Currently WoW is locked behind a hidden trigger (obviously not that hidden since you're reading this)
            // It is using memory reading and lets first try to contact Blizzard
            _vms = SettingsProvider.Load<GeneralSettings>().GamestatePort == 62575
                ? gameViewModels.OrderBy(g => g.DisplayName)
                : gameViewModels.Where(g => g.DisplayName != "WoW").OrderBy(g => g.DisplayName);

            profileManager.ProfilePreviewModel = profilePreviewModel;
            profileManager.GameViewModels.AddRange(_vms);
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            Items.Clear();
            Items.AddRange(_vms);
        }
    }
}