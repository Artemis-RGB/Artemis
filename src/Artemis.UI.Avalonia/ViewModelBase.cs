using ReactiveUI;

namespace Artemis.UI.Avalonia
{
    public class ViewModelBase : ReactiveObject
    {
        private string? _displayName;

        public string? DisplayName
        {
            get => _displayName;
            set => this.RaiseAndSetIfChanged(ref _displayName, value);
        }
    }
}
