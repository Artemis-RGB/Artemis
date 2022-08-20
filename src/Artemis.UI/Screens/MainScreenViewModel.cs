using Artemis.UI.Shared;
using ReactiveUI;

namespace Artemis.UI.Screens
{
    public abstract class MainScreenViewModel : ActivatableViewModelBase, IRoutableViewModel
    {
        protected MainScreenViewModel(IScreen hostScreen, string urlPathSegment)
        {
            HostScreen = hostScreen;
            UrlPathSegment = urlPathSegment;
        }

        /// <inheritdoc />
        public string UrlPathSegment { get; }

        /// <inheritdoc />
        public IScreen HostScreen { get; }

        public ViewModelBase? TitleBarViewModel { get; protected set; }
    }
}