using MaterialDesignThemes.Wpf;
using Stylet;

namespace Artemis.UI.Screens.News
{
    public class NewsViewModel : MainScreenViewModel
    {
        public NewsViewModel()
        {
            DisplayName = "News";
            DisplayIcon = PackIconKind.Newspaper;
            DisplayOrder = 2;
        }
    }
}