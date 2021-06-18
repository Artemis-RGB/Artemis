using MaterialDesignThemes.Wpf;
using Ninject;

namespace Artemis.UI.Screens.Sidebar
{
    public class SidebarScreenViewModel<T> : SidebarScreenViewModel where T : MainScreenViewModel
    {
        public SidebarScreenViewModel(PackIconKind icon, string displayName) : base(icon, displayName)
        {
        }

        public override MainScreenViewModel CreateInstance(IKernel kernel)
        {
            return kernel.Get<T>();
        }
    }

    public abstract class SidebarScreenViewModel
    {
        protected SidebarScreenViewModel(PackIconKind icon, string displayName)
        {
            Icon = icon;
            DisplayName = displayName;
        }

        public PackIconKind Icon { get; }
        public string DisplayName { get; }

        public abstract MainScreenViewModel CreateInstance(IKernel kernel);
    }
}