using System.Linq;
using Artemis.Managers;
using Artemis.Modules.Effects.ProfilePreview;
using Artemis.ViewModels.Abstract;

namespace Artemis.ViewModels
{
    public sealed class GamesViewModel : BaseViewModel
    {
        private IOrderedEnumerable<GameViewModel> _vms;

        public GamesViewModel(GameViewModel[] gameViewModels, ProfileManager profileManager,
            ProfilePreviewModel profilePreviewModel)
        {
            DisplayName = "Games";

            _vms = gameViewModels.OrderBy(g => g.DisplayName);

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