using System.Windows.Media;
using Stylet;

namespace Artemis.UI.Screens.Workshop
{
    public class WorkshopViewModel : Screen, IScreenViewModel
    {
        public Color TestColor { get; set; }
        public bool TestPopupOpen { get; set; }
        public string Title => "Workshop";

        public void UpdateValues()
        {
            TestPopupOpen = !TestPopupOpen;
            TestColor = Color.FromRgb(5, 174, 255);
        }
    }
}