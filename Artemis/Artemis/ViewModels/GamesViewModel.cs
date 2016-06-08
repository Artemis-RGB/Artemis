using Artemis.Managers;
using Artemis.Modules.Effects.ProfilePreview;
using Artemis.ViewModels.Abstract;

namespace Artemis.ViewModels
{
    public sealed class GamesViewModel : BaseViewModel
    {
        private readonly GameViewModel[] _gameViewModels;

        public GamesViewModel(GameViewModel[] gameViewModels, ProfileManager profileManager,
            ProfilePreviewModel profilePreviewModel)
        {
            DisplayName = "Games";
            _gameViewModels = gameViewModels;

            profileManager.ProfilePreviewModel = profilePreviewModel;
            profileManager.GameViewModels.AddRange(_gameViewModels);
        }

        protected override void OnActivate()
        {
            base.OnActivate();

            foreach (var gameViewModel in _gameViewModels)
                ActivateItem(gameViewModel);
        }
    }
}