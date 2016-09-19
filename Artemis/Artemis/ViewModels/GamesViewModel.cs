using System.Linq;
using Artemis.DAL;
using Artemis.Managers;
using Artemis.Modules.Effects.ProfilePreview;
using Artemis.Modules.Games.WoW;
using Artemis.Settings;
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

            var settings = SettingsProvider.Load<GeneralSettings>();
            foreach (var gameViewModel in _gameViewModels.OrderBy(g => g.DisplayName))
            {
                if (settings.GamestatePort != 62575 && gameViewModel is WoWViewModel)
                    continue;
                
                ActivateItem(gameViewModel);
            }
        }
    }
}