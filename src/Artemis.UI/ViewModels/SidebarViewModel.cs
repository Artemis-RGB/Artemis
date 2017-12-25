using System.Collections.ObjectModel;
using Artemis.UI.ViewModels.Interfaces;
using ReactiveUI;

namespace Artemis.UI.ViewModels
{
    public class SidebarViewModel : ReactiveObject, ISidebarViewModel
    {
        public SidebarViewModel(IScreen screen)
        {
            HostScreen = screen;
            MenuItems = new ObservableCollection<IArtemisViewModel>();
        }

        public IScreen HostScreen { get; }
        public ObservableCollection<IArtemisViewModel> MenuItems { get; set; }
    }

    public interface ISidebarViewModel
    {
        ObservableCollection<IArtemisViewModel> MenuItems { get; set; }
    }
}