using ReactiveUI;

namespace Artemis.UI.Avalonia.Screens.Workshop.ViewModels
{
    public class WorkshopViewModel : MainScreenViewModel
    {
        public WorkshopViewModel(IScreen hostScreens) : base(hostScreens, "workshop")
        {
            DisplayName = "Workshop";
        }
    }
}