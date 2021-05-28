using MaterialDesignThemes.Wpf;

namespace Artemis.UI.Screens.Sidebar.Dialogs
{
    public class IconViewModel
    {
        public IconViewModel(PackIconKind icon)
        {
            Icon = icon;
            IconName = icon.ToString();
        }

        public PackIconKind Icon { get; }
        public string IconName { get; }
    }
}