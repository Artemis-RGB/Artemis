using System.Windows.Media;
using Stylet;

namespace Artemis.UI.Screens.Workshop
{
    public class WorkshopViewModel : MainScreenViewModel
    {
        private Color _testColor;
        private bool _testPopupOpen;

        public WorkshopViewModel()
        {
            DisplayName = "Workshop";
        }

        public Color TestColor
        {
            get => _testColor;
            set => SetAndNotify(ref _testColor, value);
        }

        public bool TestPopupOpen
        {
            get => _testPopupOpen;
            set => SetAndNotify(ref _testPopupOpen, value);
        }

        public void UpdateValues()
        {
            TestPopupOpen = !TestPopupOpen;
            TestColor = Color.FromRgb(5, 174, 255);
        }
    }
}