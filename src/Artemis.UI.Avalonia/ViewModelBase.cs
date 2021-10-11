using ReactiveUI;

namespace Artemis.UI.Avalonia
{
    public abstract class ViewModelBase : ReactiveObject
    {
        private string? _displayName;

        public string? DisplayName
        {
            get => _displayName;
            set => this.RaiseAndSetIfChanged(ref _displayName, value);
        }
    }

    public abstract class ActivatableViewModelBase : ViewModelBase, IActivatableViewModel
    {
        public ViewModelActivator Activator { get; } = new();
    }
}