using System.Windows.Media;
using MaterialDesignThemes.Wpf;

namespace Artemis.UI.Screens.Workshop
{
    public class WorkshopViewModel : MainScreenViewModel
    {
        public WorkshopViewModel()
        {
            DisplayName = "Workshop";
            DisplayIcon = PackIconKind.TestTube;
            DisplayOrder = 3;
        }

        public Color TestColor { get; set; }
        public bool TestPopupOpen { get; set; }

        public void UpdateValues()
        {
            TestPopupOpen = !TestPopupOpen;
            TestColor = Color.FromRgb(5, 174, 255);
        }
    }
}