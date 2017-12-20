using System.Diagnostics;
using System.Threading.Tasks;
using ReactiveUI;

namespace Artemis.UI.ViewModels
{
    public class MainViewModel : ReactiveObject, IMainViewModel
    {
        public MainViewModel(IScreen screen)
        {
            HostScreen = screen;
            OpenUrl = ReactiveCommand.CreateFromTask<string>(OpenUrlAsync);
        }

        public IScreen HostScreen { get; }
        public string UrlPathSegment => "Home";
        public ReactiveCommand OpenUrl { get; }

        private async Task OpenUrlAsync(string url)
        {
            await Task.Run(() => Process.Start(url));
        }
    }

    public interface IMainViewModel : IRoutableViewModel
    {
        ReactiveCommand OpenUrl { get; }
    }
}