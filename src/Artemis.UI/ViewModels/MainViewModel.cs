using System.Diagnostics;
using System.Threading.Tasks;
using ReactiveUI;

namespace Artemis.UI.ViewModels
{
    public class MainViewModel : ReactiveObject
    {
        public ReactiveCommand OpenUrl { get; }

        public MainViewModel()
        {
            OpenUrl = ReactiveCommand.CreateFromTask<string>(OpenUrlAsync);
        }

        private async Task OpenUrlAsync(string url)
        {
            await Task.Run(() => Process.Start(url));
        }
    }
}
