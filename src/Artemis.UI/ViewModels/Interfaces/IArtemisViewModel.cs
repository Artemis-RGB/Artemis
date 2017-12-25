using ReactiveUI;

namespace Artemis.UI.ViewModels.Interfaces
{
    public interface IArtemisViewModel : IRoutableViewModel
    {
        string Title { get; }
        string Icon { get; }
    }
}