using System.Diagnostics;
using System.Threading.Tasks;
using Artemis.UI.ViewModels.Interfaces;
using ReactiveUI;

namespace Artemis.UI.ViewModels
{
    public class MainViewModel : ReactiveObject, IMainViewModel
    {
        public MainViewModel(IScreen screen, ISidebarViewModel sidebarViewModel)
        {
            HostScreen = screen;
            OpenUrl = ReactiveCommand.CreateFromTask<string>(OpenUrlAsync);

            // Add this view as a menu item
            sidebarViewModel.MenuItems.Add(this);
        }

        public IScreen HostScreen { get; }
        public string UrlPathSegment => Title.ToLower();
        public string Title => "Home";
        public string Icon => "Home";

        public ReactiveCommand OpenUrl { get; }

        private async Task OpenUrlAsync(string url)
        {
            await Task.Run(() => Process.Start(url));
        }
    }

    public interface IMainViewModel : IArtemisViewModel
    {
        ReactiveCommand OpenUrl { get; }
    }
}