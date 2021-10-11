using ReactiveUI;

namespace Artemis.UI.Avalonia.Screens.Workshop.ViewModels
{
    public class WorkshopViewModel : MainScreenViewModel
    {
        public WorkshopViewModel(IScreen hostScreen) : base(hostScreen, "workshop")
        {
            DisplayName = "Workshop";
        }
    }
}