using MaterialDesignThemes.Wpf;
using Stylet;

namespace Artemis.UI.Screens
{
    public abstract class MainScreenViewModel : Screen
    {
        public int DisplayOrder { get; set; }
        public PackIconKind DisplayIcon { get; set; }
    }
}