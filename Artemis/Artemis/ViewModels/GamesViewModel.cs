using Artemis.ViewModels.Abstract;

namespace Artemis.ViewModels
{
    public sealed class GamesViewModel : BaseViewModel
    {
        public GamesViewModel()
        {
            DisplayName = "Games";
        }

        protected override void OnActivate()
        {
            base.OnActivate();
        }
    }
}