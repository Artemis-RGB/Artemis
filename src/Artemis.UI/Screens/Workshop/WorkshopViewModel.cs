using System.Windows.Media;

namespace Artemis.UI.Screens.Workshop
{
    public class WorkshopViewModel : MainScreenViewModel
    {
        public WorkshopViewModel()
        {
            DisplayName = "Workshop";
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