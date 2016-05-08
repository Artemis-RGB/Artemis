using Artemis.ViewModels.Abstract;

namespace Artemis.ViewModels
{
    public sealed class GamesViewModel : BaseViewModel
    {
        private readonly GameViewModel[] _gameViewModels;

        public GamesViewModel(GameViewModel[] gameViewModels)
        {
            DisplayName = "Games";
            _gameViewModels = gameViewModels;
        }

        protected override void OnActivate()
        {
            base.OnActivate();

            foreach (var gameViewModel in _gameViewModels)
                ActivateItem(gameViewModel);
        }
    }
}