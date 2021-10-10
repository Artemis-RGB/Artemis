using ReactiveUI;

namespace Artemis.UI.Avalonia.Screens
{
    public abstract class MainScreenViewModel : ViewModelBase, IRoutableViewModel
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
    }
}