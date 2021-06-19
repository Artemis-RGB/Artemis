using Stylet;

namespace Artemis.UI.Screens
{
    public abstract class MainScreenViewModel : Screen
    {
        private Screen _headerViewModel;

        public Screen HeaderViewModel
        {
            get => _headerViewModel;
            set
            {
                if (!SetAndNotify(ref _headerViewModel, value)) return;
                _headerViewModel.ConductWith(this);
            }
        }
    }
}